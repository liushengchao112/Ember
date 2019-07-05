// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Ember/FlagWave"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _Width ("Width", float) = 1
        _Wave ("Wave", int) = 3
        _Amplitude ("Amplitude", float) = 1
        _Speed ("Speed", float) = 1
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

            float _Width;
            int _Wave;
            float _Amplitude;
            float _Speed;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
            v2f vert (appdata v)
            {
                v2f o;
                float uStartAngle = _Speed * _Time.y;
                float angleSpanH = _Wave * 3.14159265;
                float startX = -_Width / 2.0;

                float currAngle = uStartAngle + ((v.vertex.x - startX) / _Width) * angleSpanH;
                float tz = cos(currAngle) * _Amplitude;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x, v.vertex.y, v.vertex.z + tz, 1));

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
