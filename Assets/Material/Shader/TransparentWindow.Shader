Shader "Custom/MakeTransparent"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200
            
            CGPROGRAM
            
            #pragma vertex VertexShaderFunction
            #pragma fragment PixelShaderFunction
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            
            struct v2f
            {
                float4 position: SV_POSITION;
                float2 uv: TEXCOORD0;
            };
            
            v2f VertexShaderFunction(appdata_img i)
            {
                v2f o;
                o.position = UnityObjectToClipPos(i.vertex);
                o.uv = i.texcoord;
                return o;
            }
            
            
            float4 PixelShaderFunction(v2f i): SV_Target
            {
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
                
                return lerp(tex2D(_MainTex, i.uv), float4(0, 0, 0, 0), step(0.9, depth));
            }
            ENDCG
            
        }
    }
}