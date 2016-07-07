Shader "SimpleVolumeSH"
{
	Properties
	{
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
			#include "VolumeSH.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 worldNormal : TEXCOORD2;
				float4 worldPos : TEXCOORD3;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				float3 worldNormal = mul(_Object2World, float4(v.normal, 0));
				o.worldNormal = worldNormal;

				float4 worldPos = mul(_Object2World, v.vertex);
				worldPos /= worldPos.w;
				o.worldPos = worldPos;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = 0;

				col.rgb = VolumeSH12Order(float4(i.worldNormal,1), i.worldPos, float3(0,0,0));

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
