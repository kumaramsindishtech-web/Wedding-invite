Shader "SindishTech/WaterFill"
{
    Properties
    {
        [Header(Water Colors)]
        _WaterColor ("Water Color", Color) = (0.1, 0.5, 0.8, 1)
        _DeepWaterColor ("Deep Water Color", Color) = (0.05, 0.2, 0.5, 1)
        _SurfaceColor ("Surface Highlight", Color) = (0.3, 0.7, 1.0, 1)
        
        [Header(Water Level)]
        _FillAmount ("Fill Amount (0-1)", Range(0, 1)) = 0
        
        [Header(Temperature)]
        _Temperature ("Temperature (0-340)", Range(0, 340)) = 25
        _HotColor ("Hot Water Color", Color) = (1, 0.3, 0.1, 1)
        
        [Header(Surface Effects)]
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1
        _WaveStrength ("Wave Strength", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        Pass
        {
            Name "WaterFill"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
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
                float3 positionOS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float fogCoord : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _WaterColor;
                half4 _DeepWaterColor;
                half4 _SurfaceColor;
                float _FillAmount;
                float _Temperature;
                half4 _HotColor;
                float _FresnelPower;
                float _Smoothness;
                float _WaveSpeed;
                float _WaveStrength;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = posInputs.positionCS;
                output.positionOS = input.positionOS.xyz; // Keep object space position
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Use UV.y for fill level (UV typically goes 0-1 from bottom to top)
                // OR use object space Y normalized
                
                // Method: Use UV.y (most reliable for imported meshes)
                float verticalPos = input.uv.y;
                
                // Add small wave at surface
                float wave = sin(input.positionWS.x * 8 + _Time.y * _WaveSpeed) * 
                             cos(input.positionWS.z * 6 + _Time.y * _WaveSpeed * 0.7) * _WaveStrength;
                
                float fillLevel = _FillAmount + wave;
                
                // CLIP - Discard pixels ABOVE fill level
                // If UV.y > fillAmount, discard (clip negative values)
                clip(fillLevel - verticalPos);
                
                // Calculate depth for coloring (0 at bottom, 1 at fill level)
                float depth = verticalPos / max(_FillAmount, 0.001);
                
                // Base water color - deeper = darker
                half3 waterColor = lerp(_DeepWaterColor.rgb, _WaterColor.rgb, saturate(depth));
                
                // Surface highlight (near fill level)
                float surfaceProximity = 1.0 - saturate(abs(verticalPos - _FillAmount) / 0.05);
                waterColor = lerp(waterColor, _SurfaceColor.rgb, surfaceProximity * 0.5);
                
                // Temperature effect
                float tempFactor = saturate(_Temperature / 340.0);
                waterColor = lerp(waterColor, _HotColor.rgb, tempFactor * 0.5);
                
                // Fresnel effect
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                float3 normalWS = normalize(input.normalWS);
                float fresnel = pow(1.0 - saturate(dot(viewDir, normalWS)), _FresnelPower);
                waterColor += fresnel * 0.2;
                
                // Lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                // Diffuse
                half3 diffuse = waterColor * mainLight.color * (NdotL * 0.7 + 0.3);
                
                // Specular
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float spec = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 128);
                half3 specular = mainLight.color * spec * 0.4;
                
                // Ambient
                half3 ambient = waterColor * 0.15;
                
                half3 finalColor = ambient + diffuse + specular;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);
                
                return half4(finalColor, 1.0);
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
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float _FillAmount;
            float3 _LightDirection;
            
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return positionCS;
            }
            
            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                output.uv = input.uv;
                return output;
            }
            
            half4 ShadowFrag(Varyings input) : SV_Target
            {
                clip(_FillAmount - input.uv.y);
                return 0;
            }
            ENDHLSL
        }
        
        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float _FillAmount;
            
            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            half4 DepthFrag(Varyings input) : SV_Target
            {
                clip(_FillAmount - input.uv.y);
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
