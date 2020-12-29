#ifndef CUSTOM_RP_LIGHTING
#define CUSTOM_RP_LIGHTING

float3 IncomingLight(Surface s, Light l){
    return saturate(dot(s.normal, l.direction)) * l.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light){
    return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surface, BRDF brdf){
    float3 color = 0.0;
    for(int i =  0 ; i < GetDirectionalLightCount() ; i++)
    {
        color += GetLighting(surface, brdf, GetDirectionalLight(i));
    }
    return color;
}


#endif