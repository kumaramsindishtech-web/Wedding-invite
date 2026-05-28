Shader "SindishTech/WaterTankFill"
{
    Properties
    {
        // Tank base material (metallic look in editor)
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.7, 0.7, 0.75, 1)
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
        
        // Mode control
        _SimulationMode ("Simulation Mode", Range(0, 1)) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        LOD 300
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
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
                float _SimulationMode;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                half4 baseMapColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Calculate water surface Y position
                float waterWorldY = _TankBottomY + (_WaterLevel * _TankHeight);
                
                // Check if below water level
                bool isUnderwater = input.positionWS.y <= waterWorldY && _WaterLevel > 0.001;
                
                half3 finalColor;
                float finalSmoothness;
                float finalMetallic;
                
                // Mode check: 0 = metallic, 1 = water simulation
                if (_SimulationMode < 0.5 || !isUnderwater)
                {
                    // METALLIC MODE - Tank material
                    finalColor = _BaseColor.rgb * baseMapColor.rgb;
                    finalMetallic = _Metallic;
                    finalSmoothness = _Smoothness;
                }
                else
                {
                    // WATER MODE
                    float depthFromSurface = waterWorldY - input.positionWS.y;
                    float normalizedDepth = saturate(depthFromSurface / _TankHeight);
                    
                    // Surface distance for effects
                    float surfaceDistance = abs(input.positionWS.y - waterWorldY);
                    float surfaceFactor = 1.0 - saturate(surfaceDistance / 0.1);
                    
                    // Water color with depth
                    half3 waterColor = lerp(_WaterColor.rgb, _DeepWaterColor.rgb, normalizedDepth);
                    waterColor = lerp(waterColor, _SurfaceColor.rgb, surfaceFactor * 0.5);
                    
                    // Temperature effect
                    float tempFactor = saturate(_Temperature / 340.0);
                    waterColor = lerp(waterColor, _HeatColor.rgb, tempFactor * 0.4);
                    
                    // Fresnel
                    float3 viewDir = normalize(input.viewDirWS);
                    float3 normalWS = normalize(input.normalWS);
                    float fresnel = pow(1.0 - saturate(dot(viewDir, normalWS)), 3.0);
                    waterColor += fresnel * 0.15;
                    
                    // Surface ripples
                    if (surfaceFactor > 0.1)
                    {
                        float ripple = sin(input.positionWS.x * 8 + _Time.y) * 
                                       cos(input.positionWS.z * 6 + _Time.y * 0.8) * 0.02;
                        waterColor += ripple * surfaceFactor;
                    }
                    
                    finalColor = waterColor;
                    finalMetallic = 0.0;
                    finalSmoothness = 0.95;
                }
                
                // Basic lighting
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 diffuse = finalColor * mainLight.color * NdotL;
                float3 ambient = finalColor * 0.2;
                
                // Specular
                float3 viewDir = normalize(input.viewDirWS);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float spec = pow(saturate(dot(normalWS, halfDir)), finalSmoothness * 128);
                float3 specular = mainLight.color * spec * 0.5;
                
                half3 color = ambient + diffuse + specular;
                
                return half4(color, 1.0);
            }
            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            float3 _LightDirection;
            
            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }
            
            half4 ShadowFrag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
