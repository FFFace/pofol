﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Custom/test"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("NormalMap", 2D) = "white" {}
        _DissolveMap("DissolveMap", 2D) = "whtie" {}
        _DissolveColor("DissolveColor", Color) = (1,1,1,1)
        _DissolveAmount("DissolveAmount", Range(0,1)) = 0
        _DissolveWidth("DissolveWidth", Range(0,1)) = 0
        _AlphaTest("Alpha", Range(0,1)) = 0
        _OutlineBold("Outline Bold", Range(-1,1)) = 0.1
        _Specular("Specular", Range(0,1)) = 0.0
        _SpecularColor("SpecularColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf _BandedLighting fullforwardshadows alphatest:_AlphaTest vertex:vertex
        //#pragma vertex vertex
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _DissolveMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_DissolveMap;
            float3 viewDir;
            float3 lightDir;
            float3 vertex;
            float3 worldNormal;
            //float outline;
        };

        half _Glossiness;
        half _Metallic;
        half _OutlineBold;
        half _DissolveAmount;
        half _DissolveWidth;
        half _Specular;
        fixed4 _Color;
        fixed4 _DissolveColor;
        fixed4 _SpecularColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vertex(inout appdata_tan i, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.lightDir = WorldSpaceLightDir(i.vertex);

            //float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
            //float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
            //float3 worldNormal = UnityObjectToWorldNormal(i.normal);

            //float outline = dot(worldViewDir, worldNormal) * 0.5f + 0.5f;
            //outline -= _OutlineBold;
            //outline = ceil(outline * 5) / 5;

            //o.outline = outline;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            fixed4 mask = tex2D(_DissolveMap, IN.uv_DissolveMap);
            //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            
            /*float x = ceil(IN.vertex * 5) / 5;
            col.x = x;*/

            half dissolve = ceil(mask.r - (_DissolveAmount + _DissolveWidth));

            half toneDot = dot(IN.lightDir, IN.worldNormal) * 0.5f + 0.5f;
            half tone = ceil(toneDot * 3) / 3;

            half outline = dot(IN.viewDir, IN.worldNormal) * 0.5f + 0.5f;
            outline -= _OutlineBold;
            outline = ceil(outline);

            //outline* tone
            o.Albedo = (c * tone * outline * dissolve) + (_DissolveColor * (ceil(mask.r) - dissolve));
            dissolve = ceil(mask.r - _DissolveAmount);
            o.Alpha = dissolve;            

            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
            o.Emission = o.Albedo;
        }

        float4 Lighting_BandedLighting(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
        {
            float3 fSpecularColor;
            float3 fReflectVector = reflect(-lightDir, s.Normal);
            float fRDotV = saturate(dot(fReflectVector, viewDir));
            fSpecularColor = pow(fRDotV, _Specular) * _SpecularColor.rgb;

            ////! 최종 컬러 출력
            float4 fFinalColor;
            fFinalColor.rgb = (s.Albedo) + fSpecularColor;
            fFinalColor.a = s.Alpha;

            return fFinalColor;
        }

        ENDCG
    }
    FallBack "Diffuse"
}