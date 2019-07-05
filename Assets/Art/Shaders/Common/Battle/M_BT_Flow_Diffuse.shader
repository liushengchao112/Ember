/// - no Specular Color 
// - specular lighting directions are approximated per vertex
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - supports ONLY 1 directional light. Other lights are completely ignored.

Shader "Vacuum/FlowMobile/Base_Transparent/Mobile_Flow_Diffuse" {
Properties {
		_BaseColor ("Base Color (A)", Color) = (1, 1, 1, 1)
		_MainTex ("Base Texture", 2D) = "" {}
		_FlowColor ("Flow Color (RGB) Specular(A)", Color) = (1, 1, 1, 1)
		_FlowTexture ("Flow Texture", 2D) = ""{}
		_FlowMap ("FlowMap (RG) Alpha (B) Gradient (A)", 2D) = ""{}
		_RevealSize("Flow Reveal Size", Range(-1, 1)) = 1
		_DistorScale("Distortion Scale", float) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "FlowTag"="Flow"}
	    LOD 200

		ZWrite On
		
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd novertexlights alpha

		fixed4 _BaseColor;
		sampler2D _MainTex;
		fixed4 _FlowColor;
		sampler2D _FlowTexture;
		sampler2D _FlowMap;
		half _PhaseLength;
		float4 _FlowMapOffset;

		fixed _DistorScale;

		half _RevealSize;	

		struct Input
		 {
			float2 uv_MainTex;
			float2 uv_FlowTexture;
			float2 uv_FlowMap;
			float4 color: Color;
		};

		void surf (Input IN, inout SurfaceOutput o)
		 {
			half4 flowMap = tex2D (_FlowMap, IN.uv_FlowMap);
			flowMap.r = flowMap.r * 2.0f - 1.011765;
			flowMap.g = flowMap.g * 2.0f - 1.003922;

			//Gradient
			half gradient = _RevealSize;
			gradient += flowMap.a;
			gradient = clamp(gradient, 0, 1);
			
			flowMap.b *= gradient; 
			////////////////////////////////////////


			half4 t1 = tex2D (_FlowTexture, IN.uv_FlowTexture + flowMap.rg * (_FlowMapOffset.x)); 		 	
			half4 t2 = tex2D (_FlowTexture, IN.uv_FlowTexture + flowMap.rg * (_FlowMapOffset.y)); 		 	
			
			half blend = abs(_PhaseLength - _FlowMapOffset.z) / _PhaseLength;
			blend = max(0, blend);
			half4 final = lerp( t1, t2, blend);

			half flowMapColor = flowMap.b * _FlowColor.a;

			float2 distUV = (final.xy / (final.w * 2 - 1)) * _DistorScale * flowMap.b;
			half4 mainColor = tex2D (_MainTex, IN.uv_MainTex + distUV);

			mainColor.rgb *= _BaseColor * (1 - flowMapColor);
			final.rgb *= _FlowColor.rgb * flowMapColor;

			o.Albedo = mainColor.rgb + final.rgb;
			o.Alpha = flowMap.b * _BaseColor.a * IN.color.a;
		}
		ENDCG
	} 
	Fallback "Mobile/VertexLit"
}
