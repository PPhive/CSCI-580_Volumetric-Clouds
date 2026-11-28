Shader "Custom/RaymarchCloud"{
    Properties{
        _DensityTexture ("Density Texture", 3D) = "" {}
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 1)

        [Header(Noise)]
        _ShapeScale ("Shape Scale (tiles per world unit)", Float) = 0.02
        _DetailScale ("Detail Scale (tiles per world unit)", Float) = 0.1
        _DetailWeight ("Detail Weight", Range(0, 1)) = 0.2
        _DetailFadeDistance ("Detail Fade Distance (world units)", Range(10, 1000)) = 200
        _ShapeRoughness ("Shape Roughness (extra octaves)", Range(0, 1)) = 0.5

        [Header(Shape)]
        _BottomFade ("Bottom Fade (fraction of box height)", Range(0.01, 0.5)) = 0.1
        _TopFadeStart ("Top Fade Start (fraction of box height)", Range(0, 0.99)) = 0.4

        [Header(Density)]
        _DensityThreshold ("Density Threshold", Range(0, 1)) = 0.35
        _DensityMultiplier ("Density Multiplier", Range(0, 10)) = 1
        _EdgeFade ("Edge Fade Distance (world units)", Range(0, 50)) = 10

        [Header(Lighting)]
        _LightAbsorption ("Light Absorption (toward sun)", Range(0, 4)) = 1
        _DarknessThreshold ("Darkness Threshold (min light)", Range(0, 1)) = 0.3
        _ForwardScattering ("Forward Scattering (g)", Range(0, 0.95)) = 0.6
        _PhaseStrength ("Phase Strength", Range(0, 1)) = 0.5

        [Header(Quality)]
        _StepSize ("Step Size (world units)", Range(0.1, 10)) = 1.5
        _MaxSteps ("Max March Steps", Range(8, 256)) = 128
        _LightSteps ("Light March Steps", Range(1, 16)) = 6
        _LightMaxDistance ("Light March Max Distance (world units)", Range(1, 200)) = 30
        _JitterStrength ("Jitter (hides banding)", Range(0, 1)) = 1
    }

    SubShader{
        Tags{
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass{
            Name "RaymarchCloud"

            Cull Front
            ZWrite Off
            ZTest Always
            Blend One SrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #define BOUNDS_MIN float3(-0.5, -0.5, -0.5)
            #define BOUNDS_MAX float3( 0.5,  0.5,  0.5)

            TEXTURE3D(_DensityTexture);
            SAMPLER(sampler_DensityTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _CloudColor;
                float _ShapeScale;
                float _DetailScale;
                float _DetailWeight;
                float _DetailFadeDistance;
                float _BottomFade;
                float _TopFadeStart;
                float _DensityThreshold;
                float _DensityMultiplier;
                float _EdgeFade;
                float _LightAbsorption;
                float _DarknessThreshold;
                float _ForwardScattering;
                float _PhaseStrength;
                float _StepSize;
                float _MaxSteps;
                float _LightSteps;
                float _LightMaxDistance;
                float _JitterStrength;
                float _ShapeRoughness;
            CBUFFER_END

            struct Attributes{
                float4 positionOS : POSITION;
            };

            struct Varyings{
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            Varyings vert(Attributes IN){
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            float2 RayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRayDir){
                float3 t0 = (boundsMin - rayOrigin) * invRayDir;
                float3 t1 = (boundsMax - rayOrigin) * invRayDir;
                float3 tMin = min(t0, t1);
                float3 tMax = max(t0, t1);

                float dstA = max(max(tMin.x, tMin.y), tMin.z);
                float dstB = min(min(tMax.x, tMax.y), tMax.z);

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            float3 GetObjectScale(){
                return float3(
                    length(UNITY_MATRIX_M._m00_m10_m20),
                    length(UNITY_MATRIX_M._m01_m11_m21),
                    length(UNITY_MATRIX_M._m02_m12_m22));
            }

            // 0 at the box floor -> 1 a short way up -> 1 until _TopFadeStart -> 0 at the ceiling
            float HeightGradient(float heightFrac){
                float bottom = smoothstep(0.0, _BottomFade, heightFrac);
                float top = 1.0 - smoothstep(_TopFadeStart, 1.0, heightFrac);
                return bottom * top;
            }

            // Inverted Worley: 1 at feature points (blob centers) and 0 if far from them
            float WorleyInv(float3 p){
                return 1.0 - SAMPLE_TEXTURE3D_LOD(_DensityTexture, sampler_DensityTexture, p, 0).r;
            }

            float SampleDensity(float3 posOS, float3 objScale, float detailWeight){
                float3 posWS = TransformObjectToWorld(posOS);

                // We have 3 octaves of inverted Worley (big + medium + small), weights 1, r, r^2
                // Odd frequency ratios and offsets so the octaves don't line up with each other
                float3 p = posWS * _ShapeScale;
                float r = _ShapeRoughness;
                float shape = WorleyInv(p)
                            + r     * WorleyInv(p * 2.73 + float3(0.37, 0.11, 0.59))
                            + r * r * WorleyInv(p * 6.11 + float3(0.71, 0.29, 0.13));
                shape /= (1.0 + r + r * r);

                shape *= HeightGradient(posOS.y + 0.5);

                float density = shape - _DensityThreshold;

                if (detailWeight > 0){
                    float detail = SAMPLE_TEXTURE3D_LOD(_DensityTexture, sampler_DensityTexture, posWS * _DetailScale, 0).r;
                    density -= detail * detailWeight;
                }

                float3 dstToWallWS = (0.5 - abs(posOS)) * objScale;
                float dstToNearestWall = min(dstToWallWS.x, min(dstToWallWS.y, dstToWallWS.z));
                float edgeFade = saturate(dstToNearestWall / max(_EdgeFade, 1e-4));

                return max(0, density) * _DensityMultiplier * edgeFade;
            }

            float HenyeyGreenstein4Pi(float cosTheta, float g){
                float g2 = g * g;
                return (1.0 - g2) / pow(max(1e-4, 1.0 + g2 - 2.0 * g * cosTheta), 1.5);
            }

            float ScreenJitter(float2 pixelPos){
                return frac(52.9829189 * frac(dot(pixelPos, float2(0.06711056, 0.00583715))));
            }

            float LightMarch(float3 posOS, float3 lightDirOS, float worldPerObjectLight, float3 objScale, float detailWeight){
                float dstToExitOS = RayBoxDst(BOUNDS_MIN, BOUNDS_MAX, posOS, 1.0 / lightDirOS).y;
                float maxDstOS = _LightMaxDistance / worldPerObjectLight;
                float dstOS = min(dstToExitOS, maxDstOS);

                int lightSteps = (int)_LightSteps;
                float stepSizeOS = dstOS / lightSteps;
                float stepSizeWS = stepSizeOS * worldPerObjectLight;

                float totalDensity = 0;

                [loop]
                for (int j = 0; j < lightSteps; j++){
                    float3 p = posOS + lightDirOS * (stepSizeOS * (j + 0.5));
                    totalDensity += SampleDensity(p, objScale, detailWeight) * stepSizeWS;
                }

                float lightTransmittance = exp(-totalDensity * _LightAbsorption);
                return _DarknessThreshold + lightTransmittance * (1.0 - _DarknessThreshold);
            }

            half4 frag(Varyings IN) : SV_Target{
                //View ray
                float3 camPosOS = TransformWorldToObject(_WorldSpaceCameraPos);
                float3 rayDirOS = normalize(IN.positionOS - camPosOS);
                float3 rayDirWS = normalize(TransformObjectToWorldDir(rayDirOS, false));
                float2 hit = RayBoxDst(BOUNDS_MIN, BOUNDS_MAX, camPosOS, 1.0 / rayDirOS);
                float dstToBox = hit.x;
                float dstInsideBox = hit.y;

                float worldPerObject = length(TransformObjectToWorldDir(rayDirOS, false));
                float3 objScale = GetObjectScale();

                // Solid obj
                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                float rawDepth = SampleSceneDepth(screenUV);
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float sceneDstWS = eyeDepth / dot(rayDirWS, GetViewForwardDir());
                float sceneDstOS = sceneDstWS / worldPerObject;
                float dstLimit = min(dstInsideBox, sceneDstOS - dstToBox);

                if (dstLimit <= 0)
                    return half4(0, 0, 0, 1);

                // Sun
                Light mainLight = GetMainLight();
                float3 lightDirWS = mainLight.direction;
                float3 lightDirOS = normalize(TransformWorldToObjectDir(lightDirWS, false));
                float worldPerObjectLight = length(TransformObjectToWorldDir(lightDirOS, false));

                float cosTheta = dot(rayDirWS, lightDirWS);
                float phase = lerp(1.0, HenyeyGreenstein4Pi(cosTheta, _ForwardScattering), _PhaseStrength);

                // March, fixed world-space step length, capped at _MaxSteps
                float minStepOS = _StepSize / worldPerObject;
                float stepSizeOS = max(minStepOS, dstLimit / _MaxSteps);
                int numSteps = (int)ceil(dstLimit / stepSizeOS);
                stepSizeOS = dstLimit / numSteps;
                float stepSizeWS = stepSizeOS * worldPerObject;

                float jitter = lerp(0.5, ScreenJitter(IN.positionCS.xy), _JitterStrength);
                float transmittance = 1.0;
                float lightEnergy = 0.0;

                [loop]
                for (int i = 0; i < numSteps; i++){
                    float dstAlongRayOS = dstToBox + stepSizeOS * (i + jitter);
                    float3 samplePosOS = camPosOS + rayDirOS * dstAlongRayOS;

                    // Fade detail out linearly with distance from the camera
                    float dstFromCamWS = dstAlongRayOS * worldPerObject;
                    float detailWeight = _DetailWeight * saturate(1.0 - dstFromCamWS / _DetailFadeDistance);

                    float density = SampleDensity(samplePosOS, objScale, detailWeight);

                    if (density > 0){
                        float lightTransmittance = LightMarch(samplePosOS, lightDirOS, worldPerObjectLight, objScale, detailWeight);
                        lightEnergy += density * stepSizeWS * transmittance * lightTransmittance * phase;
                        transmittance *= exp(-density * stepSizeWS);

                        if (transmittance < 0.01)
                            break;
                    }
                }

                float3 color = lightEnergy * mainLight.color * _CloudColor.rgb;
                return half4(color, transmittance);
            }
            ENDHLSL
        }
    }
}