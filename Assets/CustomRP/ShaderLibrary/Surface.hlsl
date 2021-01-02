#ifndef CUSTOM_RP_SURFACE_INCLUDED
#define CUSTOM_RP_SURFACE_INCLUDED

struct Surface
{
    float3 positionWS;
    float3 normal;
    float3 viewDirection; // in wordl space 
    float3 color;
    float alpha;
    float metallic;
	float smoothness;
};

#endif