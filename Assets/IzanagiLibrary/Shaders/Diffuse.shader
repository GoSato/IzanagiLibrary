Shader "Lit/Diffuse With Ambient"
{
    Properties
    {
        // [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _Shininess("Shininess", float) = 10
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
            half _Shininess;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION; // 頂点シェーダーで計算した頂点座標を代入する
                half3 normal : TEXCOORD1;
                half3 halfDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            half4 _MainTex_ST; // TilingとOffsetの情報を取得するのに必要

            // 主に座標変換を行う
            // appdata_base構造体を引数にとる
            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 3次元上の頂点をディスプレイ上のどこに描画するか座標変換を行う
                                                            // fragmentシェーダーに渡す前に必ず変換する必要がある
                // o.uv = v.texcoord; // 
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex); // タイリングとオフセットを考慮したuv値を計算
                half3 worldNormal = UnityObjectToWorldNormal(v.normal); // 正規化済み
                o.normal = worldNormal;

                // ハーフベクトルを求める
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                half3 eyeDir = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                o.halfDir = normalize(_WorldSpaceLightPos0.xyz + eyeDir);

                return o;
            }
            
            // 主にピクセルの色の計算を行う
            // 返却する値はRGBAの4次元の色になる
            fixed4 frag (v2f i) : SV_Target
            {
                float4 ambientLight = UNITY_LIGHTMODEL_AMBIENT;

                half3 diffuse = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz)) * _LightColor0 + ambientLight; // _WroldSpaceLightPos0 : ディレクショナルライトの方向
                
                half3 specular = pow(max(0, dot(i.normal, i.halfDir)), _Shininess) * _LightColor0;
                
                fixed4 col;
                col = tex2D(_MainTex, i.uv) * _Color;
                col.rbg *= saturate(diffuse + specular);
                
                // 環境光(アンビエント)の影響を受けられるようにする,環境光は加算
                                                        // 最終的に色を出力する段階で加算する
                return col;
            }
            ENDCG
        }
    }
}