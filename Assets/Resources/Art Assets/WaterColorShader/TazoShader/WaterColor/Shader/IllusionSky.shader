

Shader "Tazo/IllusionSky"
{
	Properties
	{
		[Header(Base)]

		_S("Bright", Range(0.0, 5)) = 1
			_MColor("Color", Color) = (0.5,0.5,0.5,1)

		_ss("seam control", Range(0.0, 5)) = 1

		_d("dissolve control", Range(0.0, 1.5)) = 0
		_a("alpha", Range(0.0, 1.0)) = 0

		[Header(Project)]
		[NoScaleOffset]_Project("Project(RGB)", 2D) = "white" {}
		_tile("Tile", Range(0.0, 20.0)) = 1
		_tileOffsetX("OffsetX", Range(0, 1)) = 0
		_tileOffsetY("OffseeY", Range(0, 1)) = 0
		[Header(Distortion)]
		[NoScaleOffset]_ProjectUV("UV Dis(R)", 2D) = "black" {}
		_tileUV("UV Dis Tile", Range(0.0, 20.0)) = 1
		_flow_offset("flow_offset", Range(-10, 10)) = 0
		_flow_strength("flow_strength", Range(-10, 10)) = 0.5
		
		[Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 0

	}
	
	Subshader
	{
		Tags { "Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		



//shell
		Pass
		{
			Cull Front
			ZWrite[_ZWrite]
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
				fixed4 color : COLOR;
			};
	


			float _tile;
			float _tileUV;
			float _flow_offset;
			float _flow_strength;
			fixed _S;
			fixed _d;
			fixed _ss;
			float _tileOffsetX;
			float _tileOffsetY;
			fixed _a;
		
			v2f vert(appdata v)
			{
				v2f o;
				float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
				//o.cap.w = worldNorm.y;
				o.cap.w = v.normal.y;
				float3 VNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
			
				o.cap.xy = VNorm.xy * 0.5 + 0.5;
				o.cap.z = pow(1 - abs(v.normal.z),_ss);
				

 				o.pos = UnityObjectToClipPos(v.vertex );
				
		
				o.uv_object = v.vertex*0.5+0.5;
				return o;
			}

		
			uniform float4 _MColor;
			uniform sampler2D _MatCap;
			uniform sampler2D _Project;
			uniform sampler2D _ProjectUV;

			float4 frag(v2f i) : SV_Target
			{

				float2 offset   = float2(_tileOffsetX, _tileOffsetY);
				fixed4 t1 = tex2D(_ProjectUV, (frac(_Time.y * _flow_offset) + i.uv_object.rg * _tileUV));
				fixed4 t2 = tex2D(_ProjectUV, (frac(_Time.y * _flow_offset) + i.uv_object.gb * _tileUV));
				fixed4 p1 = tex2D(_Project, offset+i.uv_object.rg * _tile*0.98 + t1 * _flow_strength);
				fixed4 p2 = tex2D(_Project, offset+i.uv_object.gb * _tile  + t2 * _flow_strength);
				//fixed4 t3 = tex2D(_ProjectUV, (frac(_Time.y * _flow_offset) + i.uv_object.rb * _tileUV));
				fixed4 p3 = tex2D(_Project, offset + i.uv_object.rb * _tile*1.02 + t1 * _flow_strength);
				

			
				fixed4 cc = _MColor *2.7;
				
				fixed p = abs(i.cap.w);
				cc.rgb = lerp(lerp(p1,p2, i.cap.z),p3, p*p*p*p)* _MColor* _S;
	
				cc.a = lerp(lerp(_MColor.a*(cc.r), _MColor.a,_a), smoothstep(_d,_d+0.25,cc.r),_d);
				
		
				return cc;
			}
			ENDCG
		}

		
	}
	
	Fallback "VertexLit"
}
