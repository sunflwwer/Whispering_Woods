Shader "Skybox/DayAndNightCycleWithSmootherTransitions"
{
    Properties
    {
        _TexSunrise("Texture (Sunrise)", 2D) = "white" {}
        _TexDay("Texture (Day)", 2D) = "white" {}
        _TexSunset("Texture (Sunset)", 2D) = "white" {}
        _TexNight("Texture (Night)", 2D) = "white" {}
        _TimeOfDay("Time of Day", Range(0.0, 1.0)) = 0.5 // �Ϸ� ���൵ (0.0 = ��, 0.5 = ��, 1.0 = �ٽ� ��)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _TexSunrise;
            sampler2D _TexDay;
            sampler2D _TexSunset;
            sampler2D _TexNight;
            float _TimeOfDay;

            float4 BlendTextures(float4 tex1, float4 tex2, float blendFactor)
            {
                return lerp(tex1, tex2, blendFactor);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy * 0.5 + 0.5; // �ؽ�ó ��ǥ ��ȯ
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // �ؽ�ó ���ø�
                fixed4 texSunrise = tex2D(_TexSunrise, i.uv);
                fixed4 texDay = tex2D(_TexDay, i.uv);
                fixed4 texSunset = tex2D(_TexSunset, i.uv);
                fixed4 texNight = tex2D(_TexNight, i.uv);

                // �� �������� ���� ���� (0 ~ 1 ����)
                float sunriseDuration = 0.05; // ����: 5%
                float dayDuration = 0.7;     // ��: 70%
                float sunsetDuration = 0.05; // �ϸ�: 5%
                float nightDuration = 0.2;   // ��: 20%

                // �ð��� ���� ���� ���
                float blendFactor;
                fixed4 skyColor;

                if (_TimeOfDay < sunriseDuration) // �� -> ����
                {
                    blendFactor = _TimeOfDay / sunriseDuration;
                    skyColor = BlendTextures(texNight, texSunrise, blendFactor);
                }
                else if (_TimeOfDay < sunriseDuration + dayDuration) // ���� -> ��
                {
                    blendFactor = (_TimeOfDay - sunriseDuration) / dayDuration;
                    skyColor = BlendTextures(texSunrise, texDay, blendFactor);
                }
                else if (_TimeOfDay < sunriseDuration + dayDuration + sunsetDuration) // �� -> �ϸ�
                {
                    blendFactor = (_TimeOfDay - sunriseDuration - dayDuration) / sunsetDuration;
                    skyColor = BlendTextures(texDay, texSunset, blendFactor);
                }
                else // �ϸ� -> ��
                {
                    blendFactor = (_TimeOfDay - sunriseDuration - dayDuration - sunsetDuration) / nightDuration;
                    skyColor = BlendTextures(texSunset, texNight, blendFactor);
                }

                return skyColor;
            }
            ENDCG
        }
    }
}
