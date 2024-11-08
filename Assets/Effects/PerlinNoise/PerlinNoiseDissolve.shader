Shader "Custom/PerlinNoiseDissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PerlinNoiseTex ("Texture", 2D) = "white" {}
        _T ("T", Float) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _PerlinNoiseTex;
            uniform float _T;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 alpha = tex2D(_PerlinNoiseTex, i.uv);
                fixed4 col = tex2D(_MainTex, i.uv);
                
                if (alpha.r < _T){
                    discard;
                }
                return col;
            }
            ENDCG
        }
    }
}
