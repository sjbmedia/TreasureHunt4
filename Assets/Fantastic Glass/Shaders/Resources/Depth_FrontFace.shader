Shader "Depth/FrontFace" {
	Properties{
		_MainTex("", 2D) = "white" {}
		_Cutoff("", Float) = 0.5
		_Color("", Color) = (1,1,1,1)
	}

		SubShader{
			Tags { "RenderType" = "Opaque" }
			Pass {
				Cull Back
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		struct v2f {
			float4 pos : SV_POSITION;
			float4 nz : TEXCOORD0;
		};
		v2f vert(appdata_base v) {
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.nz.xyz = COMPUTE_VIEW_NORMAL;
			o.nz.w = COMPUTE_DEPTH_01;
			return o;
		}
		fixed4 frag(v2f i) : SV_Target{
			return EncodeDepthNormal (i.nz.w, i.nz.xyz);
		}
		ENDCG
			}
		}

			Fallback Off
}
