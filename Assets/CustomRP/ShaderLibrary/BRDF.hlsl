#ifndef CUSTOM_RP_BRDF
#define CUSTOM_RP_BRDF 

struct BRDF{
    float3 diffuse;
    float3 specular;
    float roughness;
};

#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity (float metallic) {
	float range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}

BRDF GetBRDF(Surface s, bool premultipliedDiffuse = false){
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(s.smoothness);
    BRDF b;
    b.diffuse = s.color * OneMinusReflectivity(s.metallic);
    if(premultipliedDiffuse)
    {
        b.diffuse *= s.alpha;
    }
    b.specular = lerp(MIN_REFLECTIVITY, s.color, s.metallic);
    b.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return b;
}

float SpecularStrength (Surface surface, BRDF brdf, Light light) {
	float3 h = SafeNormalize(light.direction + surface.viewDirection);
	float nh2 = Square(saturate(dot(surface.normal, h)));
	float lh2 = Square(saturate(dot(light.direction, h)));
	float r2 = Square(brdf.roughness);
	float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = brdf.roughness * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 DirectBRDF(Surface s, BRDF b, Light l){
    return SpecularStrength(s, b, l) * b.specular + b.diffuse;
}

#endif