Shader "Unlit/PortalShaderCut"
{
    Properties
    {
		_MainTex("Main Texture", 2D) = "white" {}
		_InactiveColour ("Inactive Colour", Color) = (1, 1, 1, 1)
		displayMask ("Display Mask", Int) = 1
    }
    SubShader
    {
		Tags 
		{ 
			"RenderType" = "Opaque"
			// "Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
		}
		

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
		ENDHLSL

        Pass
        {
			Name "Mask"
			LOD 100
			Cull Off
			Stencil
			{
				Ref 1
				Pass replace
			}

			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct appdata
				{
					float4 vertex : POSITION;
				};

				// float4 _MainTex_ST;
				CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _InactiveColour;
				int displayMask;
				CBUFFER_END

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float4 screenPos : TEXCOORD0;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = TransformObjectToHClip(v.vertex.xyz);
					o.screenPos = ComputeScreenPos(o.vertex);
					return o;
				}

				uniform sampler2D _MainTex;

				float4 frag(v2f i) : SV_Target
				{
					// Ensure screenPos.w is valid to avoid division by zero
					if (i.screenPos.w <= 0.00001)
					{
						return float4(0, 0, 0, 1); // Return a default color (black) for invalid positions
					}

					float2 uv = i.screenPos.xy / i.screenPos.w;
					uv = saturate(uv); // Clamp UV coordinates to [0, 1]
					float4 col = tex2D(_MainTex, uv);
					return col * displayMask + _InactiveColour * (1 - displayMask);
				}
				
			ENDHLSL
        }
    }

}