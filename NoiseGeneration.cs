using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGeneration
{
    public static float[,] PerlinNoise(int xSize, int ySize, int layers, int seed, float frequency, float layerChange)
    {
        Random.InitState(seed);
        int newSeed = Random.Range(-10000, 10000);

        float[,] result = new float[xSize, ySize];

        float max = float.MinValue;
        float amplitude = 1f;

        for (int currLayer = 0; currLayer < layers; currLayer++)
        {
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    float xCoord = newSeed + (float)i / xSize * frequency;
                    float yCoord = newSeed + (float)j / ySize * frequency;

                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    result[i, j] += sample * amplitude;

                    if(result[i, j] > max)
                    {
                        max = result[i, j];
                    }
                }
            }

            frequency *= layerChange;
            amplitude /= layerChange;
            seed *= 2;
        }

        for(int i = 0; i < result.GetLength(0); i++)
        {
            for(int j = 0; j < result.GetLength(1); j++)
            {
                result[i, j] /= max;
            }
        }

        return result;
    }
}
