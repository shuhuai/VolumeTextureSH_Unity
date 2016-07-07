# VolumeTextureSH_Unity
###Store SH coefficients to volume textures for Unity 5.3

This project shows a demo which stores SH coefficients to three 32bit volume textures. It enables vertex and pixel shaders to interpolate SH coefficients instead of using CPU. For a object, using world positions in shaders to interpolate SH coefficients is more accurate that using the object position.

An example to show the difference is shown below:
<img src="https://github.com/shuhuai/VolumeTextureSH_Unity/blob/master/comparsion.png" width="800px" height="225px"/>

###How to use:
1) Bake light probes for a scene.

2) Add "CreateLightVolume" script.

3) Include "VolumeSH.cginc" in a shader.

4) Call "VolumeSH12Order" function to interpolate SH by passing world-space normal and position.
