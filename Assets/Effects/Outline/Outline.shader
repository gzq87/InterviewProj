Shader "Custom/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // 主纹理属性，用于存储2D纹理
        _lineWidth("lineWidth",Range(0,20)) = 1  // 线宽属性，范围在0到20之间，默认值为1
        _lineColor("lineColor",Color)=(1,1,1,1)  // 线的颜色属性，RGBA格式，默认为白色
        //_alpha("alpha", float) = 0.5
    }
    SubShader
    {
        // 渲染队列采用透明
        Tags{
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha  // 设置混合模式为源颜色乘以源透明度减去源透明度

        Pass
        {
            CGPROGRAM
            #pragma vertex vert  
            #pragma fragment frag  

            #include "UnityCG.cginc"  

            // 顶点着色器输入结构体 
            struct VertexInput
            {
                float4 vertex : POSITION;  // 顶点坐标
                float2 uv : TEXCOORD0;  // 纹理坐标
            };

            // 顶点着色器输出结构体 
            struct VertexOutput
            {
                float2 uv : TEXCOORD0;  // 纹理坐标
                float4 vertex : SV_POSITION;  // 顶点坐标
            };

           
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);  // 将顶点坐标转换到裁剪空间
                o.uv = v.uv;  // 传递纹理坐标
                return o;
            }

            sampler2D _MainTex;  // 主纹理
            float4 _MainTex_TexelSize;  // 主纹理的像素大小
            float _lineWidth;  // 线宽
            float4 _lineColor;  // 线的颜色
            //float _alpha;

            fixed4 frag (VertexOutput i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);  // 获取纹理颜色
                fixed4 oldCol = col;

                // 采样周围4个点
                float2 up_uv = i.uv + float2(0,1) * _lineWidth * _MainTex_TexelSize.xy;
                float2 down_uv = i.uv + float2(0,-1) * _lineWidth * _MainTex_TexelSize.xy;
                float2 left_uv = i.uv + float2(-1,0) * _lineWidth * _MainTex_TexelSize.xy;
                float2 right_uv = i.uv + float2(1,0) * _lineWidth * _MainTex_TexelSize.xy;

                // 如果有一个点透明度为0，说明是边缘
                float w = tex2D(_MainTex,up_uv).a * tex2D(_MainTex,down_uv).a * tex2D(_MainTex,left_uv).a * tex2D(_MainTex,right_uv).a;

                // 和原图做插值，根据边缘判断来混合线的颜色和原图颜色
                col.rgb = lerp(_lineColor,col.rgb,w);
                //col *= _alpha;
                return col;
            }
            ENDCG
        }
    }
}
