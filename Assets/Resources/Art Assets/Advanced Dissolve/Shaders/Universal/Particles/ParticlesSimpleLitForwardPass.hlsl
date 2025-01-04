#ifndef UNIVERSAL_PARTICLES_FORWARD_SIMPLE_LIT_PASS_INCLUDED
#define UNIVERSAL_PARTICLES_FORWARD_SIMPLE_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Particles.hlsl"

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

    output.fogCoord = (half)input.positionWS.w;
    output.vertexLighting = half3(0.0h, 0.0h, 0.0h);
    output.bakedGI = SampleSHPixel(input.vertexSH, output.normalWS);
    output.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.clipPos);
    output.shadowMask = half4(1, 1, 1, 1);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

VaryingsParticle ParticlesLitVertex(AttributesParticle input)
{
    VaryingsParticle output;

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

#ifdef _NORMALMAP
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#endif

    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.positionWS.xyz = vertexInput.positionWS.xyz;
    output.positionWS.w = ComputeFogFactor(vertexInput.positionCS.z);
    output.clipPos = vertexInput.positionCS;
    output.color = GetParticleColor(input.color);

#if defined(_FLIPBOOKBLENDING_ON)
#if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
    GetParticleTexcoords(output.texcoord, output.texcoord2AndBlend, input.texcoords.xyxy, 0.0);
#else
    GetParticleTexcoords(output.texcoord, output.texcoord2AndBlend, input.texcoords, input.texcoordBlend);
#endif
#else
    GetParticleTexcoords(output.texcoord, input.texcoords.xy);
#endif

#if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON) || defined(_DISTORTION_ON)
    output.projectedPosition = ComputeScreenPos(vertexInput.positionCS);
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

//Advanced Dissolve
 ADVANCED_DISSOLVE_INIT_UV(output, input.texcoords.xy, vertexInput.positionCS)

    return output;
}

half4 ParticlesLitFragment(VaryingsParticle input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


//Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if defined(_AD_STATE_ENABLED)

    float4 dissolveBase = 0;
    #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.texcoord.xy);
        dissolveBase.rgb *= _BaseColor.rgb;
    #endif

	ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(input, dissolveBase, input.positionWS.xyz, input.normalWS.xyz)

    #if !defined(_ALPHATEST_ON)
        AdvancedDissolveClip(cutoutSource);
    #endif
    	
    float3 dissolveAlbedo = 0; 
    float3 dissolveEmission = 0;
	float dissolveBlend = AdvancedDissolveAlbedoEmission(cutoutSource, dissolveBase, dissolveAlbedo, dissolveEmission, input.texcoord.xy);

#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    ParticleParams particleParams;
    InitParticleParams(input, particleParams);

    half3 normalTS = SampleNormalTS(particleParams.uv, particleParams.blendUv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    half4 albedo = SampleAlbedo(TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), particleParams);

    //Advanced Dissolve/////////////////////////////////////////
    #ifdef _ALPHATEST_ON
            
        #if defined(_AD_STATE_ENABLED)
            AdvancedDissolveCalculateAlphaAndClip(cutoutSource, albedo.a, _Cutoff);
        #endif

        AlphaDiscard(albedo.a, _Cutoff);

    #endif


    half3 diffuse = AlphaModulate(albedo.rgb, albedo.a);
    half alpha = albedo.a;
#if defined(_EMISSION)
    half3 emission = BlendTexture(TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap), particleParams.uv, particleParams.blendUv) * _EmissionColor.rgb;
#else
    half3 emission = half3(0, 0, 0);
#endif
    half4 specularGloss = SampleSpecularSmoothness(particleParams.uv, particleParams.blendUv, albedo.a, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    half shininess = specularGloss.a;

#if defined(_DISTORTION_ON)
    diffuse = Distortion(half4(diffuse, alpha), normalTS, _DistortionStrengthScaled, _DistortionBlend, particleParams.projectedPosition);
#endif


//Advanced Dissolve/////////////////////////////////////////
#if defined(_AD_STATE_ENABLED)
    diffuse.rgb = lerp(diffuse.rgb, dissolveAlbedo, dissolveBlend);
    emission = lerp(emission, dissolveEmission, dissolveBlend);
#endif


    InputData inputData;
    InitializeInputData(input, normalTS, inputData);

    half4 color = UniversalFragmentBlinnPhong(inputData, diffuse, specularGloss, shininess, emission, alpha);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    return color;
}

#endif // UNIVERSAL_PARTICLES_FORWARD_SIMPLE_LIT_PASS_INCLUDED
