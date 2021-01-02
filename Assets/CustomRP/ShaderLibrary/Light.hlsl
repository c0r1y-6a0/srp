#ifndef CUSTOM_RP_LIGHT_INCLUDED
#define CUSTOM_RP_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light{
    float3 color;
    float3 direction;
    float attenuation;
};

DirectionalShadowData GetDirectinalShadowData(int lightIndex){
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[lightIndex].x;
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y;
    return data;
}

int GetDirectionalLightCount () {
	return _DirectionalLightCount;
}

Light GetDirectionalLight(int index, Surface surfaceWS){
    Light l;
    l.color = _DirectionalLightColors[index].rgb;
    l.direction = _DirectionalLightDirections[index].xyz;
    DirectionalShadowData data = GetDirectinalShadowData(index);
    l.attenuation = GetDirectionalShadowAttenuation(data, surfaceWS);
    return l;
}

#endif