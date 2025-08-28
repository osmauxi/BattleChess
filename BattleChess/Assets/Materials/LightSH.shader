Shader "Custom/FanLightMask" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0,180)) = 90
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _Angle;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                float2 center = float2(0.5, 0.5);
                float2 dir = i.uv - center;
                float angle = atan2(dir.x, dir.y) * (180 / 3.14159);
                angle = abs(angle);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a = (angle < _Angle/2) ? 1 : 0; // ÉÈÐÎÇøÓò±£Áô
                return col;
            }
            ENDCG
        }
    }
}