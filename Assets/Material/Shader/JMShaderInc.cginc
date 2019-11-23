fixed4 _ColorMapColor;
fixed4 _Color1;
fixed4 _Color2;
fixed4 _Color3;
sampler2D _ColorMap;
// float4 _ColorMap_ST;
sampler2D _NormalMap;
float _NormalScale;
float _ShadowIntensity;
sampler2D _ShadowMap;
fixed4 _ShadowMapColor;
float _Shadow_Step;
float _Shadow_Feather;
float _Shadow_Lightness;
float _Shadow_Purity;
float _Shadow_Level;

sampler2D _LightMap;
fixed4 _HighColor;
float _HighColor_Power;
float _HighColorIntensity;
float _TweakHighColorOnShadow;
float _HighColorLevel;
float _Is_SpecularToHighColor;
sampler2D _MatCap_Sampler;
float4 _MatCap_Sampler_ST;
fixed4 _MatCapColor;
float _MatCap;
float _TweakMatCapOnShadow;
float _Tweak_MatcapMaskLevel;
float _BumpScaleMatcap;
float _BlurLevelMatcap;

fixed4 _RimLightColor;
float _RimLightIntensity;
float _RimLightFeather;
float _RimLightLength;
float _RimLightWidth;
float _RimLightLevel;

float _Emissive_Enable;
sampler2D _Emissive_Mask1;
float4 _Emissive_Mask1_ST;
fixed4 _Emissive_ColorA1;
fixed4 _Emissive_ColorB1;
fixed _Emissive_IntA1;
fixed _Emissive_IntB1;
fixed _Emissive_Level1;
float _Emissive_MoveHor1;
float _Emissive_MoveVer1;
float _Emissive_Speed1;
sampler2D _Emissive_Mask2;
float4 _Emissive_Mask2_ST;
fixed4 _Emissive_ColorA2;
fixed4 _Emissive_ColorB2;
fixed _Emissive_IntA2;
fixed _Emissive_IntB2;
fixed _Emissive_Level2;
float _Emissive_MoveHor2;
float _Emissive_MoveVer2;
float _Emissive_Speed2;

float _Outline_Width;
fixed4 _Outline_Color;
float _Offset_Z;

struct VertexInput
{
    float4 vertex: POSITION;
    float3 normal: NORMAL;
    float4 tangent: TANGENT;
    float2 texcoord0: TEXCOORD0;
    float2 texcoord1: TEXCOORD1;
    float4 color: COLOR0;
};

struct VertexOutput
{
    float4 pos: SV_POSITION;
    float4 uv0: TEXCOORD0;
    float4 posWorld: TEXCOORD1;
    float3 normalDir: TEXCOORD2;
    float3 tangentDir: TEXCOORD3;
    float3 bitangentDir: TEXCOORD4;
    float mirrorFlag: TEXCOORD5;
    LIGHTING_COORDS(6, 7)
    UNITY_FOG_COORDS(8)
};


// UV回転をする関数：RotateUV()
float2 RotateUV(float2 _uv, float _radian, float2 _piv, float _time)
{
    float RotateUV_ang = _radian;
    float RotateUV_cos = cos(_time * RotateUV_ang);
    float RotateUV_sin = sin(_time * RotateUV_ang);
    return(mul(_uv - _piv, float2x2(RotateUV_cos, -RotateUV_sin, RotateUV_sin, RotateUV_cos)) + _piv);
}

fixed3 DecodeLightProbe(fixed3 N)
{
    return ShadeSH9(float4(N, 1));
}

float3 RGB2HSV(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
    
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSV2RGB(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float3 ChooseColor(float step, fixed4 colorOrigin, fixed4 color1, fixed4 color2, fixed4 color3)
{
    if (step <= 0.3) return lerp(colorOrigin.rgb, color1.rgb, smoothstep(0, 0.3, step));
    else if(step <= 0.6) return lerp(colorOrigin.rgb, color2.rgb, smoothstep(0.3, 0.6, step));
    else if(step <= 0.9) return lerp(colorOrigin.rgb, color3.rgb, smoothstep(0.6, 0.9, step));
    else return colorOrigin.rgb;
}

float StrandSpecular(float3 T, float3 V, float3 L, float exponent, float strength)
{
    float3 H = normalize(L + V);
    float dotTH = dot(T, H);
    float sinTH = sqrt(1 - dotTH * dotTH);
    float dirAtten = smoothstep(-1, 0, dotTH);
    return dirAtten * pow(sinTH, exponent) * strength;
}

float3 ShiftTangent(float3 T, float3 S, float shift, float step)
{
    float3 shiftedT = T + (shift - step) * S;
    return normalize(shiftedT);
}

VertexOutput vert(VertexInput v)
{
    // o.uv = TRANSFORM_TEX(v.uv, _ColorMap);
    VertexOutput o = (VertexOutput)0;
    o.uv0 = float4(v.texcoord0.xy, v.texcoord1.xy);
    o.normalDir = UnityObjectToWorldNormal(v.normal);
    o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
    o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
    o.posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.pos = UnityObjectToClipPos(v.vertex);
    float3 crossFwd = cross(UNITY_MATRIX_V[0], UNITY_MATRIX_V[1]);
    o.mirrorFlag = dot(crossFwd, UNITY_MATRIX_V[2]) < 0 ? 1: - 1;
    
    UNITY_TRANSFER_FOG(o, o.pos);
    TRANSFER_VERTEX_TO_FRAGMENT(o);
    return o;
}

fixed4 frag(VertexOutput i, fixed facing: VFACE): SV_Target
{
    i.normalDir = normalize(i.normalDir);
    
    #ifdef _IS_HAIRMODE
        // 正常UV，采样固有色和LightMap
        float2 Set_UV0 = i.uv0.zw;
        // 拉直后UV，采样噪声、法线
        float2 Set_UV1 = i.uv0.xy;
    #else
        float2 Set_UV0 = i.uv0.xy;
    #endif
    
    float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
    
    #ifdef _IS_HAIRMODE
        float3 _NormalMap_var = UnpackScaleNormal(tex2D(_NormalMap, Set_UV1), _NormalScale);
    #else
        float3 _NormalMap_var = UnpackScaleNormal(tex2D(_NormalMap, Set_UV0), _NormalScale);
    #endif
    float3 normalLocal = _NormalMap_var.rgb;
    // 扰动后的法线
    float3 normalDirection = normalize(mul(normalLocal, tangentTransform));
    
    
    // 固有色
    float4 _MainTex_var = tex2D(_ColorMap, Set_UV0);
    
    // 光照
    UNITY_LIGHT_ATTENUATION(attenuation, i, i.posWorld.xyz);
    
    float3 lightColor = 0;
    #ifdef _IS_PASS_FWDBASE
        float3 defaultLightDirection = normalize(UNITY_MATRIX_V[2].xyz + UNITY_MATRIX_V[1].xyz);
        float3 lightDirection = normalize(lerp(defaultLightDirection, _WorldSpaceLightPos0.xyz, any(_WorldSpaceLightPos0.xyz)));
        lightColor = _LightColor0.rgb;
    #elif _IS_PASS_FWDDELTA
        //                                                                                                                  0：平行光 1 ：其他
        float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz, _WorldSpaceLightPos0.w));
        float3 addPassLightColor = (0.5 * dot(normalDirection, lightDirection) + 0.5) * _LightColor0.rgb * attenuation;
        lightColor = max(0, addPassLightColor);
    #endif
    
    
    // 半角向量
    float3 halfDirection = normalize(viewDirection + lightDirection);
    
    float4 _ShadowMap_var = tex2D(_ShadowMap, Set_UV0);
    float4 _LightMap_var = tex2D(_LightMap, Set_UV0);
    
   
    
    // 平行光颜色
    float3 Set_LightColor = lightColor.rgb;
    // 亮面颜色
    float3 Set_BaseColor = (_ColorMapColor.rgb * _MainTex_var.rgb) * Set_LightColor;
    
    // 根据shadowMap调整阴影颜色
    float3 baseColor = RGB2HSV(Set_BaseColor);
    #ifdef _IS_HAIRMODE
        baseColor.gb -= float2(-0.2 * _Shadow_Purity, 0.2 * _Shadow_Lightness);
    #else
        baseColor.gb += float2(_Shadow_Purity * _ShadowMap_var.r, _Shadow_Lightness * _ShadowMap_var.b);
    #endif
    // 暗面颜色
    float3 Set_ShadowColor = lerp(HSV2RGB(saturate(baseColor)) * _ShadowMapColor.rgb, Set_BaseColor.rgb, 1 - _ShadowIntensity);
    
    // 半兰伯特 暗 0 ~ 1 亮
    float _HalfLambert_var = 0.5 * dot(normalDirection, lightDirection) + 0.5;
    
    //                                 阴影 -0.5 ~ 0.5 亮面                                  / 锐利 0.0001 ~ 1 平滑
    float shadowStep = saturate(1.0 + (_HalfLambert_var - (_Shadow_Step - _Shadow_Feather)) / - _Shadow_Feather);
    
    float Set_FinalShadowMask = max(shadowStep, 1 - _ShadowMap_var.g + _Shadow_Level);
    
    // 最终固有色
    float3 Set_FinalBaseColor = lerp(Set_BaseColor, Set_ShadowColor, Set_FinalShadowMask); // Final Color
    
    #ifdef _IS_PASS_FWDBASE
        #ifdef _IS_HAIRMODE
            float3 Set_HighColor = 0;
        #else
            float _Specular_var = 0.5 * dot(halfDirection, normalDirection) + 0.5; //  Specular
            // 高光范围
            float _TweakHighColorMask_var = ((_LightMap_var.b + _HighColorLevel) * lerp((1.0 - step(_Specular_var, (1.0 - pow(_HighColor_Power, 5)))), pow(_Specular_var, exp2(lerp(11, 1, _HighColor_Power))), _Is_SpecularToHighColor));
            
            float3 _HighColor_var = _HighColor.rgb * Set_LightColor * _TweakHighColorMask_var;
            
            // 最终高光
            float3 Set_HighColor = _HighColorIntensity * _HighColor_var * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakHighColorOnShadow));
        #endif
        
        // 边缘光
        float rimDot = saturate(1 - dot(viewDirection, normalDirection));
        float rimDot_X_lightDot = rimDot * pow(_HalfLambert_var, 10 - _RimLightLength);
        float rimIntensity = smoothstep((1 - _RimLightWidth) - _RimLightFeather, (1 - _RimLightWidth) + _RimLightFeather, rimDot_X_lightDot) * _ShadowMap_var.b;
        
        #ifdef _IS_HAIRMODE
            fixed4 final_rimColor = _RimLightColor * rimIntensity * _RimLightIntensity + _RimLightLevel;
        #else
            fixed4 final_rimColor = _RimLightColor * rimIntensity * _RimLightIntensity * _LightMap_var.g + _RimLightLevel;
        #endif
        
        // 各向异性高光
        #ifdef _IS_HAIRMODE
            fixed _NoiseMap_var = tex2Dlod(_NoiseMap, float4(TRANSFORM_TEX(Set_UV1, _NoiseMap), 0.0, _HighLit_LOD)).r;
            float3 HighLit_BT = ShiftTangent(i.bitangentDir, _HighLit_Scale, _NoiseMap_var, _HighLit_ScaleStep);
            fixed3 HighLit_color = _HighLit_Color.rgb * _LightMap_var.r * StrandSpecular(HighLit_BT, viewDirection, lightDirection, _HighLit_Range, _HighLit_Intensity + _HighLit_Level);
            fixed3 LowLit_color = _LowLit_Color.rgb * _LightMap_var.g * StrandSpecular(i.bitangentDir, viewDirection, lightDirection, _LowLit_Range, _LowLit_Intensity + _LowLit_Level);
            
            fixed3 matCapColorFinal = saturate(max(HighLit_color, LowLit_color)) * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakMatCapOnShadow));
        #else
            //鏡スクリプト判定：_sign_Mirror = -1 なら、鏡の中と判定.
            fixed _sign_Mirror = i.mirrorFlag;
            float3 _Camera_Right = UNITY_MATRIX_V[0].xyz;
            float3 _Camera_Front = UNITY_MATRIX_V[2].xyz;
            float3 _Up_Unit = float3(0, 1, 0);
            // 垂直于世界 +Y 和 摄像机正方向 的轴？？？
            float3 _Right_Axis = cross(_Camera_Front, _Up_Unit);
            //鏡の中なら反転.
            _Right_Axis *= _sign_Mirror < 0 ? - 1: 1;
            
            float _Camera_Right_Magnitude = length(_Camera_Right);
            float _Right_Axis_Magnitude = length(_Right_Axis);
            float _Camera_Roll_Cos = dot(_Right_Axis, _Camera_Right) / (_Right_Axis_Magnitude * _Camera_Right_Magnitude);
            float _Camera_Roll = acos(clamp(_Camera_Roll_Cos, -1, 1));
            fixed _Camera_Dir = _Camera_Right.y < 0 ? - 1: 1;
            float _Rot_MatCapUV_var_ang = - (_Camera_Dir * _Camera_Roll);
            
            float3 _NormalMapForMatCap_var = UnpackScaleNormal(tex2D(_NormalMap, Set_UV0), _BumpScaleMatcap);
            //v.2.0.5: MatCap with camera skew correction
            float3 viewNormal = (mul(UNITY_MATRIX_V, float4(mul(_NormalMapForMatCap_var.rgb, tangentTransform).rgb, 0))).rgb;
            float3 NormalBlend_MatcapUV_Detail = viewNormal.rgb * float3(-1, -1, 1);
            float3 NormalBlend_MatcapUV_Base = (mul(UNITY_MATRIX_V, float4(viewDirection, 0)).rgb * float3(-1, -1, 1)) + float3(0, 0, 1);
            // 修正摄像机旋转后
            float3 noSknewViewNormal = NormalBlend_MatcapUV_Base * dot(NormalBlend_MatcapUV_Base, NormalBlend_MatcapUV_Detail) / NormalBlend_MatcapUV_Base.b - NormalBlend_MatcapUV_Detail;
            float2 _ViewNormalAsMatCapUV = (noSknewViewNormal.rg * 0.5) + 0.5;
            float2 _Rot_MatCapUV_var = RotateUV(_ViewNormalAsMatCapUV, _Rot_MatCapUV_var_ang, float2(0.5, 0.5), 1.0);
            
            //鏡の中ならUV左右反転.
            if (_sign_Mirror < 0)
            {
                _Rot_MatCapUV_var.x = 1 - _Rot_MatCapUV_var.x;
            }
            //v.2.0.6 : LOD of Matcap
            float4 _MatCap_Sampler_var = tex2Dlod(_MatCap_Sampler, float4(TRANSFORM_TEX(_Rot_MatCapUV_var, _MatCap_Sampler), 0.0, _BlurLevelMatcap));
            //MatcapMask
            float _Set_MatcapMask_var = _LightMap_var.r;
            float _Tweak_MatcapMaskLevel_var = saturate(_Set_MatcapMask_var + _Tweak_MatcapMaskLevel);
            
            // 高光颜色
            float3 _Is_LightColor_MatCap_var = (_MatCap_Sampler_var.rgb * _MatCapColor.rgb) * Set_LightColor;
            // 调整阴影中强度
            float3 Set_MatCap = _Is_LightColor_MatCap_var * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakMatCapOnShadow));//+ lerp(Set_HighColor * Set_FinalShadowMask * (1.0 - _TweakMatCapOnShadow), float3(0.0, 0.0, 0.0), _Is_BlendAddToMatCap));
            float3 matCapColorFinal = Set_MatCap * _Tweak_MatcapMaskLevel_var * _MatCap;
        #endif
        
        float3 finalLightColor = max(max(matCapColorFinal, final_rimColor), Set_HighColor);// Final Composition before Emissive
        
        // 自发光
        fixed3 finalEmiColor = 0;
        #ifdef _IS_HAIRMODE
        #else
            if (_Emissive_Enable > 0)
            {
                float4 emiUV = float4(_Time.y * _Emissive_MoveHor1, _Time.y * _Emissive_MoveVer1, _Time.y * _Emissive_MoveHor2, _Time.y * _Emissive_MoveVer2);
                fixed emiMask1 = tex2D(_Emissive_Mask1, TRANSFORM_TEX(Set_UV0, _Emissive_Mask1) + emiUV.xy).r * (smoothstep(0.55, 1, _LightMap_var.a) + _Emissive_Level1);
                fixed emiMask2 = tex2D(_Emissive_Mask2, TRANSFORM_TEX(Set_UV0, _Emissive_Mask2) + emiUV.zw).r * (smoothstep(0.45, 0, _LightMap_var.a) + _Emissive_Level2);
                fixed3 emiColor1 = emiMask1 * lerp(_Emissive_ColorA1 * _Emissive_IntA1, _Emissive_ColorB1 * _Emissive_IntB1, sin(_Emissive_Speed1 * _Time.g));
                fixed3 emiColor2 = emiMask2 * lerp(_Emissive_ColorA2 * _Emissive_IntA2, _Emissive_ColorB2 * _Emissive_IntB2, sin(_Emissive_Speed2 * _Time.g));
                finalEmiColor = max(emiColor1, emiColor2);
            }
        #endif
        
        // 环境
        float3 envLightColor = DecodeLightProbe(normalDirection) < float3(1, 1, 1) ? DecodeLightProbe(normalDirection): float3(1, 1, 1);
        float envLightIntensity = 0.299 * envLightColor.r + 0.587 * envLightColor.g + 0.114 * envLightColor.b < 1 ?(0.299 * envLightColor.r + 0.587 * envLightColor.g + 0.114 * envLightColor.b): 1;
        
        
        fixed4 finalCol = 1;
        finalCol.rgb = saturate(Set_FinalBaseColor) + saturate(finalLightColor + finalEmiColor) + saturate(envLightColor * envLightIntensity * 0);
        
    #elif _IS_PASS_FWDDELTA
        float _LightIntensity = lerp(0, (0.299 * _LightColor0.r + 0.587 * _LightColor0.g + 0.114 * _LightColor0.b) * attenuation, _WorldSpaceLightPos0.w) ;
        
        float3 finalColor = Set_FinalBaseColor; // Final Color
        
        #ifdef _IS_HAIRMODE
        #else
            float _Specular_var = 0.5 * dot(halfDirection, normalDirection) + 0.5; //  Specular
            float _TweakHighColorMask_var = (saturate((_LightMap_var.b + _HighColorLevel)) * lerp((1.0 - step(_Specular_var, (1.0 - pow(_HighColor_Power, 5)))), pow(_Specular_var, exp2(lerp(11, 1, _HighColor_Power))), _Is_SpecularToHighColor));
            float3 _HighColor_var = _HighColor.rgb * Set_LightColor * _TweakHighColorMask_var * _HighColorIntensity;
            finalColor = finalColor + (_HighColor_var * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakHighColorOnShadow)));
            
            finalColor = saturate(finalColor);
        #endif
        
        fixed4 finalCol = 1;
        finalCol.rgb = finalColor;
    #endif
    
    
    
    UNITY_APPLY_FOG(i.fogCoord, finalCol);
    return finalCol;
}
