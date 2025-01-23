Shader "Custom/UIGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1,1,1,0)  // 위쪽 색상 (투명)
        _BottomColor ("Bottom Color", Color) = (0,0,0,1)  // 아래쪽 색상 (검정)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

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

            fixed4 _TopColor;
            fixed4 _BottomColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv.y를 기준으로 그라데이션 계산
                return lerp(_BottomColor, _TopColor, i.uv.y);
            }
            ENDCG
        }
    }
}
