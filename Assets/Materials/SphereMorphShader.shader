Shader "URP/AudioReactiveSphere"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Frequency ("Frequency", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float _Frequency;

            struct Attributes
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float3 wPos : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 offsetV = v.pos.xyz * (1.0 + _Frequency);
                o.pos = TransformObjectToHClip(float4(offsetV, 1.0));
                o.wPos = TransformObjectToWorld(v.pos.xyz);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
