Shader "IzanagiShader/DestructionAdditiveUnlit"
{

Properties
{
    [KeywordEnum(Property, Camera)]
    _Method("DestructionMethod", Float) = 0
    _TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
    _MainTex("Particle Texture", 2D) = "white" {}
    _InvFade("Soft Particles Factor", Range(0.01, 3.0)) = 1.0
    _Destruction("Destruction Factor", Range(0.0, 1.0)) = 0.0
    _PositionFactor("Position Factor", Range(0.0, 1.0)) = 0.2
    _RotationFactor("Rotation Factor", Range(0.0, 1.0)) = 1.0
    _ScaleFactor("Scale Factor", Range(0.0, 1.0)) = 1.0 // 大きいほど距離遠いときにちっさくなる
    _AlphaFactor("Alpha Factor", Range(0.0, 1.0)) = 1.0
    _StartDistance("Start Distance", Float) = 0.6
    _EndDistance("End Distance", Float) = 0.3
}

CGINCLUDE

#include "UnityCG.cginc"

#define PI 3.1415926535

sampler2D _MainTex;
fixed4 _MainTex_ST;
fixed4 _TintColor;
sampler2D_float _CameraDepthTexture;
fixed _InvFade;
fixed _Destruction;
fixed _PositionFactor;
fixed _RotationFactor;
fixed _ScaleFactor;
fixed _AlphaFactor;
fixed _StartDistance;
fixed _EndDistance;

// Vertex, Geometry シェーダーへの入力の構造体
// よく使われる構造体はUnityCG.cgincにあらかじめ定義されている

struct appdata_t 
{
    float4 vertex : POSITION; // 頂点の位置やで
    fixed4 color : COLOR; // 頂点の色や
    float2 texcoord : TEXCOORD0; // 1番目のUV座標
    UNITY_VERTEX_INPUT_INSTANCE_ID // GPU Instancingに対応するのに必要っぽい
};


// シンタックス(言語仕様上の機能)の組み合わせがセマンティック(意図)
// シェーダーにおいてはシェーダープログラムの入出力の値が何を意味するのかを表すためのモノ
// つまり値の用途 float4 v : POSITION ← ポジションを表してるんやで



// Vertexシェーダーの出力、Fragmentシェーダーへの入力

struct g2f
{
    float4 vertex : SV_POSITION; // 頂点のクリップ座標(必須), レンダリングパイプライン上で処理済み(Vertexシェーダー), System Velueの略？
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_FOG_COORDS(1) // fog...?
#ifdef SOFTPARTICLES_ON
        float4 projPos : TEXCOORD2;
#endif
    UNITY_VERTEX_OUTPUT_STEREO // VR用シングルパスステレオレンダリングのマクロ
};

// ランダムな値を返すための関数
// GPUに依存しない疑似ランダム関数として有名らしい
// https://qiita.com/shimacpyon/items/d15dee44a0b8b3883f76

// C#でいうところのstatic関数的な感じだろうか・・・
inline float rand(float2 seed)
{
    return frac(sin(dot(seed.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float3 rotate(float3 p, float3 rotation)
{
    float3 a = normalize(rotation);
    float angle = length(rotation);
    if (abs(angle) < 0.001) return p;
    float s = sin(angle);
    float c = cos(angle);
    float r = 1.0 - c;
    float3x3 m = float3x3(
        a.x * a.x * r + c,
        a.y * a.x * r + a.z * s,
        a.z * a.x * r - a.y * s,
        a.x * a.y * r - a.z * s,
        a.y * a.y * r + c,
        a.z * a.y * r + a.x * s,
        a.x * a.z * r + a.y * s,
        a.y * a.z * r - a.x * s,
        a.z * a.z * r + c
    );
    return mul(m, p);
}

// ①Vertex Shader
// ②Geometry Shader
// ③Fragment Shader

// Vertex シェーダー
appdata_t vert(appdata_t v)
{
    return v; // 何もせず返すだけ～～～～
}

[maxvertexcount(3)] // 作成する頂点の最大数の宣言

// void 関数名(トライアングルリスト 入力データ型 名前[要素数], inout トライアングルプリミティブのシーケンス<出力データ型> 名前)
void geom(triangle appdata_t input[3], inout TriangleStream<g2f> stream)
{
    float3 center = (input[0].vertex + input[1].vertex + input[2].vertex).xyz / 3; // ローカル座標のセンター

    float3 vec1 = input[1].vertex - input[0].vertex;
    float3 vec2 = input[2].vertex - input[0].vertex;
    float3 normal = normalize(cross(vec1, vec2));

#ifdef _METHOD_PROPERTY
    fixed destruction = _Destruction;
#else
    float4 worldPos = mul(unity_ObjectToWorld, float4(center, 1.0)); // ローカル座標にかけることでワールド座標を取得するマトリクス, wが空の時は1を入れとけばいいらしい・・・
    float3 dist = length(_WorldSpaceCameraPos - worldPos); // _WorldSpaceCameraPos : ワールド座標系のカメラ位置, length : DirectX HLSLの組み込み関数
    fixed destruction = clamp((_StartDistance - dist) / (_StartDistance - _EndDistance), 0.0, 1.0); // カメラとセンターの距離が大きいほど、破壊は小さく、距離が小さいほど、破壊は大きくなるように
#endif

    fixed r = 2 * (rand(center.xy) - 0.5); // ランダムな値をそのままrにいれるのでよさそうな気が・・・
    fixed3 r3 = fixed3(r, r, r);

    [unroll] // for文使う時の決まり文句
    for (int i = 0; i < 3; ++i)
    {
        appdata_t v = input[i]; // Vertexシェーダーで使った構造体や

        g2f o;
        UNITY_SETUP_INSTANCE_ID(v); // GPU Instancingに対応するのに必要っぽい、無視
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // VR用シングルパスステレオレンダリングのマクロ、とりあえず無視

        v.vertex.xyz = (v.vertex.xyz - center) * (1.0 - destruction * (1.0 -_ScaleFactor)) + center; // destructionが大きいと散る、ScaleFactorは小さいと小さくなる
        v.vertex.xyz = rotate(v.vertex.xyz - center, r3 * destruction * _RotationFactor) + center;
        v.vertex.xyz += normal * destruction * _PositionFactor * r3;

        o.vertex = UnityObjectToClipPos(v.vertex);
#ifdef SOFTPARTICLES_ON
        o.projPos = ComputeScreenPos(o.vertex);
        COMPUTE_EYEDEPTH(o.projPos.z);
#endif

        o.color = v.color;
        o.color.a *= 1.0 - destruction * _AlphaFactor;
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        UNITY_TRANSFER_FOG(o, o.vertex);

        stream.Append(o);
    }
    stream.RestartStrip();
}

fixed4 frag(g2f i) : SV_Target
{
#ifdef SOFTPARTICLES_ON
	// ↑いまいちよくわからんけど、ブレンドしてエッジを目立たせなくできる的な。
	// Deferred Lightingかcameraのdepth texture modeをonにしないといけないっぽい。

    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
    float partZ = i.projPos.z;
    float fade = saturate(_InvFade * (sceneZ - partZ));
    i.color.a *= fade;
#endif

    //fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
	fixed4 col = i.color * tex2D(_MainTex, i.texcoord);
    UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0, 0, 0, 0)); // fogの影響受けさせるなら必要っぽい
    return col;
}

ENDCG

SubShader
{

Tags
{
    "RenderType" = "Transparent"
	"RenderType" = "Opaque"
    "Queue" = "Transparent"
    "IgnoreProjector" = "True"
    "PreviewType" = "Plane"
}

//Blend SrcAlpha One // 描画しようとしている色 * alpha + 現在の色 * 1 
Blend SrcAlpha OneMinusSrcAlpha // 一般的なやつ
ColorMask RGB
//Cull Off Lighting Off ZWrite Off
Lighting Off

Pass
{
    CGPROGRAM
    #pragma vertex vert
    #pragma geometry geom
    #pragma fragment frag
    #pragma target 5.0
    #pragma multi_compile_particles
    #pragma multi_compile_fog // fog...
    #pragma multi_compile _METHOD_PROPERTY _METHOD_CAMERA
    ENDCG
}

}

}