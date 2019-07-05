// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:33090,y:32614,varname:node_4795,prsc:2|emission-176-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32235,y:32601,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:87958bebe30bbcc449806c57a6252147,ntxv:0,isnm:False|UVIN-4890-OUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32712,y:32760,varname:node_2393,prsc:2|A-6074-RGB,B-2053-RGB,C-2579-OUT,D-797-RGB,E-5101-RGB;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32086,y:32787,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Color,id:797,x:32447,y:32985,ptovrint:True,ptlb:Tex_color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.5894524,c3:0.3235294,c4:1;n:type:ShaderForge.SFN_TexCoord,id:7737,x:31839,y:32746,varname:node_7737,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:4890,x:32055,y:32601,varname:node_4890,prsc:2|A-8615-OUT,B-7737-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:9033,x:31626,y:32684,ptovrint:False,ptlb:niuqu_tex,ptin:_niuqu_tex,varname:_niuqu_tex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:dd332f7569301644b8a6eff38488ceaa,ntxv:0,isnm:False|UVIN-9838-UVOUT;n:type:ShaderForge.SFN_Multiply,id:8615,x:31839,y:32601,varname:node_8615,prsc:2|A-5798-OUT,B-9033-RGB;n:type:ShaderForge.SFN_ValueProperty,id:5798,x:31626,y:32601,ptovrint:False,ptlb:niuqu_v,ptin:_niuqu_v,varname:_niuqu_v,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.3;n:type:ShaderForge.SFN_Panner,id:9838,x:31438,y:32684,varname:node_9838,prsc:2,spu:1,spv:1|UVIN-6878-UVOUT,DIST-5823-OUT;n:type:ShaderForge.SFN_TexCoord,id:6878,x:31252,y:32684,varname:node_6878,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:5823,x:31252,y:32843,varname:node_5823,prsc:2|A-5616-T,B-8706-XYZ;n:type:ShaderForge.SFN_Time,id:5616,x:31026,y:32825,varname:node_5616,prsc:2;n:type:ShaderForge.SFN_Vector4Property,id:8706,x:31026,y:32988,ptovrint:False,ptlb:qiuqu_tex_UV,ptin:_qiuqu_tex_UV,varname:_qiuqu_tex_UV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2,v2:0.2,v3:0,v4:0;n:type:ShaderForge.SFN_ValueProperty,id:1894,x:32671,y:32671,ptovrint:False,ptlb:liangdu,ptin:_liangdu,varname:_liangdu,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:176,x:32883,y:32712,varname:node_176,prsc:2|A-1894-OUT,B-2393-OUT;n:type:ShaderForge.SFN_Multiply,id:2579,x:32447,y:32825,varname:node_2579,prsc:2|A-6074-A,B-2053-A;n:type:ShaderForge.SFN_Tex2d,id:5101,x:32447,y:33166,ptovrint:False,ptlb:node_5101,ptin:_node_5101,varname:node_5101,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:fa7902415b3ebfa438f7dae97b0de193,ntxv:0,isnm:False;proporder:6074-797-9033-5798-8706-1894-5101;pass:END;sub:END;*/

Shader "Shader Forge/add_normal" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Tex_color", Color) = (1,0.5894524,0.3235294,1)
        _niuqu_tex ("niuqu_tex", 2D) = "white" {}
        _niuqu_v ("niuqu_v", Float ) = 0.3
        _qiuqu_tex_UV ("qiuqu_tex_UV", Vector) = (0.2,0.2,0,0)
        _liangdu ("liangdu", Float ) = 1
        _node_5101 ("node_5101", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform sampler2D _niuqu_tex; uniform float4 _niuqu_tex_ST;
            uniform float _niuqu_v;
            uniform float4 _qiuqu_tex_UV;
            uniform float _liangdu;
            uniform sampler2D _node_5101; uniform float4 _node_5101_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_5616 = _Time + _TimeEditor;
                float2 node_9838 = (i.uv0+(node_5616.g*_qiuqu_tex_UV.rgb)*float2(1,1));
                float4 _niuqu_tex_var = tex2D(_niuqu_tex,TRANSFORM_TEX(node_9838, _niuqu_tex));
                float3 node_4890 = ((_niuqu_v*_niuqu_tex_var.rgb)+float3(i.uv0,0.0));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_4890, _MainTex));
                float4 _node_5101_var = tex2D(_node_5101,TRANSFORM_TEX(i.uv0, _node_5101));
                float3 emissive = (_liangdu*(_MainTex_var.rgb*i.vertexColor.rgb*(_MainTex_var.a*i.vertexColor.a)*_TintColor.rgb*_node_5101_var.rgb));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
