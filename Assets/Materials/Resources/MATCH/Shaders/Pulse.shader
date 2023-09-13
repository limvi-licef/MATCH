// Inspired by: https://www.youtube.com/watch?v=rmMfdyh1Ucs
Shader "MATCH/Shaders/Pulse"
{
    Properties
    {
        _ColorBase("Base color", Color) = (0.5,0.5,0.5,1)
        _ColorPulse("Pulse color", Color) = (0.5,0.5,0.5,1)
        _PulseDistance1("Pulse location 1", Range(0, 1)) = 0
        _PulseDistance2("Pulse location 2", Range(0, 1)) = 0.3
        _PulseDistance3("Pulse location 3", Range(0, 1)) = 0.6
        //_PulseDistance4("Pulse location 4", Range(0, 1)) = 0.9
        _PulseLength ("Pulse length", Range(0,1)) = 0.1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            float4 _ColorBase;
    float4 _ColorPulse;
    float _PulseDistance1;
    float _PulseDistance2;
    float _PulseDistance3;
    //float _PulseDistance4;
    float _PulseLength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _ColorBase;
                
            //if (i.uv.x > _PulseDistance-_PulseLength && i.uv.x < _PulseDistance+ _PulseLength)
            //if (i.uv.x < 0.5 && i.uv.x*0.5+ _PulseDistance > i.uv.y + _PulseDistance -_PulseLength && i.uv.x*0.5 + _PulseDistance < i.uv.y + _PulseDistance + _PulseLength)
            //if (i.uv.x > _PulseDistance && i.uv.x-_PulseDistance < 0.5 && (i.uv.x- _PulseDistance) * 0.2 > i.uv.y - _PulseLength && (i.uv.x- _PulseDistance) * 0.2 < i.uv.y + _PulseLength)
            /*if (i.uv.x > _PulseDistance - 0.1 && i.uv.x < _PulseDistance + 0.1 &&
                (i.uv.x-_PulseDistance)- _PulseLength < (i.uv.y - _PulseDistance) - _PulseLength &&
                (i.uv.x - _PulseDistance) + _PulseLength < (i.uv.y - _PulseDistance) + _PulseLength)*/
            /*if (i.uv.x > _PulseDistance - 0.1 && i.uv.x < _PulseDistance + 0.1 &&
                (i.uv.y - _PulseDistance) < 0.5 &&
                (i.uv.x- _PulseDistance) * 0.2 + _PulseDistance > i.uv.y + _PulseDistance - _PulseLength &&
                (i.uv.x- _PulseDistance) * 0.2 + _PulseDistance < i.uv.y + _PulseDistance + _PulseLength)
            {
                col = _ColorPulse;
                }*/

           /*if (i.uv.x > _PulseDistance - 0.1 && i.uv.x < _PulseDistance + 0.1 &&
                (i.uv.y - _PulseDistance) > 0.5 &&
                abs(i.uv.x- _PulseDistance-1) * 0.2 + _PulseDistance > i.uv.y + _PulseDistance - _PulseLength &&
                abs(i.uv.x- _PulseDistance-1) * 0.2 + _PulseDistance < i.uv.y + _PulseDistance + _PulseLength)
            {
                col = _ColorPulse;
            }*/

            if (i.uv.y < 0.5 &&
                i.uv.y > 0 &&
                i.uv.y * 0.05 >(i.uv.x - _PulseDistance1) - _PulseLength &&
                i.uv.y * 0.05 < (i.uv.x - _PulseDistance1) + _PulseLength)
            {
                col = _ColorPulse;
            }
            if (i.uv.y > 0.5 &&
                i.uv.y < 1 &&
                abs(i.uv.y - 1) * 0.05 > (i.uv.x - _PulseDistance1) - _PulseLength &&
                abs(i.uv.y - 1) * 0.05 < (i.uv.x - _PulseDistance1) + _PulseLength)
            {
                col = _ColorPulse;
            }

            if (i.uv.y < 0.5 &&
                i.uv.y > 0 &&
                i.uv.y * 0.05 >(i.uv.x - _PulseDistance2) - _PulseLength &&
                i.uv.y * 0.05 < (i.uv.x - _PulseDistance2) + _PulseLength)
            {
                col = _ColorPulse;
            }
            if (i.uv.y > 0.5 &&
                i.uv.y < 1 &&
                abs(i.uv.y - 1) * 0.05 > (i.uv.x - _PulseDistance2) - _PulseLength &&
                abs(i.uv.y - 1) * 0.05 < (i.uv.x - _PulseDistance2) + _PulseLength)
            {
                col = _ColorPulse;
            }

            if (i.uv.y < 0.5 &&
                i.uv.y > 0 &&
                i.uv.y * 0.05 >(i.uv.x - _PulseDistance3) - _PulseLength &&
                i.uv.y * 0.05 < (i.uv.x - _PulseDistance3) + _PulseLength)
            {
                col = _ColorPulse;
            }
            if (i.uv.y > 0.5 &&
                i.uv.y < 1 &&
                abs(i.uv.y - 1) * 0.05 > (i.uv.x - _PulseDistance3) - _PulseLength &&
                abs(i.uv.y - 1) * 0.05 < (i.uv.x - _PulseDistance3) + _PulseLength)
            {
                col = _ColorPulse;
            }
            
            /*if (i.uv.y < 0.5 &&
                i.uv.y > 0 &&
                i.uv.y * 0.05 >(i.uv.x - _PulseDistance4) - _PulseLength &&
                i.uv.y * 0.05 < (i.uv.x - _PulseDistance4) + _PulseLength)
            {
                col = _ColorPulse;
            }
            if (i.uv.y > 0.5 &&
                i.uv.y < 1 &&
                abs(i.uv.y - 1) * 0.05 > (i.uv.x - _PulseDistance4) - _PulseLength &&
                abs(i.uv.y - 1) * 0.05 < (i.uv.x - _PulseDistance4) + _PulseLength)
            {
                col = _ColorPulse;
            }*/

            return col;
    }

            ENDCG
        }
    }
}
