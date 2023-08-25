
// Inspired from: https://forum.unity.com/threads/hue-saturation-brightness-contrast-shader.260649/
Shader "MATCH/Shaders/AdjustHSV"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Hue("Hue", Range(-360, 360)) = 0.
        _Brightness("Brightness", Range(-1, 1)) = 0.
        _Contrast("Contrast", Range(0, 2)) = 1
        _Saturation("Saturation", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        float _Hue;
        float _Brightness;
        float _Contrast;
        float _Saturation;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        inline float3 applyHue(float3 aColor, float aHue)
        {
            float angle = radians(aHue);
            float3 k = float3(0.57735, 0.57735, 0.57735);
            float cosAngle = cos(angle);
            //Rodrigues' rotation formula
            return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
        }
        inline float4 applyHSBEffect(float4 startColor)
        {
            float4 outputColor = startColor;
            outputColor.rgb = applyHue(outputColor.rgb, _Hue);
            outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast)+0.5f;
            outputColor.rgb = outputColor.rgb + _Brightness;
            float3 intensity = dot(outputColor.rgb, float3(0.299, 0.587, 0.114));
            outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
            return outputColor;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float4 temp;
            temp.rgb = c.rgb;
            o.Albedo = applyHSBEffect(temp);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }

        

        ENDCG
    }
    FallBack "Diffuse"
}
