Shader "Custom/FireShader" {
	Properties {
//        _MainTexture("Main Color (RGB)", 2D) = "white" {}
//        _Color("Color", Color) = (1,1,1,1)

        _MainTex ("Main texture", 2D) = "white" {}
        _MainTint ("Color", Color) = (1,1,1,1)
        
        _DissolveTexture("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 1
        
        _ScrollXSpeed("X Scrool Speed", Range(0, 50)) = 2
        _ScrollYSpeed("Y Scrool Speed", Range(0, 50)) = 2
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0

	}
	SubShader {        
        //Tags { "RenderType"="Opaque" }
        Tags { 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"="Transparent"
        }
        LOD 200
        Cull Off
        
        CGPROGRAM
        
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0
        
        struct Input {
            float2 uv_MainTex;
            float2 uv_DissolveTexture;
            float2 uv_BumpMap;
            float3 viewDir;
        };
        
        fixed4 _MainTint;
        sampler2D _MainTex;
        sampler2D _DissolveTexture;
        sampler2D _BumpMap;
        float _DissolveAmount;
        fixed _ScrollXSpeed;
        fixed _ScrollYSpeed;
        float4 _RimColor;
        float _RimPower;
        
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o) {            
            fixed2 scrolledUV = IN.uv_MainTex;

            fixed xScrollValue = _ScrollXSpeed * _Time;             
            fixed yScrollValue = _ScrollYSpeed * _Time;         

            scrolledUV += fixed2(xScrollValue, yScrollValue);
            float4 dissolveColor = tex2D(_DissolveTexture, scrolledUV);
            clip(dissolveColor.rgb - _DissolveAmount);

            half4 c = tex2D (_MainTex, scrolledUV);
            o.Albedo = c.rgb * _MainTint;
            o.Normal = UnpackNormal (tex2D (_BumpMap, scrolledUV));
            
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow (rim, _RimPower);
            
            o.Alpha = _MainTint.a;

        }
        ENDCG
                   
	}
	FallBack "Diffuse"
}
