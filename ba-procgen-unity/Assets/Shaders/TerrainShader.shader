Shader "Custom/TerrainShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _MinHeight ("Min Height", Range(-10.0, 10.0)) = 0.0
        _MaxHeight ("Max Height", Range(0.0, 20.0)) = 15.0
        _WaterLevel ("Water Level", float) = 0.0
        _HeightColorsTex ("Height Color Gradient (RGB)", 2D) = "white" {}
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

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float _MinHeight;
        float _MaxHeight;
        float _WaterLevel;
        sampler2D _HeightColorsTex;
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
            
            // Set color dependant on height
            float current_height = IN.worldPos.y;
            uint above_water = step(_WaterLevel, current_height);
            float sample_point_x = map(_MinHeight, _MaxHeight, 0.0, 1.0, current_height);
            float4 terrain_color = tex2D(_HeightColorsTex, float2(sample_point_x, 0));
            o.Albedo = above_water * terrain_color + (1.0 - above_water) * _SandColor;
            
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
