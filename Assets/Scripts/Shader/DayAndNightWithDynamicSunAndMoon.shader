
Shader "Skybox/DayAndNightWithDynamicSunAndMoon"
{
    Properties
    {
        _CubeMap1("CubeMap1", Cube) = "" {}
        _CubeMap2("CubeMap2", Cube) = "" {}
        _SunSize("Sun Size", Range(0.000000, 1.000000)) = 0.03
        _MoonSize("Moon Size", Range(0.000000, 1.000000)) = 0.1
        _MoonColor("Moon Color", Color) = (1.0, 1.0, 0.8, 1.0) // �� ����
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
                float3 toSun : TEXCOORD1;  // �¾� ����
                float3 toMoon : TEXCOORD2; // �� ���� (�¾� �ݴ���)
            };

            samplerCUBE _CubeMap1;
            samplerCUBE _CubeMap2;
            float _SunSize;
            float _MoonSize;
            float4 _MoonColor; // �� ����

            // Unity�� �� ���� (�¾� ����)
            float4 _LightDirection;

            v2f vert(appdata v)
            {
                v2f o;
                o.texcoord = v.vertex.xyz; // 3D ���ؽ��� ť��� �������� ���
                o.vertex = UnityObjectToClipPos(v.vertex);

                // �¾�� ���� ���� ���
                o.toSun = normalize(_LightDirection.xyz);
                o.toMoon = -o.toSun; // ���� �¾� �ݴ� ����

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 direction = normalize(i.texcoord); // ť��� ������ ����ȭ

                // ť��� ���ø�
                fixed4 tex1 = texCUBE(_CubeMap1, direction);
                fixed4 tex2 = texCUBE(_CubeMap2, direction);

                // �¾��� ��� ���
                float3 lightDir = normalize(i.toSun); // ����ȭ�� �� ����
                float sunDot = dot(direction, lightDir);
                float sunIntensity = smoothstep(_SunSize * 0.02, _SunSize * 0.003, 1.0 - sunDot);

                // �¾翡 �߱� ȿ�� �߰�
                float glow = smoothstep(_SunSize * 0.04, _SunSize * 0.0000001, 1.0 - sunDot) * 0.4;

                // ������ �¾� ����
                fixed4 sunColor = fixed4(1.0, 1.0, 1.0, 1.0) * (sunIntensity + glow); // ��� �¾� ���� + �߱�

                // ���� ��� ��� (�¾� �ݴ���)
                float3 moonDir = normalize(i.toMoon); // ���� ����
                float moonDot = dot(direction, moonDir);
                float moonIntensity = smoothstep(_MoonSize * 0.01, _MoonSize * 0.001, 1.0 - moonDot);

                // �� ����
                fixed4 moonColor = _MoonColor * moonIntensity;

                // ���� ���⿡ ���� ���� ����
                float dynamicBlend = saturate(0.5 + 0.5 * lightDir.y); // ���� ���̿� ���� ���� ��ȭ

                // �ϴ� ����� �¾��� �ռ�
                fixed4 skyColor = lerp(tex1, tex2, dynamicBlend);
                fixed4 finalColor = max(skyColor, sunColor); // �¾��� �׻� ���̰� �ϱ�
                finalColor = max(finalColor, moonColor); // �޵� ���̰� �ϱ�

                return finalColor;
            }
            ENDCG
        }
    }
}