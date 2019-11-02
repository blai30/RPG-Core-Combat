// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "IL3DN/Leaf"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle(_WIND_ON)] _Wind("Wind", Float) = 1
		_WindStrenght("Wind Strenght", Range( 0 , 1)) = 0.5
		[Toggle(_WIGGLE_ON)] _Wiggle("Wiggle", Float) = 1
		_WiggleStrenght("Wiggle Strenght", Range( 0 , 1)) = 1
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
			float LeavesWiggleFloat;
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

				float mulTime877 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime877).xx;
				float simplePerlin2D879 = snoise( temp_cast_0 );
				float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise905 = simplePerlin2D712;
				float4 transform886 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D879 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise905 ) + ( worldNoise905 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch897 = transform886;
				#else
				float4 staticSwitch897 = float4( 0,0,0,0 );
				#endif
				
				o.ase_texcoord7.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch897.xyz;
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
				float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (WorldSpacePosition).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise905 = simplePerlin2D712;
				float2 temp_cast_1 = (( worldNoise905 * 2.5 )).xx;
				float simplePerlin2D797 = snoise( temp_cast_1 );
				float cos745 = cos( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
				float sin745 = sin( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
				float2 rotator745 = mul( uv0746 - float2( 0.25,0.25 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.25,0.25 );
				#ifdef _WIGGLE_ON
				float2 staticSwitch898 = rotator745;
				#else
				float2 staticSwitch898 = uv0746;
				#endif
				float4 tex2DNode97 = tex2D( _MainTex, staticSwitch898 );
				
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
			float LeavesWiggleFloat;
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

				float mulTime877 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime877).xx;
				float simplePerlin2D879 = snoise( temp_cast_0 );
				float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise905 = simplePerlin2D712;
				float4 transform886 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D879 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise905 ) + ( worldNoise905 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch897 = transform886;
				#else
				float4 staticSwitch897 = float4( 0,0,0,0 );
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
				float3 vertexValue = staticSwitch897.xyz;
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
               float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
               float3 ase_worldPos = IN.ase_texcoord8.xyz;
               float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
               float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
               float worldNoise905 = simplePerlin2D712;
               float2 temp_cast_1 = (( worldNoise905 * 2.5 )).xx;
               float simplePerlin2D797 = snoise( temp_cast_1 );
               float cos745 = cos( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
               float sin745 = sin( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
               float2 rotator745 = mul( uv0746 - float2( 0.25,0.25 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.25,0.25 );
               #ifdef _WIGGLE_ON
               float2 staticSwitch898 = rotator745;
               #else
               float2 staticSwitch898 = uv0746;
               #endif
               float4 tex2DNode97 = tex2D( _MainTex, staticSwitch898 );
               

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
			float LeavesWiggleFloat;
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

				float mulTime877 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime877).xx;
				float simplePerlin2D879 = snoise( temp_cast_0 );
				float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise905 = simplePerlin2D712;
				float4 transform886 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D879 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise905 ) + ( worldNoise905 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch897 = transform886;
				#else
				float4 staticSwitch897 = float4( 0,0,0,0 );
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
				float3 vertexValue = staticSwitch897.xyz;
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
				float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise905 = simplePerlin2D712;
				float2 temp_cast_1 = (( worldNoise905 * 2.5 )).xx;
				float simplePerlin2D797 = snoise( temp_cast_1 );
				float cos745 = cos( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
				float sin745 = sin( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
				float2 rotator745 = mul( uv0746 - float2( 0.25,0.25 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.25,0.25 );
				#ifdef _WIGGLE_ON
				float2 staticSwitch898 = rotator745;
				#else
				float2 staticSwitch898 = uv0746;
				#endif
				float4 tex2DNode97 = tex2D( _MainTex, staticSwitch898 );
				

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
			float LeavesWiggleFloat;
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
				float mulTime877 = _Time.y * 0.25;
				float2 temp_cast_0 = (mulTime877).xx;
				float simplePerlin2D879 = snoise( temp_cast_0 );
				float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
				float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
				float worldNoise905 = simplePerlin2D712;
				float4 transform886 = mul(GetWorldToObjectMatrix(),float4( ( WindDirection * ( simplePerlin2D879 * ( _WindStrenght * ( ( v.ase_color.a * worldNoise905 ) + ( worldNoise905 * v.ase_color.g ) ) * WindStrenghtFloat ) ) ) , 0.0 ));
				#ifdef _WIND_ON
				float4 staticSwitch897 = transform886;
				#else
				float4 staticSwitch897 = float4( 0,0,0,0 );
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
				float3 vertexValue = staticSwitch897.xyz;
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
           		float3 worldToObjDir883 = mul( GetWorldToObjectMatrix(), float4( (WindDirection).xzy, 0 ) ).xyz;
           		float3 ase_worldPos = IN.ase_texcoord1.xyz;
           		float2 panner706 = ( 1.0 * _Time.y * ( worldToObjDir883 * WindSpeedFloat ).xy + (ase_worldPos).xz);
           		float simplePerlin2D712 = snoise( ( ( panner706 * 0.25 ) * WindTurbulenceFloat ) );
           		float worldNoise905 = simplePerlin2D712;
           		float2 temp_cast_1 = (( worldNoise905 * 2.5 )).xx;
           		float simplePerlin2D797 = snoise( temp_cast_1 );
           		float cos745 = cos( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
           		float sin745 = sin( ( simplePerlin2D797 * LeavesWiggleFloat * _WiggleStrenght ) );
           		float2 rotator745 = mul( uv0746 - float2( 0.25,0.25 ) , float2x2( cos745 , -sin745 , sin745 , cos745 )) + float2( 0.25,0.25 );
           		#ifdef _WIGGLE_ON
           		float2 staticSwitch898 = rotator745;
           		#else
           		float2 staticSwitch898 = uv0746;
           		#endif
           		float4 tex2DNode97 = tex2D( _MainTex, staticSwitch898 );
           		
				
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
153;409;1673;1007;-3658.594;-750.5889;1;True;True
Node;AmplifyShaderEditor.Vector3Node;867;817.415,1344.312;Float;False;Global;WindDirection;WindDirection;14;0;Create;True;0;0;False;0;0,0,0;-0.7071068,0,-0.7071068;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;909;1197.83,1453.756;Inherit;False;1616.924;573.676;World Noise;11;712;750;751;749;706;884;873;714;708;883;882;;1,0,0.02020931,1;0;0
Node;AmplifyShaderEditor.SwizzleNode;882;1296.722,1729.092;Inherit;False;FLOAT3;0;2;1;2;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;883;1504.722,1729.092;Inherit;False;World;Object;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;714;1461.028,1905.042;Float;False;Global;WindSpeedFloat;WindSpeedFloat;3;0;Create;True;0;0;False;0;10;2;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;708;1501.092,1526.954;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;873;1842.531,1522.594;Inherit;False;FLOAT2;0;2;2;2;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;884;1826.611,1874.677;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;706;2050.301,1525.508;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;750;2088.474,1711.146;Float;False;Global;WindTurbulenceFloat;WindTurbulenceFloat;4;0;Create;True;0;0;False;0;0.25;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;749;2261.701,1523.591;Inherit;False;0.25;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;751;2437.135,1523.685;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;712;2615.417,1518.595;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;908;1983.116,2406.605;Inherit;False;830.5812;671.3536;Vertex Animation;8;754;755;857;888;855;854;856;853;;0,1,0.8708036,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;905;2861.922,1520.478;Float;False;worldNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;907;1540.259,843.3633;Inherit;False;905;worldNoise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;899;1792.495,773.7919;Inherit;False;1012.714;535.89;UV Animation;7;795;799;746;745;797;904;798;;0.7678117,1,0,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;853;2036.065,2481.375;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;906;1729.908,2713.099;Inherit;False;905;worldNoise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;856;2040.763,2894.441;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;876;2420.847,2070.403;Inherit;False;390.5991;274.1141;Strenght Noise;2;879;877;;1,0.6156863,0,1;0;0
Node;AmplifyShaderEditor.ScaleNode;795;1836.776,845.15;Inherit;False;2.5;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;854;2306.502,2640.616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;855;2309.503,2778.177;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;857;2457.943,2701.816;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;797;2002.644,842.9209;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;798;1934.886,1116.013;Float;False;Global;LeavesWiggleFloat;LeavesWiggleFloat;5;0;Create;True;0;0;False;0;0.26;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;904;1934.416,1210.809;Float;False;Property;_WiggleStrenght;Wiggle Strenght;6;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;888;2316.906,2908.086;Float;False;Global;WindStrenghtFloat;WindStrenghtFloat;3;0;Create;True;0;0;False;0;0.5;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;877;2436.847,2119.71;Inherit;False;1;0;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;755;2310.631,2541.86;Float;False;Property;_WindStrenght;Wind Strenght;4;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;879;2609.243,2119.71;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;746;2276.372,835.3718;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;2639.659,2678.943;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;799;2365.897,1117.559;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;745;2544.405,976.8619;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.25,0.25;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;881;2916.681,2127.176;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;872;3207.503,1341.415;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;898;3693.987,1190.03;Float;False;Property;_Wiggle;Wiggle;5;0;Create;True;0;0;False;0;1;1;1;True;_WIND_ON;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;292;4011.016,958.6533;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;0.7058823,0.5882353,0.1843136,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;97;3955.139,1167.211;Inherit;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;False;0;None;6ab0f5f5ed2482e43a5ace7eeced19e6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldToObjectTransfNode;886;3401.475,1340.256;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;910;3967.228,1376.466;Float;False;Property;_AlphaCutoff;Alpha Cutoff;1;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;897;3701.915,1311.666;Float;False;Property;_Wind;Wind;3;0;Create;True;0;0;False;0;1;1;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;293;4285.25,1058.456;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;915;4284.094,1171.589;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;914;4478.815,1058.455;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;912;4478.815,1058.455;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;913;4478.815,1058.455;Float;False;False;2;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;911;4496.815,1057.455;Float;False;True;2;ASEMaterialInspector;0;2;IL3DN/Leaf;1976390536c6c564abb90fe41f6ee334;True;Base;0;0;Base;11;False;False;False;True;2;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=TransparentCutout=RenderType;Queue=AlphaTest=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=LightweightForward;False;0;Hidden/InternalErrorShader;0;0;Standard;3;Vertex Position,InvertActionOnDeselection;1;Receive Shadows;1;LOD CrossFade;1;1;_FinalColorxAlpha;0;4;True;True;True;True;False;0
WireConnection;882;0;867;0
WireConnection;883;0;882;0
WireConnection;873;0;708;0
WireConnection;884;0;883;0
WireConnection;884;1;714;0
WireConnection;706;0;873;0
WireConnection;706;2;884;0
WireConnection;749;0;706;0
WireConnection;751;0;749;0
WireConnection;751;1;750;0
WireConnection;712;0;751;0
WireConnection;905;0;712;0
WireConnection;795;0;907;0
WireConnection;854;0;853;4
WireConnection;854;1;906;0
WireConnection;855;0;906;0
WireConnection;855;1;856;2
WireConnection;857;0;854;0
WireConnection;857;1;855;0
WireConnection;797;0;795;0
WireConnection;879;0;877;0
WireConnection;754;0;755;0
WireConnection;754;1;857;0
WireConnection;754;2;888;0
WireConnection;799;0;797;0
WireConnection;799;1;798;0
WireConnection;799;2;904;0
WireConnection;745;0;746;0
WireConnection;745;2;799;0
WireConnection;881;0;879;0
WireConnection;881;1;754;0
WireConnection;872;0;867;0
WireConnection;872;1;881;0
WireConnection;898;1;746;0
WireConnection;898;0;745;0
WireConnection;97;1;898;0
WireConnection;886;0;872;0
WireConnection;897;0;886;0
WireConnection;293;0;292;0
WireConnection;293;1;97;0
WireConnection;911;0;293;0
WireConnection;911;9;915;0
WireConnection;911;3;915;0
WireConnection;911;4;915;0
WireConnection;911;6;97;4
WireConnection;911;7;910;0
WireConnection;911;8;897;0
ASEEND*/
//CHKSM=F21095A75B5F9A5C2E6A8B49E772B0CD3E90AB0F