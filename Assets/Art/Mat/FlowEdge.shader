Shader "Custom/FlowEdge"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0, 1, 1, 1)
        _ScrollXSpeed ("X Scroll Speed", float) = 2.0
        _ScrollYSpeed ("Y Scroll Speed", float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // ZWrite Off
            

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _EmissionColor;
            float _ScrollXSpeed;
            float _ScrollYSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算流动的UV坐标
                float2 flowingUv = i.uv;
                flowingUv.x += _Time.y * _ScrollXSpeed;
                flowingUv.y += _Time.y * _ScrollYSpeed;

                // 采样纹理并乘以发光颜色
                fixed4 col = tex2D(_MainTex, flowingUv);
                col *= _EmissionColor;
                
                return col;
            }
            ENDCG
        }
    }
}