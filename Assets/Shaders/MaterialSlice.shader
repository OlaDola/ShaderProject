Shader "Custom/SliceURP"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0

        sliceNormal("Normal", Vector) = (0,0,0,0)
        sliceCentre("Centre", Vector) = (0,0,0,0)
        sliceOffsetDst("Offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Gemometry" "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 positionHCS : SV_POSITION;
            };

            sampler2D _MainTex;
            // float4 _MainTex_ST;
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Glossiness;
            float _Metallic;
            float3 sliceNormal;
            float3 sliceCentre;
            float sliceOffsetDst;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 adjustedCentre = sliceCentre + sliceNormal * sliceOffsetDst;
                float3 offsetToSliceCentre = adjustedCentre - IN.worldPos;
                clip(dot(offsetToSliceCentre, sliceNormal));
                
                float4 texColor = tex2D(_MainTex, IN.uv) * _Color;
                
                SurfaceData surfaceData;
                surfaceData.albedo = texColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Glossiness;
                surfaceData.alpha = texColor.a;
                surfaceData.normalTS = float3(0, 0, 1);
                surfaceData.emission = 0;
                surfaceData.occlusion = 1;
                surfaceData.specular = 0;
                
                InputData inputData;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.worldPos);
                inputData.shadowCoord = float4(0, 0, 0, 0);
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = 0;
                
                half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
