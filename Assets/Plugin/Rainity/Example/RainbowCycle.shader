Shader "Custom/RainbowCycle" {
	Properties {
		_Speed("Speed", float) = 0.3
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		struct Input {
			fixed4 _yote;
		};

		float _Speed;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 _Color1 = fixed4(1.0, 0.0, 0.0, 1.0);
			fixed4 _Color2 = fixed4(0.0, 0.0, 1.0, 1.0);
			fixed4 c = lerp(_Color1, _Color2, abs(fmod(_Time.a * _Speed, 2.0) - 1.0));
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
