Shader "Hidden/UModeler_Wire"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_WireframeThick("WireframeThick", float) = 1.5
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 8
//		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("CullMode", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Geometry"
			"Queue" = "Geometry"
			"DisableBatching" = "True"
			"ForceNoShadowCasting" = "True"
			"IgnoreProjector" = "True"
		}

		Pass //"Edge"
		{
			Cull Off
			Lighting Off
			ZTest[_ZTest]
			ZWrite Off
			Blend Off
			Offset -1, -1

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 pos : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			float _WireframeThick;

			inline float4 ClipToScreen(float4 v)
			{
				v.xy /= v.w;
				v.xy = v.xy * .5 + .5;
				v.xy *= _ScreenParams.xy;
				return v;
			}

			v2f vert(appdata v)
			{
				v2f o;

				o.pos = float4(UnityObjectToViewPos(v.pos.xyz), 1);

				// pull closer to camera and into clip space
				o.pos *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);
				// convert clip -> ndc -> screen, build billboards in geo shader, then screen -> ndc -> clip
				o.pos = ClipToScreen(o.pos);
				o.color = v.color;
				return o;
			}

			inline float4 ScreenToClip(float4 v)
			{
//				v.z -= .0001 * (1 - UNITY_MATRIX_P[3][3]);
				v.xy /= _ScreenParams.xy;
				v.xy = (v.xy - .5) / .5;
				v.xy *= v.w;
				return v;
			}

			[maxvertexcount(4)]
			void geo(line v2f p[2], inout TriangleStream<v2f> triStream)
			{
				float2 perp = normalize(float2(-(p[1].pos.y - p[0].pos.y), p[1].pos.x - p[0].pos.x)) * _WireframeThick;

				v2f geo_out;

                geo_out.pos = ScreenToClip( float4(p[1].pos.x + perp.x, p[1].pos.y + perp.y, p[1].pos.z, p[1].pos.w) );
                geo_out.color = p[1].color;
                triStream.Append(geo_out);

                geo_out.pos =  ScreenToClip( float4(p[1].pos.x - perp.x, p[1].pos.y - perp.y, p[1].pos.z, p[1].pos.w) );
                geo_out.color = p[1].color;
                triStream.Append(geo_out);

                geo_out.pos =  ScreenToClip( float4(p[0].pos.x + perp.x, p[0].pos.y + perp.y, p[0].pos.z, p[0].pos.w) );
                geo_out.color = p[0].color;
                triStream.Append(geo_out);

                geo_out.pos =  ScreenToClip( float4(p[0].pos.x - perp.x, p[0].pos.y - perp.y, p[0].pos.z, p[0].pos.w) );
                geo_out.color = p[0].color;
                triStream.Append(geo_out);
			}

			fixed4 frag(v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
	}
	Fallback Off
}
