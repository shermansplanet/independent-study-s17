// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/BlockShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_xColor ("X Color", Color) = (1,1,1,1)
		_yColor ("Y Color", Color) = (1,1,1,1)
		_zColor ("Z Color", Color) = (1,1,1,1)
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _xColor;
			half4 _yColor;
			half4 _zColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float4 newNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal,0.0)));
				o.color = _xColor * pow(newNormal.x,2) + _yColor * pow(newNormal.y,2) + _zColor * pow(newNormal.z,2);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				return col;
			}
			ENDCG
		}
	}
}
