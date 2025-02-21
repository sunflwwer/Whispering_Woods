
Shader "Skybox/DayAndNightWithDynamicSunAndMoon"
{
    Properties
    {
        _CubeMap1("CubeMap1", Cube) = "" {}
        _CubeMap2("CubeMap2", Cube) = "" {}
        _SunSize("Sun Size", Range(0.000000, 1.000000)) = 0.03
        _MoonSize("Moon Size", Range(0.000000, 1.000000)) = 0.1
        _MoonColor("Moon Color", Color) = (1.0, 1.0, 0.8, 1.0) // 달 색상
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
                float3 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 toSun : TEXCOORD1;  // 태양 방향
                float3 toMoon : TEXCOORD2; // 달 방향 (태양 반대편)
            };

            samplerCUBE _CubeMap1;
            samplerCUBE _CubeMap2;
            float _SunSize;
            float _MoonSize;
            float4 _MoonColor; // 달 색상

            // Unity의 빛 방향 (태양 방향)
            float4 _LightDirection;

            v2f vert(appdata v)
            {
                v2f o;
                o.texcoord = v.vertex.xyz; // 3D 버텍스를 큐브맵 방향으로 사용
                o.vertex = UnityObjectToClipPos(v.vertex);

                // 태양과 달의 방향 계산
                o.toSun = normalize(_LightDirection.xyz);
                o.toMoon = -o.toSun; // 달은 태양 반대 방향

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 direction = normalize(i.texcoord); // 큐브맵 방향을 정규화

                // 큐브맵 샘플링
                fixed4 tex1 = texCUBE(_CubeMap1, direction);
                fixed4 tex2 = texCUBE(_CubeMap2, direction);

                // 태양의 밝기 계산
                float3 lightDir = normalize(i.toSun); // 정규화된 빛 방향
                float sunDot = dot(direction, lightDir);
                float sunIntensity = smoothstep(_SunSize * 0.02, _SunSize * 0.003, 1.0 - sunDot);

                // 태양에 발광 효과 추가
                float glow = smoothstep(_SunSize * 0.04, _SunSize * 0.0000001, 1.0 - sunDot) * 0.4;

                // 고정된 태양 색상
                fixed4 sunColor = fixed4(1.0, 1.0, 1.0, 1.0) * (sunIntensity + glow); // 노란 태양 색상 + 발광

                // 달의 밝기 계산 (태양 반대편)
                float3 moonDir = normalize(i.toMoon); // 달의 방향
                float moonDot = dot(direction, moonDir);
                float moonIntensity = smoothstep(_MoonSize * 0.01, _MoonSize * 0.001, 1.0 - moonDot);

                // 달 색상
                fixed4 moonColor = _MoonColor * moonIntensity;

                // 빛의 방향에 따른 블렌드 조정
                float dynamicBlend = saturate(0.5 + 0.5 * lightDir.y); // 빛의 높이에 따라 블렌드 변화

                // 하늘 색상과 태양을 합성
                fixed4 skyColor = lerp(tex1, tex2, dynamicBlend);
                fixed4 finalColor = max(skyColor, sunColor); // 태양은 항상 보이게 하기
                finalColor = max(finalColor, moonColor); // 달도 보이게 하기

                return finalColor;
            }
            ENDCG
        }
    }
}