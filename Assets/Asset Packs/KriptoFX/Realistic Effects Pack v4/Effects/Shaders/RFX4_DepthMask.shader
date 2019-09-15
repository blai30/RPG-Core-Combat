Shader "KriptoFX/RFX4/Decal/DepthMask" {
	Properties{
		_Cutoff("Cutoff", Range(0,1)) = 0
		_Mask("Mask", 2D) = "white" {}
	}
	SubShader{
			Tags{ "Queue" = "Geometry-1" }
			ColorMask 0
			ZWrite On
			CGPROGRAM

		#pragma surface surf Standard

	sampler2D _Mask;
	float _Cutoff;

		struct Input {
			float2 uv_Mask;
		};

		half _Glossiness;
		half _Metallic;
		half4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float4 tex = tex2D(_Mask, IN.uv_Mask);
			clip(tex.r - _Cutoff);
		}
		ENDCG
	}

}
