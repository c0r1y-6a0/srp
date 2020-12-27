#ifndef CUSTOM_RP_SURFACE
#define CUSTOM_RP_SURFACE

struct Surface
{
    float3 normal;
    float3 viewDirection; // in wordl space 
    float3 color;
    float alpha;
    float metallic;
	float smoothness;
};

#endif