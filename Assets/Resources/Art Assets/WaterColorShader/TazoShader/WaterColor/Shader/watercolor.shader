

Shader "Tazo/WaterColor"
{
	Properties
	{
		[Header(Base)]
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
		_MColor("Color", Color) = (0.5,0.5,0.5,1)
		_BaseTex3("Mask(R)", 2D) = "white" {}
		_pow("POW", Range(0.0, 30.0)) = 1
		_a("alpha", Range(0.0, 1.0)) = 0
		_s("saturation", Range(0.0, 1.0)) = 0
		[Header(SoftEdge)]
		[NoScaleOffset]_MatCap ("MatCap (R)", 2D) = "white" {}
		_AV("MatAngle", Range(0, 360)) = 1
		_mat_scale("MatScale", Range(0, 10)) = 1

	

		[Header(Project)]
		

		[NoScaleOffset]_Project("Project(RGB)", 2D) = "white" {}
		_AlphaMode("Use Value as Alpha", Range(0,1)) = 0.00
		_tile("Tile", Range(0.0, 20.0)) = 1
		_tileOffsetX("OffsetX", Range(0, 1)) = 0
		_tileOffsetY("OffseeY", Range(0, 1)) = 0
		[Header(Distortion)]
		[NoScaleOffset]_ProjectUV("UV Dis(R)", 2D) = "black" {}
		_tileUV("UV Dis Tile", Range(0.0, 20.0)) = 1
		_flow_offset("flow_offset", Range(-10, 10)) = 0
		_flow_strength("flow_strength", Range(-10, 10)) = 0.5

		
	}
	
	Subshader
	{
		Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		
		Pass
		{
			Cull[_Cull]
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
				float4 pos	: SV_POSITION;
				float4 cap	: TEXCOORD0;
				float2 uv	: TEXCOORD2;
				float4 uv_object : TEXCOORD1;
				
			};
			
			uniform float4 _MColor;
			float _AlphaMode;
			uniform sampler2D _BaseTex3;
			float4 _BaseTex3_ST;
			float _pow;
			float _tile;
			float _tileUV;
			float _flow_offset;
			float _flow_strength;
			float _AV;
			fixed _a;
			fixed _s;
			float _mat_scale;
			float _tileOffsetX;
			float _tileOffsetY;
			v2f vert(appdata v)
			{
				v2f o;
				
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 worldNorm = UnityObjectToWorldNormal(v.normal);
				worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
		
				o.cap.xy = worldNorm.xy * 0.5 + 0.5;
				o.cap.z = 1 - abs(v.normal.z);

				
				o.cap.w = v.normal.y;
				o.uv = TRANSFORM_TEX(v.texcoord, _BaseTex3);
				o.uv_object = v.vertex*0.5+0.5;
				return o;
			}



			uniform sampler2D _MatCap;
			uniform sampler2D _Project;
			uniform sampler2D _ProjectUV;

			float4 frag(v2f i) : SV_Target
			{

				float2 offset   = float2(_tileOffsetX, _tileOffsetY);
				fixed4 t1 = tex2D(_ProjectUV, (frac(_Time.y * _flow_offset) + i.uv_object.rg * _tileUV));
				fixed4 t2 = tex2D(_ProjectUV, (frac(_Time.y * _flow_offset) + i.uv_object.gb* _tileUV));
				fixed4 p1 = tex2D(_Project, offset+i.uv_object.rg * _tile + t1 * _flow_strength);
				fixed4 p2 = tex2D(_Project, offset+ i.uv_object.gb* _tile*0.97 + + t2 * _flow_strength);
				fixed4 p3 = tex2D(_Project, offset + i.uv_object.rb * _tile*1.02 + t1 * _flow_strength);

				//rotation
				
				float2 uv1 = i.cap.xy - float2(0.5, 0.5);
				uv1 = float2(uv1.x * cos(_Time.y * _AV) - uv1.y * sin(_Time.y * _AV), uv1.x * sin(_Time.y * _AV) + uv1.y * cos(_Time.y * _AV));
				uv1 += float2(0.5, 0.5);

				float2 uv2 = i.cap.xy - float2(0.5, 0.5);

				uv2 = float2(uv2.x * cos(_Time.y * -_AV) - uv2.y * sin(_Time.y * -_AV), uv2.x * sin(_Time.y * -_AV) + uv2.y * cos(_Time.y * -_AV));
				uv2 += float2(0.5, 0.5);

				fixed4 mc1 = tex2D(_MatCap, (uv1 - 0.5) * _mat_scale + 0.5);
				fixed4 mc2 = tex2D(_MatCap, (uv2 - 0.5) * _mat_scale + 0.5);

				fixed4 mc = saturate(mc1 * mc2);
				
				////////////////////////////////
				//simple way
				//fixed4 mc = tex2D(_MatCap, (i.cap.xy - 0.5) * _mat_scale + 0.5);
				/////////////////////////////////////



				fixed4 cc ;
				fixed p = abs(i.cap.w);
				fixed pp = p * p*p*p;
				cc.rgb = lerp(lerp(p1,p2, i.cap.z),p3, pp) ;
				cc.rgb = lerp(cc.rgb, cc.rgb*cc.rgb,_s);

				fixed4 mt1 = tex2D(_BaseTex3, i.uv_object.rg *5.5*0.95 + t1 * _flow_strength);
				fixed4 mt2 = tex2D(_BaseTex3, i.uv_object.gb *5.5 + t1 * _flow_strength);
				fixed4 mt3 = tex2D(_BaseTex3, i.uv_object.rb *5.5*1.05 + t1 * _flow_strength);
				fixed4 mt  = lerp(lerp(mt1, mt2, i.cap.z), mt3,pp);

				cc.a = lerp(pow(mt.r, _pow) * lerp(cc.r, 1, _AlphaMode)* mc.r,1,_a);
				cc *= _MColor;
				//cc.rgb = p3;
				return cc;
			}
			ENDCG
		}

		
	}
	
	Fallback "VertexLit"
}
