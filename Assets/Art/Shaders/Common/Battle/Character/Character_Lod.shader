Shader "Ember/Character_Lod" {

    Properties {
    	_MainTex ("Base (RGB)", 2D) = "white" {}
        _Brightness ("Brightness", Float) = 1  
        _Contrast ("Contrast", Float) = 1
        _Alpha ("Alpha", Range(0, 1)) = 1
        _RimColor ("Rim Light Color", Color) = (1, 1, 1, 1)
    }

    SubShader {
    	Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "Queue"="Geometry" }
    	LOD 200
        
        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode"="ForwardBase" }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
                
            #include "UnityCG.cginc"

            float4 _LightColor0;

            sampler2D _MainTex;
            float _Brightness;
            float _Contrast;
            float _Alpha;
            fixed4 _RimColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                half2 texcoord0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                half2 uv : TEXCOORD0;
                fixed3 rimColor : TEXCOORD1;
                fixed3 vlight : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord0;

                float3 worldN = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.vlight = ShadeSH9(float4(worldN, 1.0));

                float3 worldV = WorldSpaceViewDir(v.vertex);
                float vDotN = max(0.0, 1.0 - dot(normalize(worldV), normalize(worldN)));
                o.rimColor = _RimColor.rgb * vDotN * vDotN * _RimColor.a;

                //float3 normalDirection = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                float3 reflection = _LightColor0.xyz * max(0.0, dot(worldN, normalize(_WorldSpaceLightPos0.xyz))); 
                o.color = float4(reflection, 1);

                //TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
                
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                fixed3 finalColor = color * (i.color * _Contrast + _Brightness) * i.vlight + i.rimColor;

                /*
                fixed gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;  
                fixed3 grayColor = fixed3(gray, gray, gray);

                finalColor = lerp(grayColor, finalColor, _Saturation);
                fixed3 avgColor = fixed3(0.5, 0.5, 0.5);  
                finalColor = lerp(avgColor, finalColor, _Contrast);
                */

                return fixed4(finalColor, _Alpha);
            }
            ENDCG
        }

        Pass
        {
            Name "Shadow"

            Stencil
            {
                Ref 0
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite off
            Offset 1 , 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            float3 ShadowProjectPos(float4 vertDir)
            {
                float3 shadowPos;

                float3 wPos = mul(unity_ObjectToWorld, vertDir).xyz;
                half3 lightDir = half3(-0.1, 0.1, -0.1);
                shadowPos.y = 0.05;
                shadowPos.xz = wPos.xz - lightDir.xz * (wPos.y - 0.05) * 10;
                //shadowPos = lerp( shadowPos , wPos,step(wPos.y - 0.05 , 0));

                return shadowPos;
            }

            v2f vert (appdata v)
            {
                v2f o;

                float3 shadowPos = ShadowProjectPos(v.vertex);
                o.vertex = UnityWorldToClipPos(shadowPos);
                float3 center = float3(unity_ObjectToWorld[0].w, 0.05, unity_ObjectToWorld[2].w);
                float falloff = saturate(1 - distance(shadowPos, center) * 0.2);
                o.color = fixed4(0.1, 0.1, 0.1, 1); 
                o.color.a = falloff;

                return o;
            }
                
            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

    }
}
