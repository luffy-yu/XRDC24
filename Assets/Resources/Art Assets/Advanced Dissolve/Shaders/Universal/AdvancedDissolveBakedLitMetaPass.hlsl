#ifndef UNIVERSAL_BAKEDLIT_META_PASS_INCLUDED
#define UNIVERSAL_BAKEDLIT_META_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv0          : TEXCOORD0;
    float2 uv1          : TEXCOORD1;
    float2 uv2          : TEXCOORD2;
#ifdef _TANGENT_TO_WORLD
    float4 tangentOS     : TANGENT;
#endif
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;


    //Advanced Dissolve
	float3 positionOS    : TEXCOORD1;
    float3 normalOS      : TEXCOORD2;

	ADVANCED_DISSOLVE_UV(3)
};

Varyings UniversalVertexMeta(Attributes input)
{
    Varyings output;
    output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2,
        unity_LightmapST, unity_DynamicLightmapST);
    output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);


    //Advanced Dissolve 
	output.positionOS = input.positionOS.xyz;
	output.normalOS = input.normalOS;

	ADVANCED_DISSOLVE_INIT_UV(output, input.uv0, output.positionCS)


    return output;
}

half4 UniversalFragmentMetaBakedLit(Varyings input) : SV_Target
{

//Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if defined(_AD_STATE_ENABLED)

    float4 dissolveBase = 0;
    #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
        dissolveBase.rgb *= _BaseColor.rgb;
    #endif 

	ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_OS(input, dissolveBase, input.positionOS, input.normalOS)
    	
    float3 dissolveAlbedo = 0; 
    float3 dissolveEmission = 0;
	float dissolveBlend = AdvancedDissolveAlbedoEmission(cutoutSource, dissolveBase, dissolveAlbedo, dissolveEmission, input.uv);

#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    MetaInput metaInput = (MetaInput)0;
    metaInput.Albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;


//Advanced Dissolve/////////////////////////////////////////
#if defined(_AD_STATE_ENABLED)
    metaInput.Albedo = lerp(metaInput.Albedo, dissolveAlbedo, dissolveBlend);
    metaInput.Albedo += dissolveEmission, dissolveBlend;
#endif


    return MetaFragment(metaInput);
}

//LWRP -> Universal Backwards Compatibility
Varyings LightweightVertexMeta(Attributes input)
{
    return UniversalVertexMeta(input);
}

half4 LightweightFragmentMetaBakedLit(Varyings input) : SV_Target
{
    return UniversalFragmentMetaBakedLit(input);
}

#endif
