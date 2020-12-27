﻿Shader "SRP/srp_lit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white"{}
        _BaseColor("Base Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1]_ZWrite("ZWrite", Float) = 1
        [Toggle(_PREMULTIPLIED_ALPHA)] _PremulAlpha("Premutiplied Aplha", Float) = 0

        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0

        _Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader
    {

        Pass
        {
            Tags
            {
                "LightMode" = "CustomLit"
            }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma target 3.5

            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMULTIPLIED_ALPHA
            #pragma multi_compile_instancing
            #pragma vertex lit_vert
            #pragma fragment lit_frag

            #include "lit.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "CustomShaderGUI"
}
