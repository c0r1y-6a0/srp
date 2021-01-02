#ifndef CUSTOM_RP_SHADOWS_INCLUDED
#define CUSTOM_RP_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
CBUFFER_END

struct DirectionalShadowData{
    float strength; 
    int tileIndex;
};

struct ShadowData{
    int cascadeIndex;
    float strength;
};

ShadowData GetShadowData(Surface surface){
    ShadowData data;
    int i;
    for( i = 0 ; i < _CascadeCount ; i++){
        float4 sphere = _CascadeCullingSpheres[i];
        if(DistanceSquared(surface.positionWS, sphere.xyz) < sphere.w){
            break;
        }
    }
    data.strength = i == _CascadeCount ? 0.0 : 1.0;
    data.cascadeIndex = i;
    return data;
}

float SampleDirectionalShadowAtlas(float3 positionSTS){//不是特别理解此处
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surface){
    if(data.strength <= 0.0){
        return 1.0;
    }

    float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex], float4(surface.positionWS, 1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, data.strength);
}

#endif