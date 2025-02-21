Shader "Skybox/DayAndNightWithDynamicSunAndMoonColorWithTransitions"
{
    Properties
    {
        _TexSunrise("Texture (Sunrise)", 2D) = "white" {} // �������� �ؽ�ó
        _TexDay("Texture (Day)", 2D) = "white" {} // ��ħ �ؽ�ó
        _TexSunset("Texture (Sunset)", 2D) = "white" {} // ���� �ؽ�ó
        _TexNight("Texture (Night)", 2D) = "white" {} // ���� �ؽ�ó
        _SunSize("Sun Size", Range(0.000000, 1.000000)) = 0.03
        _MoonSize("Moon Size", Range(0.000000, 1.000000)) = 0.1
        _MoonColor("Moon Color", Color) = (1.0, 1.0, 0.8, 1.0) // �� ����
        _TimeOfDay("Time of Day", Range(0.0, 1.0)) = 0.5 // �Ϸ��� ���൵ (0.0 = ��, 0.5 = ��, 1.0 = �ٽ� ��)
    }
    SubShader
    {
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
                float2 uv : TEXCOORD0;      // 2D �ؽ�ó ��ǥ
                float4 vertex : SV_POSITION;
                float3 toSun : TEXCOORD1;  // �¾� ����
                float3 toMoon : TEXCOORD2; // �� ���� (�¾� �ݴ���)
            };

            sampler2D _TexSunrise;
            sampler2D _TexDay;
            sampler2D _TexSunset;
            sampler2D _TexNight;
            float _SunSize;
            float _MoonSize;
            fixed4 _MoonColor;
            float _TimeOfDay;

            float4 BlendTextures(float4 tex1, float4 tex2, float blendFactor)
            {
                return lerp(tex1, tex2, blendFactor);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.vertex.xy * 0.5 + 0.5; // �ؽ�ó ��ǥ ��ȯ
                o.vertex = UnityObjectToClipPos(v.vertex);

                // �¾�� ���� ���� ���
                o.toSun = normalize(float3(0.0, 1.0, 0.0)); // ������ �¾� ����
                o.toMoon = -o.toSun; // ���� �¾� �ݴ� ����

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // �ؽ�ó ���ø�
                fixed4 texSunrise = tex2D(_TexSunrise, i.uv);
                fixed4 texDay = tex2D(_TexDay, i.uv);
                fixed4 texSunset = tex2D(_TexSunset, i.uv);
                fixed4 texNight = tex2D(_TexNight, i.uv);

                // �Ϸ��� ���൵�� ���� �ؽ�ó ����
                float blendFactor;
                fixed4 skyColor;

                if (_TimeOfDay < 0.25) // �� -> ��������
                {
                    blendFactor = _TimeOfDay / 0.25;
                    skyColor = BlendTextures(texNight, texSunrise, blendFactor);
                }
                else if (_TimeOfDay < 0.5) // �������� -> ��ħ
                {
                    blendFactor = (_TimeOfDay - 0.25) / 0.25;
                    skyColor = BlendTextures(texSunrise, texDay, blendFactor);
                }
                else if (_TimeOfDay < 0.75) // ��ħ -> ����
                {
                    blendFactor = (_TimeOfDay - 0.5) / 0.25;
                    skyColor = BlendTextures(texDay, texSunset, blendFactor);
                }
                else // ���� -> ��
                {
                    blendFactor = (_TimeOfDay - 0.75) / 0.25;
                    skyColor = BlendTextures(texSunset, texNight, blendFactor);
                }

                // �¾��� ��� ���
                float3 lightDir = normalize(i.toSun);
                float sunDot = dot(float3(0, 0, -1), lightDir);
                float sunIntensity = smoothstep(_SunSize * 0.02, _SunSize * 0.003, 1.0 - sunDot);

                // �¾翡 �߱� ȿ�� �߰�
                float glow = smoothstep(_SunSize * 0.04, _SunSize * 0.0000001, 1.0 - sunDot) * 0.4;

                // ������ �¾� ����
                fixed4 sunColor = fixed4(1.0, 1.0, 1.0, 1.0) * (sunIntensity + glow);

                // ���� ��� ��� (�¾� �ݴ���)
                float3 moonDir = normalize(i.toMoon);
                float moonDot = dot(float3(0, 0, -1), moonDir);
                float moonIntensity = smoothstep(_MoonSize * 0.01, _MoonSize * 0.001, 1.0 - moonDot);

                // ����� ���� �� ���� ����
                fixed4 moonColor = _MoonColor * moonIntensity;

                // �¾�� ���� �ռ�
                fixed4 finalColor = max(skyColor, sunColor); // �¾��� �׻� ���̰� �ϱ�
                finalColor = max(finalColor, moonColor); // �޵� ���̰� �ϱ�

                return finalColor;
            }
            ENDCG
        }
    }
}
