Shader "SRP/srp_unlit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white"{}
        _BaseColor("Base Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1]_ZWrite("ZWrite", Float) = 1

        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0
    }
    SubShader
    {

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma target 3.5
            
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex unlit_vert
            #pragma fragment unlit_frag

            #include "unlit.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "CustomShaderGUI"
}
