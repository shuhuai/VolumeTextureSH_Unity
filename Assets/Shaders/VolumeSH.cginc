#ifndef VOLUME_SH_INCLUDED
#define VOLUME_SH_INCLUDED

#define USE_VOLUME_SH 1
// SH volume textures
sampler3D _LightVolumeR;
sampler3D _LightVolumeG;
sampler3D _LightVolumeB;
// Light probes position and size
float3 _LightVolumeMin;
float3 _LightVolumeSize;

inline float3 VolumeSH12Order(float4 normal, float3 worldPos,float3 ambient)
{
	
	float3 pos = ((worldPos - _LightVolumeMin) / _LightVolumeSize);

	// Accumulate R G B intensity
	half3 x1;
	x1.r = dot(normal, tex3Dlod(_LightVolumeR, float4(pos, 0)) * 2 - 1);
	x1.g = dot(normal, tex3Dlod(_LightVolumeG, float4(pos, 0)) * 2 - 1);
	x1.b = dot(normal, tex3Dlod(_LightVolumeB, float4(pos, 0)) * 2 - 1);
	ambient = max(half3(0, 0, 0), x1 + ambient);


	return ambient;
}


#endif
