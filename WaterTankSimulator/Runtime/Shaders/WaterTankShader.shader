Shader "SindishTech/WaterTankFill"
{
    Properties
    {
        // Tank base material (metallic look in editor)
        _MainTex ("Main Texture", 2D) = "white" {}
        _MetallicColor ("Metallic Color", Color) = (0.7, 0.7, 0.75, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.8
        _Smoothness ("Smoothness", Range(0, 1)) = 0.6
        
        // Water appearance
        _WaterColor ("Water Color", Color) = (0.1, 0.4, 0.7, 0.9)
        _DeepWaterColor ("Deep Water Color", Color) = (0.02, 0.15, 0.4, 0.95)
        _SurfaceColor ("Surface Color", Color) = (0.4, 0.7, 0.9, 0.95)
        
        // Water level control
        _WaterLevel ("Water Level (0-1)", Range(0, 1)) = 0.0
        _TankHeight ("Tank Height", Float) = 2.0
        _TankBottomY ("Tank Bottom Y Position", Float) = -1.0
        
        // Temperature visual
        _Temperature ("Temperature (0-340)", Range(0, 340)) = 25
        _HeatColor ("Heat Tint Color", Color) = (1, 0.3, 0.1, 1)
        
        // Water surface animation
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.05)) = 0.01
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 8.0
        
        // Fresnel for water
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3.0
        
        // Mode control (0 = show metallic, 1 = show water simulation)
        _SimulationMode ("Simulation Mode (0=Metallic, 1=Water)", Range(0, 1)) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
        }
        
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0
        
        sampler2D _MainTex;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
            float3 worldNormal;
        };
        
        // Properties
        float4 _MetallicColor;
        float _Metallic;
        float _Smoothness;
        
        float4 _WaterColor;
        float4 _DeepWaterColor;
        float4 _SurfaceColor;
        
        float _WaterLevel;
        float _TankHeight;
        float _TankBottomY;
        
        float _Temperature;
        float4 _HeatColor;
        
        float _WaveSpeed;
        float _WaveAmplitude;
        float _WaveFrequency;
        float _FresnelPower;
        
        float _SimulationMode;
        
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            
            // Calculate water surface Y position in world space
            float waterWorldY = _TankBottomY + (_WaterLevel * _TankHeight);
            
            // Check if this pixel is below water level
            bool isUnderwater = IN.worldPos.y <= waterWorldY && _WaterLevel > 0.001;
            
            // Simulation mode check (0 = metallic only, 1 = water simulation)
            if (_SimulationMode < 0.5 || !isUnderwater)
            {
                // METALLIC MODE - Show tank material
                o.Albedo = _MetallicColor.rgb * texColor.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Smoothness;
                o.Alpha = 1.0;
            }
            else
            {
                // WATER SIMULATION MODE
                
                // Calculate depth from surface
                float depthFromSurface = waterWorldY - IN.worldPos.y;
                float normalizedDepth = saturate(depthFromSurface / _TankHeight);
                
                // Distance from water surface (for surface effects)
                float surfaceDistance = abs(IN.worldPos.y - waterWorldY);
                float surfaceFactor = 1.0 - saturate(surfaceDistance / 0.1); // Within 0.1 units of surface
                
                // Base water color - deeper = darker
                float3 waterColor = lerp(_WaterColor.rgb, _DeepWaterColor.rgb, normalizedDepth);
                
                // Surface color blend (lighter at surface)
                waterColor = lerp(waterColor, _SurfaceColor.rgb, surfaceFactor * 0.5);
                
                // Temperature effect - warmer = more red/orange tint
                float tempFactor = saturate(_Temperature / 340.0);
                waterColor = lerp(waterColor, _HeatColor.rgb, tempFactor * 0.4);
                
                // Fresnel effect for realistic water look
                float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);
                waterColor += fresnel * 0.15;
                
                // Animated ripples at surface
                if (surfaceFactor > 0.1)
                {
                    float time = _Time.y * _WaveSpeed;
                    float ripple = sin(IN.worldPos.x * _WaveFrequency + time) * 
                                   cos(IN.worldPos.z * _WaveFrequency * 0.7 + time * 0.8);
                    ripple *= _WaveAmplitude * surfaceFactor;
                    waterColor += ripple;
                }
                
                // Heat shimmer for hot water
                if (_Temperature > 100)
                {
                    float shimmer = sin(IN.worldPos.y * 30 + _Time.y * 4) * 0.02 * tempFactor;
                    waterColor += shimmer;
                }
                
                o.Albedo = waterColor;
                o.Metallic = 0.0;
                o.Smoothness = 0.95; // Water is very smooth
                o.Alpha = 1.0;
            }
        }
        ENDCG
    }
    
    FallBack "Standard"
}
