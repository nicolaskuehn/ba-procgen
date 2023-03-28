Shader "Custom/WaterShader"
{
    Properties
    {
        _WaterColorsAlbedoTex ("Water Color Gradient", 2D) = "blue" {}
        _OceanFlipbookNormalTex ("Ocean Normal Map (Flipbook)", 2D) = "bump" {}

        _WaterDensity ("Water Density", Range(0,10)) = 5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        // Disables writing to the depth buffer for this SubShader
        //ZWrite Off -> need depth buffer later for depth fading! (so delete this later)
        // Enable regular alpha blending
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_OceanFlipbookNormalTex : TEXCOORD0;
            float3 worldPos;
            float4 screenPos;
            float eyeDepth;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _ColorSurface;
        fixed4 _ColorSeabed;
        float _WaterDensity;

        sampler2D _WaterColorsAlbedoTex;
        sampler2D _OceanFlipbookNormalTex;

        sampler2D _CameraDepthTexture;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // TODO: RENAME CAMEL CASE TO UNDERSCORES!!! (shader code convention)

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
 
            // Calculate ocean depth
            float2 screenSpaceUV = IN.screenPos.xy / IN.screenPos.w;

            float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenSpaceUV);
            sceneDepth = Linear01Depth(sceneDepth)  // get linear depth
                    * _ProjectionParams.z;          // camera's far plane in world space
            
            // if (sceneDepth > _ProjectionParams.z - 1.0) discard;
                    
            float3 cameraToWaterSurface = IN.worldPos - _WorldSpaceCameraPos;

            float oceanDepth = sceneDepth - length(cameraToWaterSurface);

            // Albedo
            // Mix colors based on the ocean depth
            float sample_point_x = clamp(oceanDepth / _WaterDensity, 0.0, 1.0);   // normalize ocean depth with water density

            o.Albedo = tex2D(_WaterColorsAlbedoTex, float2(sample_point_x, 0)).rgb;
            o.Alpha = 0.9;

            // Test NormalMap
            o.Normal = UnpackNormal(tex2D(_OceanFlipbookNormalTex, IN.uv_OceanFlipbookNormalTex));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
