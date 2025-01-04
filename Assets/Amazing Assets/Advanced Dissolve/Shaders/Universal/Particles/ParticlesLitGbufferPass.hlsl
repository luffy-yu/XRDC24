#ifndef UNIVERSAL_PARTICLES_GBUFFER_LIT_PASS_INCLUDED
#define UNIVERSAL_PARTICLES_GBUFFER_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

void InitializeInputData(VaryingsParticle input, half3 normalTS, out InputData output)
{
    output = (InputData)0;

    output.positionWS = input.positionWS.xyz;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
    output.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
#else
    half3 viewDirWS = input.viewDirWS;
    output.normalWS = input.normalWS;
#endif

    output.normalWS = NormalizeNormalPerPixel(output.normalWS);

#if SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    output.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
#else
    output.shadowCoord = float4(0, 0, 0, 0);
#endif

    output.fogCoord = 0.0; // not used for deferred shading
    output.vertexLighting = half3(0.0h, 0.0h, 0.0h);
    output.bakedGI = SampleSHPixel(input.vertexSH, output.normalWS);
    output.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.clipPos);
    output.shadowMask = half4(1, 1, 1, 1);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

VaryingsParticle ParticlesGBufferVertex(AttributesParticle input)
{
    VaryingsParticle output = (VaryingsParticle)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);


#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
   #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
      CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(input.vertex, input.normal, input.tangent)
   #else
      CURVEDWORLD_TRANSFORM_VERTEX(input.vertex)
   #endif
#endif


    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangent);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
#if !SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

#ifdef _NORMALMAP
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#endif

    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.positionWS.xyz = vertexInput.positionWS;
    output.clipPos = vertexInput.positionCS;
    output.color = input.color;

#if defined(_FLIPBOOKBLENDING_ON)
#if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
    GetParticleTexcoords(output.texcoord, output.texcoord2AndBlend, input.texcoords.xyxy, 0.0);
#else
    GetParticleTexcoords(output.texcoord, output.texcoord2AndBlend, input.texcoords, input.texcoordBlend);
#endif
#else
    GetParticleTexcoords(output.texcoord, input.texcoords.xy);
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif


//Advanced Dissolve
 ADVANCED_DISSOLVE_INIT_UV(output, input.texcoords.xy, vertexInput.positionCS)

    return output;
}

FragmentOutput ParticlesGBufferFragment(VaryingsParticle input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


//Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float4 dissolveCutout = 0;
#if defined(_AD_STATE_ENABLED)

    float4 dissolveBase = 0;
    #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.texcoord.xy);
        dissolveBase.rgb *= _BaseColor.rgb;
    #endif

	ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(input, dissolveBase, input.positionWS.xyz, input.normalWS.xyz)
    dissolveCutout = cutoutSource;

    #if !defined(_ALPHATEST_ON)
        AdvancedDissolveClip(cutoutSource);
    #endif
    	
    float3 dissolveAlbedo = 0; 
    float3 dissolveEmission = 0;
	float dissolveBlend = AdvancedDissolveAlbedoEmission(cutoutSource, dissolveBase, dissolveAlbedo, dissolveEmission, input.texcoord.xy);

#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    float3 blendUv = float3(0, 0, 0);
#if defined(_FLIPBOOKBLENDING_ON)
    blendUv = input.texcoord2AndBlend;
#endif

    float4 projectedPosition = float4(0,0,0,0);
#if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON) || defined(_DISTORTION_ON)
    projectedPosition = input.projectedPosition;
#endif

    SurfaceData surfaceData;
    InitializeParticleLitSurfaceData(input.texcoord, blendUv, input.color, projectedPosition, surfaceData, dissolveCutout);

    InputData inputData = (InputData)0;
    InitializeInputData(input, surfaceData.normalTS, inputData);


//Advanced Dissolve/////////////////////////////////////////
#if defined(_AD_STATE_ENABLED)
    surfaceData.albedo.rgb = lerp(surfaceData.albedo.rgb, dissolveAlbedo, dissolveBlend);
    surfaceData.emission = lerp(surfaceData.emission, dissolveEmission, dissolveBlend);
#endif

    // Stripped down version of UniversalFragmentPBR().

    // in LitForwardPass GlobalIllumination (and temporarily LightingPhysicallyBased) are called inside UniversalFragmentPBR
    // in Deferred rendering we store the sum of these values (and of emission as well) in the GBuffer
    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    half3 color = GlobalIllumination(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.normalWS, inputData.viewDirectionWS);

    return BRDFDataToGbuffer(brdfData, inputData, surfaceData.smoothness, surfaceData.emission + color);
}

#endif // UNIVERSAL_PARTICLES_GBUFFER_LIT_PASS_INCLUDED
