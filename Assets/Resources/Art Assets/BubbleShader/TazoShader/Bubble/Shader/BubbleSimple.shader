Shader "Tazo/BubbleSimple"
{
    Properties
    {
        [Header(Mat)]
        _MColor("Mat Color", Color) = (0.5,0.5,0.5,1)
        [NoScaleOffset]_MatCap ("MatCap which has alpha (RGBA)", 2D) = "white" {}
        _powh("HightLight POW", Range(0.0, 20.0)) = 1
        [Header(Rim)]
        _RimColor("Rim Color", Color) = (1,1,1,1)
        rimWidth("rimWidth", Range(0.0, 2.0)) = 0.75
        _AlphaMode("Alpha", Range(0,1)) = 0.00
        [Header(Transparency)]
        _OverallAlpha("Overall Transparency", Range(0, 1)) = 1.0 // 新增透明度参数
    }

    SubShader
    {
        Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Pass
        {
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 cap : TEXCOORD0;
                float2 uv : TEXCOORD2;
                float4 uv_object : TEXCOORD1;
                fixed4 color : COLOR;
            };

            uniform float rimWidth;
            uniform float _AlphaMode;
            uniform float _OverallAlpha; // 新增全局透明度变量
            float _powh;
            uniform float4 _RimColor;
            uniform float4 _MColor;
            uniform sampler2D _MatCap;

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + 
                                             unity_WorldToObject[1].xyz * v.normal.y + 
                                             unity_WorldToObject[2].xyz * v.normal.z);
                worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float dotProduct = 1 - dot(v.normal, viewDir);
                o.color.r = smoothstep(1 - rimWidth, 1.0, dotProduct);
                o.color.g = dotProduct;
                o.cap.xy = worldNorm.xy * 0.5 + 0.5;
                o.cap.z = 1 - abs(v.normal.y);
                o.cap.w = v.color.a;

                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 mc1 = tex2D(_MatCap, i.cap.xy);
                fixed4 mc = saturate(mc1);
                fixed highlight = pow(mc.a, _powh);
                fixed4 cc = _MColor * mc * 2.7;

                cc.rgb = _MColor * mc.rgb + _RimColor.rgb * _RimColor.a * i.color.r + highlight;
                cc.a = lerp(cc.r, 1, _AlphaMode) * mc.r * i.cap.w + _RimColor.r * _RimColor.a * i.color.r + highlight;

                cc.a *= _OverallAlpha; // 应用全局透明度

                return cc;
            }
            ENDCG
        }
    }

    Fallback "VertexLit"
}
