// Sombreado Lambert con color por vértice para los modelos voxel generados por código.
// El canal alfa del color de vértice marca la emisión: 1 = normal, 0 = brilla (ventanas, neones, farolillos).
Shader "SpanishDungeon/VoxelLit"
{
    Properties
    {
        _Color ("Tinte", Color) = (1,1,1,1)
        _EmissionBoost ("Intensidad de emisión", Range(0, 3)) = 1.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma target 3.0

        fixed4 _Color;
        half _EmissionBoost;

        struct Input
        {
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed3 c = IN.color.rgb * _Color.rgb;
            half e = 1.0 - IN.color.a;
            o.Albedo = c * (1.0 - e);
            o.Emission = c * e * _EmissionBoost;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
