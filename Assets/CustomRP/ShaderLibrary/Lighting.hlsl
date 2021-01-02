#ifndef CUSTOM_RP_LIGHTING_INCLUDED
#define CUSTOM_RP_LIGHTING_INCLUDED

float3 IncomingLight(Surface s, Light l){
    return saturate(dot(s.normal, l.direction) * l.attenuation) * l.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light){
    return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surfaceWS, BRDF brdf){
    ShadowData shadowData = GetShadowData(surfaceWS);
    float3 color = 0.0;
    for(int i =  0 ; i < GetDirectionalLightCount() ; i++) {
        Light light = GetDirectionalLight(i, surfaceWS, shadowData);
        color += GetLighting(surfaceWS, brdf, light);
    }
    return color;
}


#endif