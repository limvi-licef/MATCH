// Source: http://trolltungakreative.com/Unity/Gradient.shader
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MATCH/Shaders/Pulse_SurfaceShader"
{
	Properties
	{
		//_MainTex("Texture", 2D) = "white" {}
		_ColorMain("Main color", Color) = (1,1,1,1)
		_ColorPulse("Pulse color", Color) = (1,1,1,1)
		_Distance("Pulse relative location", Range(0,100)) = 50
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque"  }
		LOD 200

		CGPROGRAM

		float4 _ColorMain;
		float4 _ColorPulse;
		float _Distance;

#pragma surface surf Standard fullforwardshadows

#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
			float3 localPos: POSITION;
		};

		UNITY_INSTANCING_BUFFER_START(Props)

		UNITY_INSTANCING_BUFFER_END(Props)

			/*void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
		}*/

			void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed2 center = (0.5, 0.5);
			float d = distance(center, IN.localPos);

			fixed4 c = _ColorMain;
			c.rgb *= d;

			o.Albedo = _ColorPulse*IN.localPos;
			//o.Alpha = 1.0 - d;
		}

		ENDCG
	}
		FallBack "Diffuse"
}