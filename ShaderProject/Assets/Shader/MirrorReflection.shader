Shader "MirrorReflection"
{
	Properties
	{
		_MainTex ("ReflectionTexture", 2D) = "white" {}
		_Color ("Color", Color) = (0.7, 0.7, 0.7, 0.5)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 pos : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return tex2Dproj(_MainTex, i.pos) * _Color;
			}
			ENDCG
		}
	}
}