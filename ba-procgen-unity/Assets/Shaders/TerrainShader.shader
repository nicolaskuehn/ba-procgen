Shader "Custom/TerrainShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _MinHeight ("Min Height", Range(-10.0, 10.0)) = 0.0
        _MaxHeight ("Max Height", Range(0.0, 20.0)) = 4.0
        _WaterLevel ("Water Level", float) = 0.0
        _HeightColorsAlbedoTex ("Height Color Gradient", 2D) = "white" {}
        _HeightSmoothnessTex ("Height Smoothness Gradient", 2D) = "white" {}
        _SandColor ("Sand Color", Color) = (1.0,1.0,0.5,1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Metallic;
        fixed4 _Color;

        float _MinHeight;
        float _MaxHeight;
        float _WaterLevel;
        sampler2D _HeightColorsAlbedoTex;
        sampler2D _HeightSmoothnessTex;
        fixed4 _SandColor;

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


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            // Set color and smoothness dependant on height (from 2d gradient textures)
            float current_height = IN.worldPos.y;
            uint above_water = step(_WaterLevel, current_height);
            
            float sample_point_x = map(_MinHeight, _MaxHeight, 0.0, 1.0, current_height);
            
            fixed3 terrain_color = tex2D(_HeightColorsAlbedoTex, float2(sample_point_x, 0)).rgb;
            fixed terrain_smoothness = tex2D(_HeightSmoothnessTex, float2(sample_point_x, 0)).r;

            // Set albedo (color) and smoothness
            o.Albedo = above_water * terrain_color + (1.0 - above_water) * _SandColor;
            o.Smoothness = terrain_smoothness;
            
            // Metallic comes from slider variable
            o.Metallic = _Metallic;
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
