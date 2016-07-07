// An implementation of input SH coefficients to shaders by volume textures for Unity 5.3

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CreateLightVolume : MonoBehaviour {

    public Vector3 resoultion = new Vector3(1, 0.25f, 0.5f);
    public Vector3 maxVolumeSize = new Vector3(256, 128, 128);

    public Texture3D[] _volumeLightProbe;
    public Vector3 _minVec;
    public Vector3 _volumeTexSize;
    public Color _defaultLightProbeColor;
    int _colorChannel = 3;


    void Start ()
    {
        _volumeLightProbe = new Texture3D[3];
        BuildLightVolume();

    }

    bool CheckLightProbe()
    {
        LightProbes probes = LightmapSettings.lightProbes;
        if (probes == null)
        {
            CreateDefaultVolume();
            return false;
        }

        return true;
    }
    void OnValidate()
    {
        if(!CheckLightProbe())
            return;

        BuildLightVolume();
   
    }

    void BuildLightVolume()
    {

        LightProbes probes = LightmapSettings.lightProbes;

        if (probes == null || probes.positions.Length < 1)
        {
            CreateDefaultVolume();
            return;
        }

        // Calculate the bounding of light probes
        Vector3 maxVec = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3 minVec = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        foreach (Vector3 vec in probes.positions)
        {
            maxVec = Vector3.Max(vec, maxVec);
            minVec = Vector3.Min(vec, minVec);
        }


        // Compute the size of volume texture according to _resoultion
        Vector3 center = (maxVec + minVec) / 2;
        Vector3 size = (maxVec - minVec);
        Vector3 numVoxels;
        numVoxels.x = size.x / resoultion.x + 0.5f;
        numVoxels.y = size.y / resoultion.y + 0.5f;
        numVoxels.z = size.z / resoultion.z + 0.5f;

        numVoxels = new Vector3(Mathf.NextPowerOfTwo((int)numVoxels.x), Mathf.NextPowerOfTwo((int)numVoxels.y), Mathf.NextPowerOfTwo((int)numVoxels.z));

        if (numVoxels.x > maxVolumeSize.x)
        {
            numVoxels.x = maxVolumeSize.x;
        }
        if (numVoxels.y > maxVolumeSize.y)
        {
            numVoxels.y = maxVolumeSize.y;
        }
        if (numVoxels.z > maxVolumeSize.z)
        {
            numVoxels.z = maxVolumeSize.z;
        }

        // Store SH probes with the texture format RGBA32
        // 8 bit for each coefficient
        for (int i = 0; i < _colorChannel; i++)
        {
            _volumeLightProbe[i] = new Texture3D((int)numVoxels.x, (int)numVoxels.y, (int)numVoxels.z, TextureFormat.RGBA32, false);
        }


        // Allocate arrays to store SH coefficients
        Vector3 offset = new Vector3(size.x / numVoxels.x, size.y / numVoxels.y, size.z / numVoxels.z);
        Color[] colors = new Color[(int)(numVoxels.x * numVoxels.y * numVoxels.z)];
        Color[] colorsR = new Color[(int)(numVoxels.x * numVoxels.y * numVoxels.z)];
        Color[] colorsG = new Color[(int)(numVoxels.x * numVoxels.y * numVoxels.z)];
        Color[] colorsB = new Color[(int)(numVoxels.x * numVoxels.y * numVoxels.z)];
        Renderer renderer = GetComponent<Renderer>();
        int counter = 0;

        // Interpolate SH coefficients in each cell center
        for (int k = 0; k < numVoxels.z; k++)
        {
            for (int j = 0; j < numVoxels.y; j++)
            {
                for (int i = 0; i < numVoxels.x; i++)
                {

                    Vector3 aVoxelCen = minVec + new Vector3(offset.x * i, offset.y * j, offset.z * k);
                    aVoxelCen += offset * 0.5f;

                    UnityEngine.Rendering.SphericalHarmonicsL2 sh;
                    LightProbes.GetInterpolatedProbe(aVoxelCen, renderer, out sh);
                    // Remap negative values to positive
                    colors[counter] = new Color( sh[0, 3] * 0.5f + 0.5f, sh[0, 1] * 0.5f + 0.5f, sh[0, 2] * 0.5f + 0.5f, sh[0, 0] * 0.5f + 0.5f);
                    colorsR[counter] = new Color( sh[0, 3] * 0.5f + 0.5f,  sh[0, 1] * 0.5f + 0.5f,  sh[0, 2] * 0.5f + 0.5f, sh[0, 0] * 0.5f + 0.5f);
                    colorsG[counter] = new Color( sh[1, 3] * 0.5f + 0.5f,  sh[1, 1] * 0.5f + 0.5f,  sh[1, 2] * 0.5f + 0.5f, sh[1, 0] * 0.5f + 0.5f);
                    colorsB[counter] = new Color( sh[2, 3] * 0.5f + 0.5f,  sh[2, 1] * 0.5f + 0.5f,  sh[2, 2] * 0.5f + 0.5f, sh[2, 0] * 0.5f + 0.5f);
                    counter++;
                }
            }
        }
        _volumeLightProbe[0].SetPixels(colorsR);
        _volumeLightProbe[1].SetPixels(colorsG);
        _volumeLightProbe[2].SetPixels(colorsB);

        // Clamp SH coefficients when out of bounding
        for (int channel = 0; channel < _colorChannel; channel++)
        {
            _volumeLightProbe[channel].wrapMode = TextureWrapMode.Clamp;
            _volumeLightProbe[channel].Apply();
        }


        // Set parameters to all shaders
        Shader.SetGlobalTexture("_LightVolumeR", _volumeLightProbe[0]);
        Shader.SetGlobalTexture("_LightVolumeG", _volumeLightProbe[1]);
        Shader.SetGlobalTexture("_LightVolumeB", _volumeLightProbe[2]);

        _minVec = minVec;
        _volumeTexSize = size;
        Shader.SetGlobalVector("_LightVolumeMin", _minVec);
        Shader.SetGlobalVector("_LightVolumeSize", _volumeTexSize);
    }
    public void ResetParameters()
    {
        Shader.SetGlobalTexture("_LightVolumeR", _volumeLightProbe[0]);
        Shader.SetGlobalTexture("_LightVolumeG", _volumeLightProbe[1]);
        Shader.SetGlobalTexture("_LightVolumeB", _volumeLightProbe[2]);
        Shader.SetGlobalVector("_LightVolumeMin", _minVec);
        Shader.SetGlobalVector("_LightVolumeSize", _volumeTexSize);
    }
    void CreateDefaultVolume()
    {
        for (int i = 0; i < _colorChannel; i++)
        {
            _volumeLightProbe[i] = new Texture3D(2, 2, 2, TextureFormat.RGBA32, false);
        }


        UnityEngine.Rendering.SphericalHarmonicsL2 sh;
        sh = new UnityEngine.Rendering.SphericalHarmonicsL2();
        sh.AddAmbientLight(_defaultLightProbeColor);

        Color[] colorsR = new Color[8];
        Color[] colorsG = new Color[8];
        Color[] colorsB = new Color[8];

        int counter = 0;

        for (int k = 0; k < 2; k++)
        {

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    colorsR[counter] = new Color( sh[0, 3] * 0.5f + 0.5f,  sh[0, 1] * 0.5f + 0.5f,  sh[0, 2] * 0.5f + 0.5f,  sh[0, 0] * 0.5f + 0.5f);
                    colorsG[counter] = new Color( sh[1, 3] * 0.5f + 0.5f,  sh[1, 1] * 0.5f + 0.5f,  sh[1, 2] * 0.5f + 0.5f,  sh[1, 0] * 0.5f + 0.5f);
                    colorsB[counter] = new Color( sh[2, 3] * 0.5f + 0.5f,  sh[2, 1] * 0.5f + 0.5f,  sh[2, 2] * 0.5f + 0.5f,  sh[2, 0] * 0.5f + 0.5f);
                    counter++;
                }
            }
        }



        _volumeLightProbe[0].SetPixels(colorsR);
        _volumeLightProbe[1].SetPixels(colorsG);
        _volumeLightProbe[2].SetPixels(colorsB);
        for (int channel = 0; channel < _colorChannel; channel++)
        {
            _volumeLightProbe[channel].wrapMode = TextureWrapMode.Clamp;
            _volumeLightProbe[channel].Apply();
        }
        Shader.SetGlobalTexture("_LightVolumeR", _volumeLightProbe[0]);
        Shader.SetGlobalTexture("_LightVolumeG", _volumeLightProbe[1]);
        Shader.SetGlobalTexture("_LightVolumeB", _volumeLightProbe[2]);

        _minVec = new Vector3(0, 0, 0);
        _volumeTexSize = new Vector3(2, 2, 2);
        Shader.SetGlobalVector("_LightVolumeMin", _minVec);
        Shader.SetGlobalVector("_LightVolumeSize", _volumeTexSize);
    }
}
