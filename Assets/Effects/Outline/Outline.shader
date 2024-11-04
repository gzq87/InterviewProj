Shader "Custom/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // ���������ԣ����ڴ洢2D����
        _lineWidth("lineWidth",Range(0,20)) = 1  // �߿����ԣ���Χ��0��20֮�䣬Ĭ��ֵΪ1
        _lineColor("lineColor",Color)=(1,1,1,1)  // �ߵ���ɫ���ԣ�RGBA��ʽ��Ĭ��Ϊ��ɫ
        //_alpha("alpha", float) = 0.5
    }
    SubShader
    {
        // ��Ⱦ���в���͸��
        Tags{
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha  // ���û��ģʽΪԴ��ɫ����Դ͸���ȼ�ȥԴ͸����

        Pass
        {
            CGPROGRAM
            #pragma vertex vert  
            #pragma fragment frag  

            #include "UnityCG.cginc"  

            // ������ɫ������ṹ�� 
            struct VertexInput
            {
                float4 vertex : POSITION;  // ��������
                float2 uv : TEXCOORD0;  // ��������
            };

            // ������ɫ������ṹ�� 
            struct VertexOutput
            {
                float2 uv : TEXCOORD0;  // ��������
                float4 vertex : SV_POSITION;  // ��������
            };

           
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);  // ����������ת�����ü��ռ�
                o.uv = v.uv;  // ������������
                return o;
            }

            sampler2D _MainTex;  // ������
            float4 _MainTex_TexelSize;  // ����������ش�С
            float _lineWidth;  // �߿�
            float4 _lineColor;  // �ߵ���ɫ
            //float _alpha;

            fixed4 frag (VertexOutput i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);  // ��ȡ������ɫ
                fixed4 oldCol = col;

                // ������Χ4����
                float2 up_uv = i.uv + float2(0,1) * _lineWidth * _MainTex_TexelSize.xy;
                float2 down_uv = i.uv + float2(0,-1) * _lineWidth * _MainTex_TexelSize.xy;
                float2 left_uv = i.uv + float2(-1,0) * _lineWidth * _MainTex_TexelSize.xy;
                float2 right_uv = i.uv + float2(1,0) * _lineWidth * _MainTex_TexelSize.xy;

                // �����һ����͸����Ϊ0��˵���Ǳ�Ե
                float w = tex2D(_MainTex,up_uv).a * tex2D(_MainTex,down_uv).a * tex2D(_MainTex,left_uv).a * tex2D(_MainTex,right_uv).a;

                // ��ԭͼ����ֵ�����ݱ�Ե�ж�������ߵ���ɫ��ԭͼ��ɫ
                col.rgb = lerp(_lineColor,col.rgb,w);
                //col *= _alpha;
                return col;
            }
            ENDCG
        }
    }
}
