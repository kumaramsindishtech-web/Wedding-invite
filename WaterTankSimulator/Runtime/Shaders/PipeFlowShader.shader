Shader "SindishTech/PipeFlowRealistic"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.6, 0.6, 0.65, 1)
        _FlowColor ("Flow Color", Color) = (0.2, 0.5, 0.9, 0.8)
        _HotColor ("Hot Flow Color", Color) = (0.9, 0.3, 0.1, 0.8)
        
        // Metal pipe properties
        _Metallic ("Metallic", Range(0, 1)) = 0.8
        _Smoothness ("Smoothness", Range(0, 1)) = 0.7
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        
        // Flow visualization
        _FlowActive ("Flow Active", Range(0, 1)) = 0
        _FlowSpeed ("Flow Speed", Range(0, 10)) = 2.0
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        _Temperature ("Temperature (0-340)", Range(0, 340)) = 25
        
        // Interior water visible through gaps
        _WaterVisibility ("Water Visibility", Range(0, 1)) = 0.3
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float3 viewDir;
            float3 worldPos;
        };
        
        float4 _Color;
        float4 _FlowColor;
        float4 _HotColor;
        float _Metallic;
        float _Smoothness;
        float _NormalStrength;
        float _FlowActive;
        float _FlowSpeed;
        float4 _FlowDirection;
        float _Temperature;
        float _WaterVisibility;
        float _FresnelPower;
        
        // Simple noise for flow effect
        float hash2D(float2 p)
        {
            return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
        }
        
        float noise2D(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            
            float a = hash2D(i);
            float b = hash2D(i + float2(1, 0));
            float c = hash2D(i + float2(0, 1));
            float d = hash2D(i + float2(1, 1));
            
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Base metal texture
            float4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            float4 baseColor = texColor * _Color;
            
            // Normal map
            float3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            normal.xy *= _NormalStrength;
            o.Normal = normalize(normal);
            
            // Flow effect (visible through pipe material)
            if (_FlowActive > 0.5)
            {
                // Animated flow pattern
                float time = _Time.y * _FlowSpeed;
                float2 flowUV = IN.uv_MainTex + _FlowDirection.xy * time;
                float flowPattern = noise2D(flowUV * 5.0) * 0.5 + 0.5;
                flowPattern += noise2D(flowUV * 10.0 - time * 0.5) * 0.25;
                
                // Temperature-based flow color
                float tempNormalized = saturate(_Temperature / 200.0);
                float4 currentFlowColor = lerp(_FlowColor, _HotColor, tempNormalized);
                
                // Fresnel for seeing "inside" the pipe
                float fresnel = pow(1.0 - saturate(dot(IN.viewDir, o.Normal)), _FresnelPower);
                float flowVisibility = _WaterVisibility * fresnel * _FlowActive;
                
                // Blend flow color with base
                baseColor.rgb = lerp(baseColor.rgb, currentFlowColor.rgb * flowPattern, flowVisibility);
                
                // Add subtle emission for hot flow
                if (_Temperature > 100)
                {
                    float emissionStrength = saturate((_Temperature - 100) / 240.0) * 0.2;
                    o.Emission = currentFlowColor.rgb * emissionStrength * flowVisibility;
                }
            }
            
            o.Albedo = baseColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    
    FallBack "Standard"
}
