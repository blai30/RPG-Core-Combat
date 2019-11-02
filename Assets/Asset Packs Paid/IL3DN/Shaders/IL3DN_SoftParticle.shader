// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "IL3DN/SoftParticle"
{
    Properties
    {
		_Color("Color", Color) = (0.4764151,0.9408201,1,0)
		_Diffuse("Diffuse", 2D) = "white" {}
		_SoftParticleFactor("Soft Particle Factor", Range( 0 , 10)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
    }

    SubShader
    {
		

        Tags { "RenderPipeline"="LightweightPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

		
        Pass
        {
            Tags { "LightMode"="LightweightForward" }
            Name "Base"

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

            HLSLPROGRAM
            #define ASE_SRP_VERSION 50702
            #define REQUIRE_DEPTH_TEXTURE 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            // -------------------------------------
            // Lightweight Pipeline keywords
            #pragma shader_feature _SAMPLE_GI

            // -------------------------------------
            // Unity defined keywords
			#ifdef ASE_FOG
            #pragma multi_compile_fog
			#endif
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag


            // Lighting include is needed because of GI
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/UnlitInput.hlsl"

            

			sampler2D _Diffuse;
			uniform float4 _CameraDepthTexture_TexelSize;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color;
			float4 _Diffuse_ST;
			float _SoftParticleFactor;
			CBUFFER_END

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct GraphVertexOutput
            {
                float4 position : POSITION;
				#ifdef ASE_FOG
				float fogCoord : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

			
            GraphVertexOutput vert (GraphVertexInput v)
            {
                GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float3 vertexPos80 = v.vertex.xyz;
				float4 ase_clipPos80 = TransformObjectToHClip((vertexPos80).xyz);
				float4 screenPos80 = ComputeScreenPos(ase_clipPos80);
				o.ase_texcoord2 = screenPos80;
				
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue =  defaultVertexValue ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue; 
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;
                o.position = TransformObjectToHClip(v.vertex.xyz);
				#ifdef ASE_FOG
				o.fogCoord = ComputeFogFactor( o.position.z );
				#endif
                return o;
            }

            half4 frag (GraphVertexOutput IN ) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				float2 uv_Diffuse = IN.ase_texcoord1.xy * _Diffuse_ST.xy + _Diffuse_ST.zw;
				float4 tex2DNode9 = tex2D( _Diffuse, uv_Diffuse );
				
				float4 screenPos80 = IN.ase_texcoord2;
				float4 ase_screenPosNorm80 = screenPos80 / screenPos80.w;
				ase_screenPosNorm80.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm80.z : ase_screenPosNorm80.z * 0.5 + 0.5;
				float screenDepth80 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm80.xy ),_ZBufferParams);
				float distanceDepth80 = abs( ( screenDepth80 - LinearEyeDepth( ase_screenPosNorm80.z,_ZBufferParams ) ) / ( _SoftParticleFactor ) );
				
		        float3 Color = ( _Color * tex2DNode9 ).rgb;
		        float Alpha = ( tex2DNode9.a * saturate( distanceDepth80 ) * _Color.a );
		        float AlphaClipThreshold = 0;
			
			#if _AlphaClip
				clip(Alpha - AlphaClipThreshold);
			#endif

			#ifdef ASE_FOG
				Color = MixFog( Color, IN.fogCoord );
			#endif

			#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
			#endif

                return half4(Color, Alpha);
            }
            ENDHLSL
        }

		
        Pass
        {
			
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
			ZWrite On
			ColorMask 0

            HLSLPROGRAM
            #define ASE_SRP_VERSION 50702
            #define REQUIRE_DEPTH_TEXTURE 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            

			sampler2D _Diffuse;
			uniform float4 _CameraDepthTexture_TexelSize;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color;
			float4 _Diffuse_ST;
			float _SoftParticleFactor;
			CBUFFER_END

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // x: global clip space bias, y: normal world space bias
            float3 _LightDirection;

			
            VertexOutput ShadowPassVertex(GraphVertexInput v )
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
				float3 vertexPos80 = v.vertex.xyz;
				float4 ase_clipPos80 = TransformObjectToHClip((vertexPos80).xyz);
				float4 screenPos80 = ComputeScreenPos(ase_clipPos80);
				o.ase_texcoord1 = screenPos80;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue =  defaultVertexValue ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldDir(v.ase_normal.xyz);

                float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                float scale = invNdotL * _ShadowBias.y;

                // normal bias is negative since we want to apply an inset normal offset
                positionWS = _LightDirection * _ShadowBias.xxx + positionWS;
				positionWS = normalWS * scale.xxx + positionWS;
                float4 clipPos = TransformWorldToHClip(positionWS);

                // _ShadowBias.x sign depens on if platform has reversed z buffer
                //clipPos.z += _ShadowBias.x; 

            #if UNITY_REVERSED_Z
                clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
            #else
                clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
            #endif
                o.clipPos = clipPos;

                return o;
            }

            half4 ShadowPassFragment(VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
        		float2 uv_Diffuse = IN.ase_texcoord.xy * _Diffuse_ST.xy + _Diffuse_ST.zw;
        		float4 tex2DNode9 = tex2D( _Diffuse, uv_Diffuse );
        		float4 screenPos80 = IN.ase_texcoord1;
        		float4 ase_screenPosNorm80 = screenPos80 / screenPos80.w;
        		ase_screenPosNorm80.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm80.z : ase_screenPosNorm80.z * 0.5 + 0.5;
        		float screenDepth80 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm80.xy ),_ZBufferParams);
        		float distanceDepth80 = abs( ( screenDepth80 - LinearEyeDepth( ase_screenPosNorm80.z,_ZBufferParams ) ) / ( _SoftParticleFactor ) );
        		

				float Alpha = ( tex2DNode9.a * saturate( distanceDepth80 ) * _Color.a );
				float AlphaClipThreshold = AlphaClipThreshold;
			#if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
			#endif

			#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
			#endif
                return 0;
            }

            ENDHLSL
        }

		
        Pass
        {
			
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
			ZTest LEqual
			ColorMask 0

            HLSLPROGRAM
            #define ASE_SRP_VERSION 50702
            #define REQUIRE_DEPTH_TEXTURE 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            

			sampler2D _Diffuse;
			uniform float4 _CameraDepthTexture_TexelSize;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color;
			float4 _Diffuse_ST;
			float _SoftParticleFactor;
			CBUFFER_END

			struct GraphVertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

			
			VertexOutput vert( GraphVertexInput v  )
			{
					VertexOutput o = (VertexOutput)0;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					float3 vertexPos80 = v.vertex.xyz;
					float4 ase_clipPos80 = TransformObjectToHClip((vertexPos80).xyz);
					float4 screenPos80 = ComputeScreenPos(ase_clipPos80);
					o.ase_texcoord1 = screenPos80;
					
					o.ase_texcoord.xy = v.ase_texcoord.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord.zw = 0;
					#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
					#else
					float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue =  defaultVertexValue ;	
					#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
					#else
					v.vertex.xyz += vertexValue;
					#endif
					v.ase_normal =  v.ase_normal ;
					o.clipPos = TransformObjectToHClip(v.vertex.xyz);
					return o;
			}

            half4 frag( VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				float2 uv_Diffuse = IN.ase_texcoord.xy * _Diffuse_ST.xy + _Diffuse_ST.zw;
				float4 tex2DNode9 = tex2D( _Diffuse, uv_Diffuse );
				float4 screenPos80 = IN.ase_texcoord1;
				float4 ase_screenPosNorm80 = screenPos80 / screenPos80.w;
				ase_screenPosNorm80.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm80.z : ase_screenPosNorm80.z * 0.5 + 0.5;
				float screenDepth80 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm80.xy ),_ZBufferParams);
				float distanceDepth80 = abs( ( screenDepth80 - LinearEyeDepth( ase_screenPosNorm80.z,_ZBufferParams ) ) / ( _SoftParticleFactor ) );
				

				float Alpha = ( tex2DNode9.a * saturate( distanceDepth80 ) * _Color.a );
				float AlphaClipThreshold = AlphaClipThreshold;

			#if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
			#endif
                return 0;
			#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
			#endif
            }
            ENDHLSL
        }
		
    }
    Fallback "Hidden/InternalErrorShader"
	CustomEditor "ASEMaterialInspector"
	
}
/*ASEBEGIN
Version=17009
1073;499;1673;965;779.0806;322.979;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;81;-951.8075,279.1663;Float;False;Property;_SoftParticleFactor;Soft Particle Factor;2;0;Create;True;0;0;False;0;0.5;5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;82;-868.3976,103.8649;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;80;-605.8818,182.7348;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-455.9091,-51.02061;Inherit;True;Property;_Diffuse;Diffuse;1;0;Create;True;0;0;False;0;None;bb4b4370dcb8aa24a9ecb8aee5ce9648;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;85;-309.356,188.3893;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-386.909,-237.422;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;0.4764151,0.9408201,1,0;0.3025098,0.5723806,0.7450981,0.854902;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-66.4999,-107.2;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-65.05418,159.1748;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;91;136.6728,-109.3395;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPUnlit;e2514bdcf5e5399499a9eb24d175b9db;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;92;136.6728,-109.3395;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPUnlit;e2514bdcf5e5399499a9eb24d175b9db;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthOnly;True;0;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;90;152.6728,-107.3395;Float;False;True;2;ASEMaterialInspector;0;3;IL3DN/SoftParticle;e2514bdcf5e5399499a9eb24d175b9db;True;Base;0;0;Base;5;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=LightweightForward;False;0;Hidden/InternalErrorShader;0;0;Standard;4;Vertex Position,InvertActionOnDeselection;1;Receive Shadows;1;Built-in Fog;0;LOD CrossFade;0;0;3;True;True;True;False;0
WireConnection;80;1;82;0
WireConnection;80;0;81;0
WireConnection;85;0;80;0
WireConnection;10;0;2;0
WireConnection;10;1;9;0
WireConnection;83;0;9;4
WireConnection;83;1;85;0
WireConnection;83;2;2;4
WireConnection;90;0;10;0
WireConnection;90;1;83;0
ASEEND*/
//CHKSM=A600D3AB4D1703BA411123182226F9A2D6881231