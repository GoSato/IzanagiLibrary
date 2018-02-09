Shader "Lit/Diffuse With Ambient"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_DiffuseFactor("Diffuse Factor", Range(0.0, 2.0)) = 0.5
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
        
            CGPROGRAM
            #pragma vertex vert // 頂点シェーダーの関数を指定
            #pragma fragment frag // ピクセルシェーダーの関数を指定
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

			fixed4 _Color;
			fixed _DiffuseFactor;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0;
                float4 vertex : SV_POSITION;　// 頂点シェーダーで計算した頂点座標を代入する
            };

            // 主に座標変換を行う
            // appdata_base構造体を引数にとる
            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);　// 3次元上の頂点をディスプレイ上のどこに描画するか座標変換を行う
                                                            // fragmentシェーダーに渡す前に必ず変換する必要がある
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));
                return o;
            }
            
            sampler2D _MainTex;

            // 主にピクセルの色の計算を行う
            // 返却する値はRGBAの4次元の色になる
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color * tex2D(_MainTex, i.uv) * _DiffuseFactor;
                col *= i.diff;
                return col;
            }
            ENDCG
        }
    }
}