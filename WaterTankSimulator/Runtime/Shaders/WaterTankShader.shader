Shader "SindishTech/WaterTankRealistic"
{
    Properties
    {
        // Water appearance
        _WaterColor ("Water Color", Color) = (0.1, 0.4, 0.7, 0.85)
        _DeepWaterColor ("Deep Water Color", Color) = (0.02, 0.15, 0.4, 0.95)
        _FoamColor ("Foam/Surface Color", Color) = (0.6, 0.8, 0.9, 0.9)
        
        // Water level
        _WaterLevel ("Water Level (0-1)", Range(0, 1)) = 0.5
        _TankHeight ("Tank Height", Float) = 2.0
        _TankBottomY ("Tank Bottom Y Position", Float) = -1.0
        
        // Surface properties
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 4.0
        _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.1
        
        // Wave animation
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 5.0
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.3
        
        // Temperature visual
        _Temperature ("Temperature (0-340)", Range(0, 340)) = 25
        _HeatDistortion ("Heat Distortion", Range(0, 0.1)) = 0.02
        _HeatColor ("Heat Tint Color", Color) = (1, 0.3, 0.1, 1)
        
        // Caustics & depth
        _CausticsScale ("Caustics Scale", Range(0.1, 10)) = 2.0
        _CausticsSpeed ("Caustics Speed", Range(0, 3)) = 0.5
        _CausticsIntensity ("Caustics Intensity", Range(0, 2)) = 0.5
        _DepthFade ("Depth Fade Distance", Range(0.1, 10)) = 2.0
        
        // Normal map for surface detail
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _NormalTiling ("Normal Tiling", Range(0.1, 10)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True"
        }
        
        LOD 300
        
        // Render back faces first for proper transparency
        Pass
        {
            Name "BackFace"
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            
            float4 _WaterColor;
            float4 _DeepWaterColor;
            float _WaterLevel;
            float _TankHeight;
            float _TankBottomY;
            float _Smoothness;
            float _FresnelPower;
            float _Temperature;
            float4 _HeatColor;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Clip above water level
                float waterWorldY = _TankBottomY + (_WaterLevel * _TankHeight);
                clip(waterWorldY - i.worldPos.y);
                
                // Depth-based color
                float depth = (waterWorldY - i.worldPos.y) / _TankHeight;
                float4 color = lerp(_WaterColor, _DeepWaterColor, saturate(depth * 2));
                
                // Temperature tint
                float tempFactor = saturate(_Temperature / 340.0);
                color = lerp(color, _HeatColor, tempFactor * 0.3);
                
                // Simple fresnel for back faces
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, -i.worldNormal)), _FresnelPower);
                color.rgb += fresnel * 0.1;
                
                color.a *= 0.6;
                
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;
            }
            ENDCG
        }
        
        // Front face pass
        Pass
        {
            Name "FrontFace"
            Cull Back
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float3 worldTangent : TEXCOORD4;
                float3 worldBitangent : TEXCOORD5;
                UNITY_FOG_COORDS(6)
            };
            
            float4 _WaterColor;
            float4 _DeepWaterColor;
            float4 _FoamColor;
            float _WaterLevel;
            float _TankHeight;
            float _TankBottomY;
            float _Smoothness;
            float _Metallic;
            float _FresnelPower;
            float _RefractionStrength;
            float _WaveSpeed;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _RippleStrength;
            float _Temperature;
            float _HeatDistortion;
            float4 _HeatColor;
            float _CausticsScale;
            float _CausticsSpeed;
            float _CausticsIntensity;
            float _DepthFade;
            sampler2D _NormalMap;
            float _NormalStrength;
            float _NormalTiling;
            
            // Simple noise function
            float hash(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Caustics pattern
            float caustics(float2 uv, float time)
            {
                float2 p = uv * _CausticsScale;
                float c = 0;
                c += noise(p + time * 0.3) * 0.5;
                c += noise(p * 2.0 - time * 0.2) * 0.25;
                c += noise(p * 4.0 + time * 0.1) * 0.125;
                return c;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float waterWorldY = _TankBottomY + (_WaterLevel * _TankHeight);
                
                // Add wave displacement at water surface
                float surfaceDist = abs(worldPos.y - waterWorldY);
                if (surfaceDist < 0.1 && worldPos.y <= waterWorldY + 0.05)
                {
                    float wave = sin(worldPos.x * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveAmplitude;
                    wave += cos(worldPos.z * _WaveFrequency * 0.7 + _Time.y * _WaveSpeed * 0.8) * _WaveAmplitude * 0.5;
                    v.vertex.y += wave;
                }
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBitangent = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                o.uv = v.uv;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Clip above water level
                float waterWorldY = _TankBottomY + (_WaterLevel * _TankHeight);
                clip(waterWorldY - i.worldPos.y);
                
                float time = _Time.y;
                
                // UV animation for normal map
                float2 animUV1 = i.uv * _NormalTiling + float2(time * 0.02, time * 0.01);
                float2 animUV2 = i.uv * _NormalTiling * 1.5 - float2(time * 0.015, time * 0.02);
                
                // Sample and blend normal maps
                float3 normal1 = UnpackNormal(tex2D(_NormalMap, animUV1));
                float3 normal2 = UnpackNormal(tex2D(_NormalMap, animUV2));
                float3 blendedNormal = normalize(float3(
                    (normal1.xy + normal2.xy) * _NormalStrength,
                    1.0
                ));
                
                // Transform normal to world space
                float3x3 TBN = float3x3(i.worldTangent, i.worldBitangent, i.worldNormal);
                float3 worldNormal = normalize(mul(blendedNormal, TBN));
                
                // Depth calculation
                float depth = (waterWorldY - i.worldPos.y) / _TankHeight;
                float depthFactor = saturate(depth / (_DepthFade / _TankHeight));
                
                // Base water color with depth
                float4 color = lerp(_WaterColor, _DeepWaterColor, depthFactor);
                
                // Surface foam near water level
                float surfaceDist = abs(i.worldPos.y - waterWorldY) / _TankHeight;
                float foamFactor = 1.0 - saturate(surfaceDist * 20.0);
                color = lerp(color, _FoamColor, foamFactor * 0.3);
                
                // Temperature effect
                float tempFactor = saturate(_Temperature / 340.0);
                float4 tempColor = lerp(color, _HeatColor, tempFactor * 0.4);
                color = lerp(color, tempColor, tempFactor);
                
                // Heat distortion (shimmer effect at high temps)
                if (_Temperature > 100)
                {
                    float heatShimmer = sin(i.worldPos.y * 50 + time * 3) * _HeatDistortion * tempFactor;
                    color.rgb += heatShimmer;
                }
                
                // Caustics
                float causticsValue = caustics(i.uv, time * _CausticsSpeed);
                color.rgb += causticsValue * _CausticsIntensity * (1.0 - depthFactor) * 0.5;
                
                // Fresnel effect
                float NdotV = saturate(dot(worldNormal, i.viewDir));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);
                color.rgb += fresnel * 0.2;
                
                // Specular highlight (simple Blinn-Phong)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + i.viewDir);
                float spec = pow(saturate(dot(worldNormal, halfDir)), _Smoothness * 256);
                color.rgb += spec * 0.5;
                
                // Ripple effect when water is flowing (simulated via time)
                float ripple = sin(length(i.worldPos.xz) * 10 - time * 2) * _RippleStrength * 0.05;
                color.rgb += ripple;
                
                // Alpha based on depth and fresnel
                color.a = lerp(_WaterColor.a, _DeepWaterColor.a, depthFactor);
                color.a = lerp(color.a, 1.0, fresnel * 0.3);
                
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
