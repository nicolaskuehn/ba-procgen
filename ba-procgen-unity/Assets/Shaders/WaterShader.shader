Shader "Custom/WaterShader"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _WaterColorsAlbedoTex ("Water Color Gradient", 2D) = "blue" {}
        _OceanFlipbookNormalTex ("Ocean Normal Map (Flipbook)", 2D) = "bump" {} // TODO: CHECK COMPRESSION IN UNITY EDITOR
        _OceanFlipbookLength1D ("Ocean Flipbook Frame Count 1D", Integer) = 8
        _OceanFlipbookFramerate ("Framerate of the Ocean Flipbook Animation", Integer) = 10
        _OceanTiling ("Tiling of the ocean textures", Range(0,20)) = 10
        _WaveScale ("Scales the height of the waves", Range(0,2)) = 1

        _WaterDensity ("Water Density", Range(0,10)) = 5.0
        _WaterFogDensity ("Water Fog Density", Range(0,10)) = 1.0

        _FoamNoiseTex ("Foam Noise Texture", 2D) = "white" {}
        _FoamAlpha ("Foam Alpha", Range(0,1.0)) = 0.5
        _FoamOffset ("Foam Offset", Range(0,1.0)) = 0.05
        _FoamFrequency ("Foam Frequency", Range(0,100.0)) = 75.0
        _FoamSpread ("Foam Spread", Range(0,0.5)) = 0.2
        _FoamFade ("Foam Fade", Range(0.0,0.1)) = 0.06
        _FoamWaveSpeed ("Foam Wave Speed", Range(0.0,2.0)) = 1.2
        _FoamWaveFrequency ("Foam Wave Frequency", Range(0.0,200.0)) = 100.0
        _FoamWaveStrength ("Foam Wave Strength", Range(0.0,0.5)) = 0.03
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        // Disables writing to the depth buffer for this SubShader
        //ZWrite Off -> need depth buffer later for depth fading! (so delete this later)
        // Enable regular alpha blending
        Blend SrcAlpha OneMinusSrcAlpha

        GrabPass { "_OceanGround" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha // alpha:fade fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #define TAU 6.28318530718

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_OceanFlipbookNormalTex : TEXCOORD0;
            float2 uv_FoamNoiseTex;
            float3 worldPos;
            float4 screenPos;
            float eyeDepth;
        };

        half _Glossiness;
        half _Metallic;
        
        float _WaterDensity;
        float _WaterFogDensity;
        float _WaveScale;
        float _OceanTiling;

        float _DepthTreshold;
        
        sampler2D _FoamNoiseTex;
        float _FoamAlpha;
        float _FoamOffset;
        float _FoamFrequency;
        float _FoamSpread;
        float _FoamFade;
        float _FoamWaveSpeed;
        float _FoamWaveFrequency;
        float _FoamWaveStrength;

        sampler2D _WaterColorsAlbedoTex;
        sampler2D _OceanFlipbookNormalTex;
        int _OceanFlipbookLength1D;
        int _OceanFlipbookFramerate;

        // TODO: REMOVE (DEBUG)
        sampler2D _TestFlipbookTex;

        sampler2D _OceanGround;
        sampler2D _CameraDepthTexture;



        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        // Helper functions
        float map (float a_min, float a_max, float b_min, float b_max, float t) {
            return min(max((t - a_min) * (b_max - b_min) / (a_max - a_min) + b_min, b_min), b_max);
        } 

        float2 calculateFlipbookUV(int frame, float2 uv) {
            int row = frame / _OceanFlipbookLength1D;
            int col = frame - row * _OceanFlipbookLength1D;

            float tileUVSize1D = 1.0 / _OceanFlipbookLength1D;

            float2 currentFrameMin = float2(tileUVSize1D * col, 1 - tileUVSize1D * (row + 1));
            float2 currentFrameMax = float2(tileUVSize1D * (col + 1), 1 - tileUVSize1D * row);
            
            return lerp(currentFrameMin, currentFrameMax, uv);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // TODO: RENAME CAMEL CASE TO UNDERSCORES!!! (shader code convention)
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
 
            // Calculate ocean depth
            float2 screenSpaceUV = IN.screenPos.xy / IN.screenPos.w;
            
            float groundDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenSpaceUV);
            
            groundDepth = Linear01Depth(groundDepth)  // get linear depth
                         * _ProjectionParams.z;       // camera's far plane in world space
            
            float waterSurfaceDepth = IN.screenPos.w;

            float oceanDepth = groundDepth - waterSurfaceDepth;


            // ... Albedo ... //
            // Mix colors based on the ocean depth
            float transmission = exp(-_WaterDensity * oceanDepth);
            
            float sample_point_x = clamp(1.0 - transmission, 0.0, 1.0);   // Sample gradient texture based on transmission
            float3 waterColor = tex2D(_WaterColorsAlbedoTex, float2(sample_point_x, 0)).rgb;
            
            float3 oceanGroundColor = tex2D(_OceanGround, screenSpaceUV).rgb;

            float fogFactor = exp2(-_WaterFogDensity * oceanDepth);

            float3 oceanColor = lerp(waterColor, oceanGroundColor, fogFactor); // TODO: dont show oceanGround completely! (see edges)

            // Foam
            //float foamMask = smoothstep(_FoamTreshold + _FoamFade, _FoamTreshold, oceanDepth);
            
            float foamNoise = tex2D(_FoamNoiseTex, IN.uv_FoamNoiseTex * _FoamFrequency).r * 2.0 - 1.0; // [-1.0, 1.0]
            float foamWavePhaseShift = (tex2D(_FoamNoiseTex, IN.uv_FoamNoiseTex * 2.0).r - 0.5) * 2.0 * TAU;
            float foamSDF = -oceanDepth + _FoamOffset + _FoamSpread * foamNoise + sin(oceanDepth * _FoamWaveFrequency - _Time.y * _FoamWaveSpeed - foamWavePhaseShift) * _FoamWaveStrength;
            float3 foamColor = float3(1.0, 1.0, 1.0);
            
            float3 foamMask = smoothstep(-_FoamFade, _FoamFade, foamSDF);

            // Final albedo color
            o.Albedo = lerp(oceanColor, foamColor, foamMask);

            // ... Alpha ... //
            float oceanAlpha = map(0.0, 1.0, 0.75, 1.0, 1.0 - transmission);
            
            o.Alpha = lerp(oceanAlpha, _FoamAlpha, foamMask);


            // ... Emission ... //
            o.Emission = 0.3 * foamMask;


            // ... Normal ... //
            float frame = _Time.y * _OceanFlipbookFramerate;
            
            int frameBefore = floor(frame);
            int frameAfter = frameBefore + 1;

            float2 tiledUV = frac(IN.uv_OceanFlipbookNormalTex * _OceanTiling);

            float2 frameBeforeUV = calculateFlipbookUV(frameBefore, tiledUV);
            float2 frameAfterUV = calculateFlipbookUV(frameAfter, tiledUV);

            float3 normalBefore = UnpackNormal(tex2D(_OceanFlipbookNormalTex, frameBeforeUV));
            float3 normalAfter = UnpackNormal(tex2D(_OceanFlipbookNormalTex, frameAfterUV));

            float3 normal = lerp(normalBefore, normalAfter, frac(frame)); 
            normal.xy *= _WaveScale * saturate(oceanDepth * 3.0 - 0.3);
            
            o.Normal = normalize(normal);
        }
        ENDCG
    }
    //FallBack "Diffuse"
}
