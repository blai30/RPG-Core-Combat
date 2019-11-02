// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "IL3DN/Grass"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.5
		[NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
		[Toggle(_WIND_ON)] _Wind("Wind", Float) = 1
		_WindStrenght("Wind Strenght", Range( 0 , 1)) = 0.5
		[Toggle(_WIGGLE_ON)] _Wiggle("Wiggle", Float) = 1
		_WiggleStrenght("Wiggle Strenght", Range( 0 , 1)) = 0.5
    }


    SubShader
    {
		
        Tags { "RenderPipeline"="LightweightPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }

		Cull Off
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL
		
        Pass
        {
			
        	Tags { "LightMode"="LightweightForward" }

        	Name "Base"
			Blend One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
            
        	HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 60900
            #define _SPECULAR_SETUP 1
            #define _AlphaClip 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

        	// -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            
        	// -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
        	#pragma fragment frag

        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
		
			#pragma multi_compile __ _WIND_ON
			#pragma multi_compile __ _WIGGLE_ON


			float3 WindDirection;
			float WindSpeedFloat;
			float WindTurbulenceFloat;
			float WindStrenghtFloat;
			sampler2D _MainTex;
			float GrassWiggleFloat;
			CBUFFER_START( UnityPerMaterial )
			float _WindStrenght;
			float4 _Color;
			float _WiggleStrenght;
			float _AlphaCutoff;
			CBUFFER_END

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
                float4 ase_tangent : TANGENT;
                float4 texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct GraphVertexOutput
            {
                float4 clipPos                : SV_POSITION;
                float4 lightmapUVOrVertexSH	  : TEXCOORD0;
        		half4 fogFactorAndVertexLight : TEXCOORD1; // x: fogFactor, yzw: vertex light
            	float4 shadowCoord            : TEXCOORD2;
				float4 tSpace0					: TEXCOORD3;
				float4 tSpace1					: TEXCOORD4;
				float4 tSpace2					: TEXCOORD5;
				float4 ase_texcoord7 : TEXCOORD7;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            	UNITY_VERTEX_OUTPUT_STEREO
            };

			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

            GraphVertexOutput vert (GraphVertexInput v  )
        	{
        		GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
            	UNITY_TRANSFER_INSTANCE_ID(v, o);
        		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float mulTime986 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime986).xx;
				float simplePerlin2D985 = snoise( temp_cast_0 );
				float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise1038 = simplePerlin2D712;
				float4 transform1029 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D985 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise1038 ) + ( worldNoise1038 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch1031 = transform1029;
				#else
				float4 staticSwitch1031 = float4( 0,0,0,0 );
				#endif
				
				o.ase_texcoord7.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch1031.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal =  v.ase_normal ;

        		// Vertex shader outputs defined by graph
                float3 lwWNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
				float3 lwWTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float3 lwWBinormal = normalize(cross(lwWNormal, lwWTangent) * v.ase_tangent.w);
				o.tSpace0 = float4(lwWTangent.x, lwWBinormal.x, lwWNormal.x, lwWorldPos.x);
				o.tSpace1 = float4(lwWTangent.y, lwWBinormal.y, lwWNormal.y, lwWorldPos.y);
				o.tSpace2 = float4(lwWTangent.z, lwWBinormal.z, lwWNormal.z, lwWorldPos.z);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                
         		// We either sample GI from lightmap or SH.
        	    // Lightmap UV and vertex SH coefficients use the same interpolator ("float2 lightmapUV" for lightmap or "half3 vertexSH" for SH)
                // see DECLARE_LIGHTMAP_OR_SH macro.
        	    // The following funcions initialize the correct variable with correct data
        	    OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
        	    OUTPUT_SH(lwWNormal, o.lightmapUVOrVertexSH.xyz);

        	    half3 vertexLight = VertexLighting(vertexInput.positionWS, lwWNormal);
        	    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
        	    o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
        	    o.clipPos = vertexInput.positionCS;

        	#ifdef _MAIN_LIGHT_SHADOWS
        		o.shadowCoord = GetShadowCoord(vertexInput);
        	#endif
        		return o;
        	}

        	half4 frag (GraphVertexOutput IN  ) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

        		float3 WorldSpaceNormal = normalize(float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z));
				float3 WorldSpaceTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldSpaceBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldSpacePosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldSpaceViewDirection = SafeNormalize( _WorldSpaceCameraPos.xyz  - WorldSpacePosition );
    
				float2 uv0746 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (WorldSpacePosition).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise1038 = simplePerlin2D712;
				float2 temp_cast_1 = (( worldNoise1038 * 2 )).xx;
				float simplePerlin2D797 = snoise( temp_cast_1 );
				float cos745 = cos( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
				float sin745 = sin( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
				float2 rotator745 = mul( uv0746 - float2( 0.5,0 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.5,0 );
				#ifdef _WIGGLE_ON
				float2 staticSwitch1033 = rotator745;
				#else
				float2 staticSwitch1033 = uv0746;
				#endif
				float4 tex2DNode97 = tex2D( _MainTex, staticSwitch1033 );
				
				float3 temp_cast_3 = (0.0).xxx;
				
				
		        float3 Albedo = ( _Color * tex2DNode97 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = temp_cast_3;
				float Metallic = 0.0;
				float Smoothness = 0.0;
				float Occlusion = 1;
				float Alpha = tex2DNode97.a;
				float AlphaClipThreshold = _AlphaCutoff;

        		InputData inputData;
        		inputData.positionWS = WorldSpacePosition;

        #ifdef _NORMALMAP
        	    inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal)));
        #else
            #if !SHADER_HINT_NICE_QUALITY
                inputData.normalWS = WorldSpaceNormal;
            #else
        	    inputData.normalWS = normalize(WorldSpaceNormal);
            #endif
        #endif

        #if !SHADER_HINT_NICE_QUALITY
        	    // viewDirection should be normalized here, but we avoid doing it as it's close enough and we save some ALU.
        	    inputData.viewDirectionWS = WorldSpaceViewDirection;
        #else
        	    inputData.viewDirectionWS = normalize(WorldSpaceViewDirection);
        #endif

        	    inputData.shadowCoord = IN.shadowCoord;

        	    inputData.fogCoord = IN.fogFactorAndVertexLight.x;
        	    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
        	    inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS);

        		half4 color = LightweightFragmentPBR(
        			inputData, 
        			Albedo, 
        			Metallic, 
        			Specular, 
        			Smoothness, 
        			Occlusion, 
        			Emission, 
        			Alpha);

			#ifdef TERRAIN_SPLAT_ADDPASS
				color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
			#else
				color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
			#endif

        #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

		#if ASE_LW_FINAL_COLOR_ALPHA_MULTIPLY
				color.rgb *= color.a;
		#endif
		
		#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
		#endif
        		return color;
            }

        	ENDHLSL
        }

		
        Pass
        {
			
        	Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 60900
            #define _AlphaClip 1

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

            #pragma multi_compile __ _WIND_ON
            #pragma multi_compile __ _WIGGLE_ON


            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			float3 WindDirection;
			float WindSpeedFloat;
			float WindTurbulenceFloat;
			float WindStrenghtFloat;
			sampler2D _MainTex;
			float GrassWiggleFloat;
			CBUFFER_START( UnityPerMaterial )
			float _WindStrenght;
			float4 _Color;
			float _WiggleStrenght;
			float _AlphaCutoff;
			CBUFFER_END

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord7 : TEXCOORD7;
                float4 ase_texcoord8 : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
        	};

			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

            // x: global clip space bias, y: normal world space bias
            float3 _LightDirection;

            VertexOutput ShadowPassVertex(GraphVertexInput v )
        	{
        	    VertexOutput o;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO (o);

				float mulTime986 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime986).xx;
				float simplePerlin2D985 = snoise( temp_cast_0 );
				float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise1038 = simplePerlin2D712;
				float4 transform1029 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D985 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise1038 ) + ( worldNoise1038 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch1031 = transform1029;
				#else
				float4 staticSwitch1031 = float4( 0,0,0,0 );
				#endif
				
				o.ase_texcoord8.xyz = ase_worldPos;
				
				o.ase_texcoord7.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				o.ase_texcoord8.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch1031.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

        	    float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

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

               float2 uv0746 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
               float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
               float3 ase_worldPos = IN.ase_texcoord8.xyz;
               float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
               float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
               float worldNoise1038 = simplePerlin2D712;
               float2 temp_cast_1 = (( worldNoise1038 * 2 )).xx;
               float simplePerlin2D797 = snoise( temp_cast_1 );
               float cos745 = cos( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
               float sin745 = sin( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
               float2 rotator745 = mul( uv0746 - float2( 0.5,0 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.5,0 );
               #ifdef _WIGGLE_ON
               float2 staticSwitch1033 = rotator745;
               #else
               float2 staticSwitch1033 = uv0746;
               #endif
               float4 tex2DNode97 = tex2D( _MainTex, staticSwitch1033 );
               

				float Alpha = tex2DNode97.a;
				float AlphaClipThreshold = _AlphaCutoff;

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
			ColorMask 0

            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 60900
            #define _AlphaClip 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #pragma multi_compile __ _WIND_ON
            #pragma multi_compile __ _WIGGLE_ON


			float3 WindDirection;
			float WindSpeedFloat;
			float WindTurbulenceFloat;
			float WindStrenghtFloat;
			sampler2D _MainTex;
			float GrassWiggleFloat;
			CBUFFER_START( UnityPerMaterial )
			float _WindStrenght;
			float4 _Color;
			float _WiggleStrenght;
			float _AlphaCutoff;
			CBUFFER_END

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord : TEXCOORD0;
                float4 ase_texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			
           

            VertexOutput vert(GraphVertexInput v  )
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float mulTime986 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime986).xx;
				float simplePerlin2D985 = snoise( temp_cast_0 );
				float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise1038 = simplePerlin2D712;
				float4 transform1029 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D985 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise1038 ) + ( worldNoise1038 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch1031 = transform1029;
				#else
				float4 staticSwitch1031 = float4( 0,0,0,0 );
				#endif
				
				o.ase_texcoord1.xyz = ase_worldPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				o.ase_texcoord1.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch1031.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

        	    o.clipPos = TransformObjectToHClip(v.vertex.xyz);
        	    return o;
            }

            half4 frag(VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

				float2 uv0746 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise1038 = simplePerlin2D712;
				float2 temp_cast_1 = (( worldNoise1038 * 2 )).xx;
				float simplePerlin2D797 = snoise( temp_cast_1 );
				float cos745 = cos( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
				float sin745 = sin( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
				float2 rotator745 = mul( uv0746 - float2( 0.5,0 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.5,0 );
				#ifdef _WIGGLE_ON
				float2 staticSwitch1033 = rotator745;
				#else
				float2 staticSwitch1033 = uv0746;
				#endif
				float4 tex2DNode97 = tex2D( _MainTex, staticSwitch1033 );
				

				float Alpha = tex2DNode97.a;
				float AlphaClipThreshold = _AlphaCutoff;

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

        // This pass it not used during regular rendering, only for lightmap baking.
		
        Pass
        {
			
        	Name "Meta"
            Tags { "LightMode"="Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 60900
            #define _AlphaClip 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

			
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/MetaInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #pragma multi_compile __ _WIND_ON
            #pragma multi_compile __ _WIGGLE_ON


			float3 WindDirection;
			float WindSpeedFloat;
			float WindTurbulenceFloat;
			float WindStrenghtFloat;
			sampler2D _MainTex;
			float GrassWiggleFloat;
			CBUFFER_START( UnityPerMaterial )
			float _WindStrenght;
			float4 _Color;
			float _WiggleStrenght;
			float _AlphaCutoff;
			CBUFFER_END

            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION


            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord : TEXCOORD0;
                float4 ase_texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

            VertexOutput vert(GraphVertexInput v  )
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float mulTime986 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime986).xx;
				float simplePerlin2D985 = snoise( temp_cast_0 );
				float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise1038 = simplePerlin2D712;
				float4 transform1029 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D985 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise1038 ) + ( worldNoise1038 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch1031 = transform1029;
				#else
				float4 staticSwitch1031 = float4( 0,0,0,0 );
				#endif
				
				o.ase_texcoord1.xyz = ase_worldPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				o.ase_texcoord1.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch1031.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;
#if !defined( ASE_SRP_VERSION ) || ASE_SRP_VERSION  > 51300				
                o.clipPos = MetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST);
#else
				o.clipPos = MetaVertexPosition (v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST);
#endif
        	    return o;
            }

            half4 frag(VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

           		float2 uv0746 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
           		float3 worldToObjDir1015 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
           		float3 ase_worldPos = IN.ase_texcoord1.xyz;
           		float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir1015 * WindSpeedFloat ).xy + (ase_worldPos).xz);
           		float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
           		float worldNoise1038 = simplePerlin2D712;
           		float2 temp_cast_1 = (( worldNoise1038 * 2 )).xx;
           		float simplePerlin2D797 = snoise( temp_cast_1 );
           		float cos745 = cos( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
           		float sin745 = sin( ( simplePerlin2D797 * GrassWiggleFloat * _WiggleStrenght ) );
           		float2 rotator745 = mul( uv0746 - float2( 0.5,0 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.5,0 );
           		#ifdef _WIGGLE_ON
           		float2 staticSwitch1033 = rotator745;
           		#else
           		float2 staticSwitch1033 = uv0746;
           		#endif
           		float4 tex2DNode97 = tex2D( _MainTex, staticSwitch1033 );
           		
				
		        float3 Albedo = ( _Color * tex2DNode97 ).rgb;
				float3 Emission = 0;
				float Alpha = tex2DNode97.a;
				float AlphaClipThreshold = _AlphaCutoff;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = Albedo;
                metaInput.Emission = Emission;
                
                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
		
    }
    Fallback "Hidden/InternalErrorShader"
	CustomEditor "ASEMaterialInspector"
	
}
/*ASEBEGIN
Version=17009
153;409;1673;1007;-3110.743;-638.0487;1.3;True;True
Node;AmplifyShaderEditor.CommentaryNode;1037;1367.12,1452.801;Inherit;False;1510.231;587.8159;World Noise;11;712;751;750;749;706;918;1019;714;1015;708;1018;;1,0,0.02020931,1;0;0
Node;AmplifyShaderEditor.Vector3Node;867;1057.914,1357.966;Float;False;Global;WindDirection;WindDirection;14;0;Create;True;0;0;False;0;0,0,0;-0.7071068,0,-0.7071068;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;1018;1417.681,1749.989;Inherit;False;FLOAT3;0;2;1;2;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;708;1427.754,1519.424;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformDirectionNode;1015;1666.559,1750.773;Inherit;False;World;Object;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;714;1623.195,1934.13;Float;False;Global;WindSpeedFloat;WindSpeedFloat;3;0;Create;True;0;0;False;0;2;2;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1019;1957.423,1890.921;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;918;1739.673,1516.475;Inherit;False;FLOAT2;0;2;2;2;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;706;2139.862,1524.508;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;750;2196.823,1844.52;Float;False;Global;WindTurbulenceFloat;WindTurbulenceFloat;4;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;749;2344.651,1523.283;Inherit;False;0.25;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;751;2510.797,1526.13;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;712;2661.281,1523.776;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1041;2058.232,2401.528;Inherit;False;806.2452;680.9117;Vertex Animation;8;754;857;755;1030;854;855;856;853;;0,1,0.8708036,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1038;2941.43,1527.926;Float;False;worldNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1039;1871.658,2708.265;Inherit;False;1038;worldNoise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1040;1641.561,812.3293;Inherit;False;1038;worldNoise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;853;2112.974,2490.024;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;856;2108.345,2900.443;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1034;1872.214,747.3168;Inherit;False;1007.189;586.5881;UV Animation;7;745;799;746;798;797;1035;795;;0.7678117,1,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;854;2346.273,2578.697;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;993;2487.22,2076.615;Inherit;False;387.5991;274.1141;Strenght Noise;2;985;986;;1,0.6156863,0,1;0;0
Node;AmplifyShaderEditor.ScaleNode;795;1908.579,812.6546;Inherit;False;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;855;2351.035,2800.589;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;986;2501.715,2122.83;Inherit;False;1;0;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;797;2095.125,806.8777;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;798;2023.787,1052.705;Float;False;Global;GrassWiggleFloat;GrassWiggleFloat;4;0;Create;True;0;0;False;0;0.26;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1035;2025.109,1216.133;Float;False;Property;_WiggleStrenght;Wiggle Strenght;6;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;857;2531.034,2683.803;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1030;2337.359,2929.482;Float;False;Global;WindStrenghtFloat;WindStrenghtFloat;3;0;Create;True;0;0;False;0;0.5;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;755;2344.516,2484.129;Float;False;Property;_WindStrenght;Wind Strenght;4;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;746;2344.942,813.6662;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;985;2670.07,2123.101;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;2701.919,2661.832;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;799;2439.094,1033.282;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1028;2980.049,2129.731;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;745;2617.415,902.978;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;1033;3577.498,858.7823;Float;False;Property;_Wiggle;Wiggle;5;0;Create;True;0;0;False;0;1;1;1;True;_WIND_ON;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;872;3167.726,1367.195;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;292;3969.53,927.8489;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;0.3764702,0.470588,0.188235,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldToObjectTransfNode;1029;3341.557,1372.28;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;97;3893.026,1123.991;Inherit;True;Property;_MainTex;MainTex;2;1;[NoScaleOffset];Create;True;0;0;False;0;None;a73c218e0d8156240a793d22710686d1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;1031;3576.195,1345.762;Float;False;Property;_Wind;Wind;3;0;Create;True;0;0;False;0;1;1;1;True;_WIND_ON;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;293;4335.922,1060.561;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1036;3903.526,1334.934;Float;False;Property;_AlphaCutoff;Alpha Cutoff;1;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1046;4332.093,1163.248;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1044;4525.362,1096.846;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1045;4525.362,1096.846;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1043;4525.362,1096.846;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1042;4553.673,1057.39;Float;False;True;2;ASEMaterialInspector;0;2;IL3DN/Grass;1976390536c6c564abb90fe41f6ee334;True;Base;0;0;Base;11;False;False;False;True;2;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=TransparentCutout=RenderType;Queue=AlphaTest=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=LightweightForward;False;0;Hidden/InternalErrorShader;0;0;Standard;3;Vertex Position,InvertActionOnDeselection;1;Receive Shadows;1;LOD CrossFade;1;1;_FinalColorxAlpha;0;4;True;True;True;True;False;0
WireConnection;1018;0;867;0
WireConnection;1015;0;1018;0
WireConnection;1019;0;1015;0
WireConnection;1019;1;714;0
WireConnection;918;0;708;0
WireConnection;706;0;918;0
WireConnection;706;2;1019;0
WireConnection;749;0;706;0
WireConnection;751;0;749;0
WireConnection;751;1;750;0
WireConnection;712;0;751;0
WireConnection;1038;0;712;0
WireConnection;854;0;853;4
WireConnection;854;1;1039;0
WireConnection;795;0;1040;0
WireConnection;855;0;1039;0
WireConnection;855;1;856;2
WireConnection;797;0;795;0
WireConnection;857;0;854;0
WireConnection;857;1;855;0
WireConnection;985;0;986;0
WireConnection;754;0;755;0
WireConnection;754;1;857;0
WireConnection;754;2;1030;0
WireConnection;799;0;797;0
WireConnection;799;1;798;0
WireConnection;799;2;1035;0
WireConnection;1028;0;985;0
WireConnection;1028;1;754;0
WireConnection;745;0;746;0
WireConnection;745;2;799;0
WireConnection;1033;1;746;0
WireConnection;1033;0;745;0
WireConnection;872;0;867;0
WireConnection;872;1;1028;0
WireConnection;1029;0;872;0
WireConnection;97;1;1033;0
WireConnection;1031;0;1029;0
WireConnection;293;0;292;0
WireConnection;293;1;97;0
WireConnection;1042;0;293;0
WireConnection;1042;9;1046;0
WireConnection;1042;3;1046;0
WireConnection;1042;4;1046;0
WireConnection;1042;6;97;4
WireConnection;1042;7;1036;0
WireConnection;1042;8;1031;0
ASEEND*/
//CHKSM=B2417C554108FDCE406CAD8D1B5AFD1BCA37A76A