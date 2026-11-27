Shader "Custom/MirrorPlane"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ReflectionTex ("Reflection", 2D) = "white" {}
        _Reflectivity ("Reflectivity", Range(0, 1)) = 0.5
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 refl : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReflectionTex;
            float _Reflectivity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.refl = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // スクリーン座標で反射テクスチャをサンプリング
                fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl));
                // 反射カメラのAlpha(背景=0, 反射対象=1)を使って透過を制御
                fixed3 col = refl.rgb;
                fixed alpha = saturate(refl.a * _Reflectivity);
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}
