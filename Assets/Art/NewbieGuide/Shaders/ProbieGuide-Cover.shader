Shader "Bot/ProbieGuide-Cover" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Cover Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent+1" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask RGBA
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }
	
	SubShader {
		Pass {
			SetTexture [_MainTex] {}
		}
	}	
}
}
