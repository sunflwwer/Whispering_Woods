Shader "Skybox/DayAndNightWithDynamicSunAndMoonColorWithTransitions"
{
    Properties
    {
        _TexSunrise("Texture (Sunrise)", 2D) = "white" {} // 선라이즈 텍스처
        _TexDay("Texture (Day)", 2D) = "white" {} // 아침 텍스처
        _TexSunset("Texture (Sunset)", 2D) = "white" {} // 선셋 텍스처
        _TexNight("Texture (Night)", 2D) = "white" {} // 저녁 텍스처
        _SunSize("Sun Size", Range(0.000000, 1.000000)) = 0.03
        _MoonSize("Moon Size", Range(0.000000, 1.000000)) = 0.1
        _MoonColor("Moon Color", Color) = (1.0, 1.0, 0.8, 1.0) // 달 색상
        _TimeOfDay("Time of Day", Range(0.0, 1.0)) = 0.5 // 하루의 진행도 (0.0 = 밤, 0.5 = 낮, 1.0 = 다시 밤)
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
                float2 uv : TEXCOORD0;      // 2D 텍스처 좌표
                float4 vertex : SV_POSITION;
                float3 toSun : TEXCOORD1;  // 태양 방향
                float3 toMoon : TEXCOORD2; // 달 방향 (태양 반대편)
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
                o.uv = v.vertex.xy * 0.5 + 0.5; // 텍스처 좌표 변환
                o.vertex = UnityObjectToClipPos(v.vertex);

                // 태양과 달의 방향 계산
                o.toSun = normalize(float3(0.0, 1.0, 0.0)); // 임의의 태양 방향
                o.toMoon = -o.toSun; // 달은 태양 반대 방향

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 텍스처 샘플링
                fixed4 texSunrise = tex2D(_TexSunrise, i.uv);
                fixed4 texDay = tex2D(_TexDay, i.uv);
                fixed4 texSunset = tex2D(_TexSunset, i.uv);
                fixed4 texNight = tex2D(_TexNight, i.uv);

                // 하루의 진행도에 따라 텍스처 블렌딩
                float blendFactor;
                fixed4 skyColor;

                if (_TimeOfDay < 0.25) // 밤 -> 선라이즈
                {
                    blendFactor = _TimeOfDay / 0.25;
                    skyColor = BlendTextures(texNight, texSunrise, blendFactor);
                }
                else if (_TimeOfDay < 0.5) // 선라이즈 -> 아침
                {
                    blendFactor = (_TimeOfDay - 0.25) / 0.25;
                    skyColor = BlendTextures(texSunrise, texDay, blendFactor);
                }
                else if (_TimeOfDay < 0.75) // 아침 -> 선셋
                {
                    blendFactor = (_TimeOfDay - 0.5) / 0.25;
                    skyColor = BlendTextures(texDay, texSunset, blendFactor);
                }
                else // 선셋 -> 밤
                {
                    blendFactor = (_TimeOfDay - 0.75) / 0.25;
                    skyColor = BlendTextures(texSunset, texNight, blendFactor);
                }

                // 태양의 밝기 계산
                float3 lightDir = normalize(i.toSun);
                float sunDot = dot(float3(0, 0, -1), lightDir);
                float sunIntensity = smoothstep(_SunSize * 0.02, _SunSize * 0.003, 1.0 - sunDot);

                // 태양에 발광 효과 추가
                float glow = smoothstep(_SunSize * 0.04, _SunSize * 0.0000001, 1.0 - sunDot) * 0.4;

                // 고정된 태양 색상
                fixed4 sunColor = fixed4(1.0, 1.0, 1.0, 1.0) * (sunIntensity + glow);

                // 달의 밝기 계산 (태양 반대편)
                float3 moonDir = normalize(i.toMoon);
                float moonDot = dot(float3(0, 0, -1), moonDir);
                float moonIntensity = smoothstep(_MoonSize * 0.01, _MoonSize * 0.001, 1.0 - moonDot);

                // 사용자 정의 달 색상 적용
                fixed4 moonColor = _MoonColor * moonIntensity;

                // 태양과 달을 합성
                fixed4 finalColor = max(skyColor, sunColor); // 태양은 항상 보이게 하기
                finalColor = max(finalColor, moonColor); // 달도 보이게 하기

                return finalColor;
            }
            ENDCG
        }
    }
}
