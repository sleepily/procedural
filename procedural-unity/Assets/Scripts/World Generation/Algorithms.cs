using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Algorithms
{
    /*
     * Calculates the world's terrain structure.
     */
    public static float CalculatePerlinNoiseFloat(float x, float z, float seed, float perlinDivisor, float perlinStrength)
    {
        // get perlin noise value
        float output = 0f;
        output += Mathf.PerlinNoise(seed + (x / perlinDivisor), seed + (z / perlinDivisor)) * perlinStrength;
        output += Mathf.PerlinNoise(seed + (x / perlinDivisor / 2), seed + (z / perlinDivisor / 2)) * perlinStrength / 3;
        output /= 2;

        return output;
    }

    public static int CalculatePerlinNoiseInt(float x, float z, float seed, float perlinDivisor, float perlinStrength)
    {
        return Mathf.RoundToInt(CalculatePerlinNoiseFloat(x, z, seed, perlinDivisor, perlinStrength));
    }
}
