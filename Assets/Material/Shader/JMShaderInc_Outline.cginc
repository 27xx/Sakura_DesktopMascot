uniform float4 _LightColor0;
sampler2D _ColorMap;
fixed4 _ColorMapColor;
sampler2D _ShadowMap;
sampler2D _OutlineNormalMap;
fixed4 _ShadowMapColor;
float _Outline_Width;
float4 _Outline_Color;
float _Offset_Z;

struct VertexInput
{
    float4 vertex: POSITION;
    float3 normal: NORMAL;
    float4 tangent: TANGENT;
    float2 texcoord0: TEXCOORD0;
    float4 color: COLOR0;
};
struct VertexOutput
{
    float4 pos: SV_POSITION;
    float2 uv0: TEXCOORD0;
    float3 normalDir: TEXCOORD1;
    float3 tangentDir: TEXCOORD2;
    float3 bitangentDir: TEXCOORD3;
};

half3 UnpackMyNormal(half4 packednormal)
{
    #if defined(UNITY_NO_DXT5nm)
        half3 normal = packednormal.xyz * 2 - 1;
        return normal;
    #else
        packednormal.x *= packednormal.w;
        
        half3 normal;
        normal.xy = (packednormal.xy * 2 - 1);
        normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
        return normal;
    #endif
}

VertexOutput vert(VertexInput v)
{
    // 初始化
    VertexOutput o = (VertexOutput)0;
    if (_Outline_Width == 0)
        return o;
    
    o.uv0 = v.texcoord0;
    float2 Set_UV0 = o.uv0;
    o.normalDir = UnityObjectToWorldNormal(v.normal);
    o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
    o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
    float3x3 tangentTransform = float3x3(o.tangentDir, o.bitangentDir, o.normalDir);
    
    float3 _BakedNormal_var = normalize(UnpackMyNormal(tex2Dlod(_OutlineNormalMap, float4(Set_UV0, 0.0, 0))));
    float3 _BakedNormalDir = normalize(mul(_BakedNormal_var.rgb, tangentTransform));
    
    float4 _Outline_Sampler_var = tex2Dlod(_ColorMap, float4(Set_UV0, 0.0, 0));
    float Set_Outline_Width = _Outline_Width * 0.001;// * v.color.r;
    float4 _ClipCameraPos = mul(UNITY_MATRIX_VP, float4(_WorldSpaceCameraPos.xyz, 1));
    #if defined(UNITY_REVERSED_Z)
        //v.2.0.4.2 (DX)
        _Offset_Z = _Offset_Z * - 0.01;
    #else
        //OpenGL
        _Offset_Z = _Offset_Z * 0.01;
    #endif
    o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal * Set_Outline_Width, 1));
    o.pos.z = o.pos.z + _Offset_Z * _ClipCameraPos.z;
    return o;
}
float4 frag(VertexOutput i): SV_Target
{
    if (_Outline_Width == 0)
        clip(-1);
    
    float3 lightColor = _LightColor0.rgb;
    float2 Set_UV0 = i.uv0;
    float4 _MainTex_var = tex2D(_ColorMap, Set_UV0);
    float4 _ShadowMap_var = tex2D(_ShadowMap, Set_UV0);
    float3 Set_BaseColor = _ColorMapColor.rgb * _MainTex_var.rgb;
    float3 _Is_BlendBaseColor_var = _Outline_Color.rgb * pow(Set_BaseColor, 5) * lightColor;
    
    float3 Set_Outline_Color = _Is_BlendBaseColor_var;
    return float4(Set_Outline_Color, 1.0);
}
