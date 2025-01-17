Shader "SeeThroughShader/ShaderExtensionExamples/Handwritten/BiRP/UnityUnlitExtendedWithSTS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    ////////////////////////////////////////////////////////////////////
    //Add STS Properties from 'STSPropertiesToBeAddedToYourShader.txt'//
    ////////////////////////////////////////////////////////////////////
	    _DissolveColor("Dissolve Color", Color) = (1,1,1,1)
	    _DissolveColorSaturation("Dissolve Color Saturation", Range(0,1)) = 1.0
	    _DissolveEmission("Dissolve Emission", Range(0,1)) = 1.0
	    [AbsoluteValue()] _DissolveEmissionBooster("Dissolve Emission Booster", float) = 1
	    _DissolveTex("Dissolve Effect Texture", 2D) = "white" {}

	    _DissolveMethod ("Dissolve Method", Float) = 0
	    _DissolveTexSpace ("Dissolve Tex Space", Float) = 0


	    [Enum(STSInteractionMode)] _InteractionMode ("Interaction Mode", Float) = 0
	    [Enum(ObstructionMode)] _Obstruction ("Obstruction Mode", Float) = 0
	    _AngleStrength("Angle Obstruction Strength", Range(0,1)) = 1.0
        
	    _ConeStrength ("Cone Obstruction Strength", Range(0,1)) = 1.0
	    _ConeObstructionDestroyRadius ("Cone Obstruction Destroy Radius", float) = 10.0

        
	    _CylinderStrength ("Cylinder Obstruction Strength", Range(0,1)) = 1.0
	    _CylinderObstructionDestroyRadius ("Cylinder Obstruction Destroy Radius", float) = 10.0

	    _CircleStrength ("Circle Obstruction Strength", Range(0,1)) = 1.0
	    _CircleObstructionDestroyRadius ("Circle Obstruction Destroy Radius", float) = 10.0

	    _CurveStrength ("Curve Obstruction Strength", Range(0,1)) = 1.0
	    _CurveObstructionDestroyRadius ("Curve Obstruction Destroy Radius", float) = 10.0
	    [HideInInspector] _ObstructionCurve("Obstruction Curve", 2D) = "white" {}

	    _DissolveFallOff("Dissolve FallOff", Range(0,1)) = 0.0

	    _DissolveMask("Dissolve Mask", 2D) = "white" {}
	    _DissolveMaskEnabled("Use DissolveMask", float) = 0.0

        _AffectedAreaPlayerBasedObstruction("_AffectedAreaPlayerBasedObstruction",  float) = 0.0

	    _IntrinsicDissolveStrength("Intrinsic Dissolve Strength", Range(0,1)) = 0.0

	    [MaterialToggle] _PreviewMode("Preview Mode", float) = 0.0
	    _PreviewIndicatorLineThickness("Indicator Line Thickness",  Range(0.01,0.5)) = 0.04        

	    [AbsoluteValue()] _UVs ("Dissolve Texture Scale", float) = 1.0
	    [MaterialToggle] _hasClippedShadows("Has Clipped Shadows", Float) = 0
        
	    [MaterialToggle] _Floor ("Floor", float) = 1.0
	    [Enum(FloorMode)] _FloorMode ("Floor Mode", Float) = 0
	    _FloorY ("FloorY",  float) = 1.0
	    _PlayerPosYOffset ("PlayerPos Y Offset", float) = 1.0  
        _AffectedAreaFloor("_AffectedAreaFloor",  float) = 0.0

	    [AbsoluteValue()] _FloorYTextureGradientLength ("FloorY Texture Gradient Length", float) = 0.1  

	    [MaterialToggle] _AnimationEnabled("Animation Enabled", Float) = 1
	    _AnimationSpeed("Animation Speed", Range(0,2)) = 1

	    [MaterialToggle] _IsReplacementShader ("hidden: _IsReplacementShader", Float) = 0

	    [HideInInspector] _RaycastMode ("hidden: _RaycastMode", Float) = 0
	    [HideInInspector] _TriggerMode ("hidden: _TriggerMode", Float) = 0

	    [HideInInspector] _IsExempt ("_IsExempt", Float) = 0

	    [AbsoluteValue()] _TransitionDuration ("Transition Duration In Seconds", Float) = 2

	    [AbsoluteValue()] _DefaultEffectRadius ("Default Effect Radius",Float) = 25    
        [MaterialToggle] _EnableDefaultEffectRadius("Enable Default Effect Radius", float) = 0.0

	    [HideInInspector] _numOfPlayersInside ("hidden: _numOfPlayersInside", Float) = 0
	    [HideInInspector] _tValue ("hidden: _tValue", Float) = 0
	    [HideInInspector] _tDirection ("hidden: _tDirection", Float) = 0
	    [HideInInspector] _id ("hidden: _id", Float) = 0

	    [MaterialToggle] _TexturedEmissionEdge("Textured Emission Edge", float) = 1.0
	    _TexturedEmissionEdgeStrength("Textured Emission Edge Strength", Range(0,1)) = 0.3

	    [MaterialToggle] _IsometricExclusion("Isometric Exclusion", float) = 0.0
	    _IsometricExclusionDistance("Isometric Exclusion Distance", float) = 0.0
	    _IsometricExclusionGradientLength("Isometric Exclusion Gradient Length", float) = 0.1

	    [MaterialToggle] _Ceiling ("Ceiling", float) = 0.0

	    [Enum(CeilingMode)] _CeilingMode ("Ceiling Mode", Float) = 0
	    [Enum(CeilingBlendMode)] _CeilingBlendMode ("Blending Mode", Float) = 1.0
	    _CeilingY ("CeilingY",  float) = 1.0
	    _CeilingPlayerYOffset ("PlayerPos Y Offset", float) = 1.0  
	    _CeilingYGradientLength ("CeilingY Gradient Length", float) = 0.1

	    [MaterialToggle] _Zoning("Zoning", float) = 0.0
	    [Enum(ZoningMode)] _ZoningMode("Zoning Mode", Float) = 0.0
	    _ZoningEdgeGradientLength ("Edge Gradient Length", float) = 0.1
    


	    [MaterialToggle] _IsZoningRevealable ("Is Zoning Revealable", float) = 0.0

    

	    [MaterialToggle] _SyncZonesWithFloorY ("Sync Zones With FloorY", float) = 0.0
	    _SyncZonesFloorYOffset ("Sync Zones Floor YOffset", float) = 0.0

        [MaterialToggle] _UseCustomTime ("_UseCustomTime", float) = 0.0


	    [MaterialToggle] _isReferenceMaterial("Is Reference Material", float) = 0.0


        // FOR UI ONLY
        [HideInInspector] _ShowContentDissolveArea ("hidden: _ShowContentDissolveArea", Float) = 1
        [HideInInspector] _ShowContentInteractionOptionsArea ("hidden: _ShowContentInteractionOptionsArea", Float) = 1
        [HideInInspector] _ShowContentObstructionOptionsArea ("hidden: _ShowContentObstructionOptionsArea", Float) = 1
        [HideInInspector] _ShowContentAnimationArea ("hidden: _ShowContentAnimationArea", Float) = 1
        [HideInInspector] _ShowContentZoningArea ("hidden: _ShowContentZoningArea", Float) = 1
        [HideInInspector] _ShowContentReplacementOptionsArea ("hidden: _ShowContentReplacementOptionsArea", Float) = 1
        [HideInInspector] _ShowContentDebugArea ("hidden: _ShowContentDebugArea", Float) = 1
    
        [MaterialToggle] _SyncCullMode ("_SyncCullMode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog


            #include "UnityCG.cginc"


            sampler2D _MainTex;
            float4 _MainTex_ST;

            //////////////////
            //Add dependency//
            //////////////////
            #include "Packages/com.shadercrew.seethroughshader.core/Scripts/Shaders/ShaderCustomization/STS Handwritten Shader Extension/HandwrittenShaderExtension.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f //vertexToFragment
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;   //Add world position, because it is needed in the STS function
                float3 worldNormal : TEXCOORD2; //Add world normal, because it is needed in the STS function
                float4 screenPos : TEXCOORD3; //Add screen position, because it is needed in the STS function

            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
    
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; //calculate world position
                o.worldNormal = UnityObjectToWorldNormal(v.normal); //calculate world normal
                o.screenPos = ComputeScreenPos(o.vertex); //calculate screen position
    
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

    

    
                float3 albedo = col; // your color values
                float3 emission = float3(0, 0, 0); // your emission values
                float alphaForClipping = 0; // you can use this if you dont want to use clipping and instead a alpha blend/fade shader
                
                //Call function from HandwrittenShaderExtension.hlsl
                AddSeeThroughShaderToShader(albedo, emission, alphaForClipping, i.worldPos, i.worldNormal, i.screenPos);
    
                return half4(albedo + emission, 1);
}
            ENDCG
        }
    }

    ////////////////////////////////////////
    // Add custom editor! Very important! //
    ////////////////////////////////////////
    CustomEditor"ShaderCrew.SeeThroughShader.STSShaderGraphGenericEditor" 

}
