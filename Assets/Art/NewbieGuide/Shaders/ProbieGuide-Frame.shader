Shader "Bot/ProbieGuide-Frame" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		
		Lighting Off 
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
        Fog { Mode Off } 

		Pass {
			SetTexture [_MainTex] {}
		}
	} 
}
