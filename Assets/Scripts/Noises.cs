using NUnit.Framework;
using System;
using UnityEngine;

public static class Noises {
    /// <summary>
    /// Noise function for plane terrain generation.
    /// </summary>
    public static float GeneratePlaneTerrainValue(Vector3 position, Vector3 offset, float scale = 1f, float horizon = 0f) {
        float horizonValue = (position.y - horizon) / scale;
        position += offset;
        position /= scale;
        float value = horizonValue + OctavePerlinNoise(new Vector2(position.x, position.z), Vector3.zero);
        return value;
    }
    
    /// <remarks>
    /// This function might not be very efficient, as it calculate pow many times.
    /// </remarks>
    public static float OctavePerlinNoise(Vector2 position, Vector2 offset, float scale = 1f, int octaves = 4, float persistence = 0.5f, float lacunarity = 2f) {
        float noiseValue = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        position += offset;
        position /= scale;
        Func<float, int, float> QuickPow = (float a, int x) => {
            int i = octaves;
            float res = 1f;
            while (i > 0) {
                if ((i & 1) == 1) {
                    res *= a;
                }
                i >>= 1;
                a *= a;
            }
            return res;
        };
        float maxNoiseValue = (1 - QuickPow(persistence, octaves)) / (1 - persistence);
        float minNoiseValue = 0f;
        
        for (int i = 0; i < octaves; i++) {
            float x = position.x * frequency;
            float y = position.y * frequency;
            noiseValue += Mathf.PerlinNoise(x, y) * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        Assert.IsTrue(noiseValue >= minNoiseValue && noiseValue <= maxNoiseValue, "Noise value out of bounds.");
        noiseValue = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseValue);
        
        return noiseValue;
    }

    public static float PerlinNoise3D(float x, float y, float z) {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);

        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }
}
