Shader "Custom/AnaglyphURP"
{
    Properties
    {
        _LeftTex ("Left Eye", 2D) = "white" {}
        _RightTex ("Right Eye", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_LeftTex);  SAMPLER(sampler_LeftTex);
            TEXTURE2D(_RightTex); SAMPLER(sampler_RightTex);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 left = SAMPLE_TEXTURE2D(_LeftTex, sampler_LeftTex, IN.uv);
                half4 right = SAMPLE_TEXTURE2D(_RightTex, sampler_RightTex, IN.uv);
                return half4(left.r, right.g, right.b, 1);
            }
            ENDHLSL
        }
    }
}