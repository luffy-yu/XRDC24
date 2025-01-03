Shader "Custom/SeeThroughWithEdges"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _EdgeColor("Edge Color", Color) = (1,0,0,1)
        _EdgeThickness("Edge Thickness", Range(0.1, 3.0)) = 1.0
        _Transparency("Transparency", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            fixed4 _BaseColor;
            fixed4 _EdgeColor;
            float _EdgeThickness;
            float _Transparency;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fresnel effect for edges
                float edgeFactor = pow(1.0 - dot(i.normal, i.viewDir), _EdgeThickness);

                // Base transparency
                fixed4 baseColor = _BaseColor;
                baseColor.a *= _Transparency;

                // Combine edge and base color
                fixed4 edgeColor = _EdgeColor * edgeFactor;
                return lerp(baseColor, edgeColor, edgeFactor);
            }
            ENDCG
        }
    }
}
