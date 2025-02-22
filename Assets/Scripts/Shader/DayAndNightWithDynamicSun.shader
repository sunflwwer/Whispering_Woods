Shader "Skybox/DayAndNightWithDynamicSun"
{
    Properties
    {
        _CubeMap1("CubeMap1", Cube) = "" {}
        _CubeMap2("CubeMap2", Cube) = "" {}
        _SunSize("Sun Size", Range(0.000000, 1.000000)) = 0.03
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
                float3 toSun : TEXCOORD1;  // Direction to the sun
            };

            samplerCUBE _CubeMap1;
            samplerCUBE _CubeMap2;
            float _SunSize;

            // Unity's built-in light direction
            float4 _LightDirection; // Light direction passed from Unity to the shader

            v2f vert(appdata v)
            {
                v2f o;
                o.texcoord = v.vertex.xyz; // Use the 3D vertex position as the cube map direction
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Calculate the direction to the sun
                o.toSun = normalize(_LightDirection.xyz);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 direction = normalize(i.texcoord); // Normalize the cube map direction

                // Sample the cube maps
                fixed4 tex1 = texCUBE(_CubeMap1, direction);
                fixed4 tex2 = texCUBE(_CubeMap2, direction);

                // Calculate sun intensity
                float3 lightDir = normalize(i.toSun); // Extract normalized light direction
                float sunDot = dot(direction, lightDir);
                float sunIntensity = smoothstep(_SunSize * 0.02, _SunSize * 0.01, 1.0 - sunDot);

                // Add glow effect to the sun
                float glow = smoothstep(_SunSize * 0.04, _SunSize * 0.005 * 0.5, 1.0 - sunDot) * 0.8;

                // Fixed sun color
                fixed4 sunColor = fixed4(1.0, 1.0, 1.0, 1.0) * (sunIntensity + glow); // Yellowish sun color with glow

                // Adjust blend based on the light direction
                float dynamicBlend = saturate(0.5 + 0.5 * lightDir.y); // Blend changes with light height

                // Combine sky colors and sun
                fixed4 skyColor = lerp(tex1, tex2, dynamicBlend);
                return max(skyColor, sunColor); // Ensure sun is always visible
            }
            ENDCG
        }
    }
}
