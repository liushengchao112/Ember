// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:Transparent/Cutout/Diffuse,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:False,enco:False,rmgx:True,imps:False,rpth:0,vtps:0,hqsc:False,nrmq:0,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:True,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:True,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:0,x:34000,y:32640,varname:node_0,prsc:2|diff-1-RGB,emission-1-RGB,clip-1-A,voffset-394-OUT;n:type:ShaderForge.SFN_Tex2d,id:1,x:33711,y:32431,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:_Diffuse,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:66321cc856b03e245ac41ed8a53e0ecc,ntxv:0,isnm:False;n:type:ShaderForge.SFN_NormalVector,id:391,x:33073,y:33231,prsc:2,pt:True;n:type:ShaderForge.SFN_Time,id:392,x:33132,y:33651,varname:node_392,prsc:2;n:type:ShaderForge.SFN_Sin,id:393,x:33479,y:33548,varname:node_393,prsc:2|IN-413-OUT;n:type:ShaderForge.SFN_Multiply,id:394,x:33729,y:33420,cmnt:Wind animation,varname:node_394,prsc:2|A-562-OUT,B-1649-OUT,C-393-OUT;n:type:ShaderForge.SFN_Add,id:413,x:33298,y:33548,varname:node_413,prsc:2|A-520-OUT,B-392-TDB;n:type:ShaderForge.SFN_Pi,id:520,x:32919,y:33494,varname:node_520,prsc:2;n:type:ShaderForge.SFN_Add,id:561,x:33294,y:33171,varname:node_561,prsc:2|A-1574-XYZ,B-391-OUT;n:type:ShaderForge.SFN_Normalize,id:562,x:33529,y:33266,varname:node_562,prsc:2|IN-561-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1649,x:33354,y:33436,ptovrint:False,ptlb:node_1649,ptin:_node_1649,varname:node_1649,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.01;n:type:ShaderForge.SFN_Vector4Property,id:1574,x:32911,y:33080,ptovrint:False,ptlb:node_1574,ptin:_node_1574,varname:node_1574,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1,v2:0.1,v3:0.1,v4:0;proporder:1-1649-1574;pass:END;sub:END;*/

Shader "Ember/AnimateGrass" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _node_1649 ("node_1649", Float ) = 0.01
        _node_1574 ("node_1574", Vector) = (0.1,0.1,0.1,0)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            AlphaToMask On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _node_1649;
            uniform float4 _node_1574;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_392 = _Time + _TimeEditor;
                v.vertex.xyz += (normalize((_node_1574.rgb+v.normal))*_node_1649*sin((3.141592654+node_392.b)));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip(_Diffuse_var.a - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = _Diffuse_var.rgb;
                float3 finalColor = emissive;
                return fixed4(finalColor,(_Diffuse_var.a) * 2.0 - 1.0);
            }
            ENDCG
        }
    }
    FallBack "Legacy Shaders/Transparent/Cutout/Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
