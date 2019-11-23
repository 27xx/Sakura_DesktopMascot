Shader "Jason Ma/ToonShader"
{
    Properties
    {
        [Enum(OFF, 0, FRONT, 1, BACK, 2)] _CullMode ("Cull Mode：裁剪", int) = 2  //OFF/FRONT/BACK
        
        [Header(Base Color)]
        [NoScaleOffset] _ColorMap ("ColorMap (RGB)", 2D) = "white" { }
        _ColorMapColor ("Color", Color) = (1, 1, 1, 1)
        
        [Header(Normal)]
        [NoScaleOffset][Normal] _NormalMap ("NormalMap", 2D) = "bump" { }
        _NormalScale ("Normal Scale：深度", Range(0, 1)) = 1
        [NoScaleOffset][Normal] _OutlineNormalMap ("Outline NormalMap", 2D) = "bump" { }
        
        [Header(Shadow)]
        [NoScaleOffset] _ShadowMap ("ShadowMap (RGB)", 2D) = "white" { }
        _ShadowMapColor ("ShadowMap Color", Color) = (1, 1, 1, 1)
        _ShadowIntensity ("Int：强度", Range(0, 1)) = 1
        _Shadow_Step ("Step：阈值", Range(0, 1)) = 0.5
        [PowerSlider(6)] _Shadow_Feather ("Feather：羽化", Range(0.0001, 1)) = 0.0001
        [PowerSlider(1.7)] _Shadow_Purity ("Purity：纯度缩放", Range(-2, 2)) = 0
        [PowerSlider(1.7)] _Shadow_Lightness ("Lightness：明度缩放", Range(-2, 2)) = 0
        [PowerSlider(1.7)] _Shadow_Level ("Level", Range(-1, 1)) = 0
        
        [Header(High Light)]
        [NoScaleOffset] _LightMap ("LightMap (RGBA)", 2D) = "white" { }
        [HDR] _HighColor ("High Color", Color) = (1, 1, 1, 1)
        _Is_SpecularToHighColor ("Specular：混合高光算法", Range(0, 1)) = 0
        [PowerSlider(2)] _HighColor_Power ("Power：范围", Range(0, 1)) = 0
        _HighColorIntensity ("Int：强度", Range(0, 1)) = 1
        [PowerSlider(2)] _HighColorLevel ("Level：偏移", Range(-1, 1)) = 0
        [PowerSlider(2)] _TweakHighColorOnShadow ("Int On Shadow：阴影中强度", Range(0, 1)) = 0
        
        [Header(Anisotropy High Light)]
        _MatCap_Sampler ("Anisotropy Map (R)：各向异性形状", 2D) = "black" { }
        [HDR] _MatCapColor ("Anisotropy Color", Color) = (1, 1, 1, 1)
        _MatCap ("Int：强度", Range(0, 1)) = 1
        [PowerSlider(2)] _TweakMatCapOnShadow ("Int On Shadow：阴影中强度", Range(0, 1)) = 0.25
        [PowerSlider(2)] _Tweak_MatcapMaskLevel ("Level：强度偏移", Range(-1, 1)) = 0
        _BumpScaleMatcap ("Normal Int：法线强度", Range(0, 1)) = 1
        [PowerSlider(2)] _BlurLevelMatcap ("Blur Level：模糊", Range(0, 10)) = 0
        
        [Header(RimLight)]
        [HDR] _RimLightColor ("RimLight Color：边缘光", Color) = (0, 0, 0, 1)
        _RimLightIntensity ("Int：强度", Range(0, 1)) = 0
        [PowerSlider(8)] _RimLightFeather ("Feather：羽化", Range(0, 1)) = 0.005
        [PowerSlider(1.5)] _RimLightWidth ("Width：宽度", Range(0, 1)) = 0.3
        [PowerSlider(0.35)] _RimLightLength ("Length：长度", Range(0, 10)) = 7
        [PowerSlider(2)] _RimLightLevel ("Level：强度偏移", Range(-1, 1)) = 0
                
        
        [Header(OutLine)]
        _Outline_Color ("Outline Color", Color) = (0.5, 0.5, 0.5, 1)
        [PowerSlider(2)] _Outline_Width ("Width：宽度", Range(0, 30)) = 0
        [PowerSlider(2)] _Offset_Z ("Offset Z：深度偏移", Range(0, 100)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        
        Pass
        {
            Name "FORWARD"
            Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
            Cull[_CullMode]
            LOD 100
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _IS_PASS_FWDBASE
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            
            #include "JMShaderInc.cginc"
            
            ENDCG
            
        }
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            Cull[_CullMode]
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            //for Unity2018.x
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma target 3.0
            
            #pragma multi_compile _IS_PASS_FWDDELTA
            
            #include "JMShaderInc.cginc"
            
            ENDCG
            
        }
        
        Pass
        {
            Name "Outline"
            Tags {  }
            Cull Front
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0
            
            
            #include "JMShaderInc_Outline.cginc"
            
            ENDCG
            
        }
    }
    FallBack "Legacy Shaders/VertexLit"
}
