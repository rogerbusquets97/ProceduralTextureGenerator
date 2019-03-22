using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum FractalType { Brownian, Ridged, Billow };
    public enum InterpolationType { Linear, Hermite, Quintic};
    private delegate void NoiseMethod(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters);

    private static Color[] TextureNoise(Vector2Int ressolution, NoiseMethod method, params object[] parameters)
    {
        Color[] outPixels = new Color[ressolution.x * ressolution.y];

        if(method!= null)
        {
            method(ressolution, ref outPixels, parameters);
        }

        return outPixels;
    }
    private static int GetIndex(int x, int y, int width, int height)
    {
        int i = Mathf.Clamp(x, 0, width - 1) + Mathf.Clamp(y, 0, height - 1) * width;
        return i;
    }

    #region Fractals
    public struct FractalSettings
    {
        public int seed;
        public float octaves;
        public float lacunarity;
        public float frquency;
        public float persistance;

        public FractalSettings(int seed, float octaves, float lacunarity, float frequency, float persistance)
        {
            this.seed = seed;
            this.octaves = octaves;
            this.lacunarity = lacunarity;
            this.frquency = frequency;
            this.persistance = persistance;
        }
    }
    
    public static float fBM(float x, float y, FractalSettings settings)
    {
        x *= settings.frquency;
        y *= settings.frquency;

        int mSeed = settings.seed;
        float total = Utils.PerlinNoise(mSeed, x, y);
        float amplitude = 1;

        for(int i = 1; i < settings.octaves; i++)
        {
            x *= settings.lacunarity;
            y *= settings.lacunarity;

            amplitude *= settings.persistance;

            total += Utils.PerlinNoise(++mSeed, x, y) * amplitude;
        }

        return total;
    }

    public static Color[] Fractal(Vector2Int ressolution, FractalSettings settings, FractalType type)
    {
        return TextureNoise(ressolution, BrownianFunc, settings); 
    }

    private static void BrownianFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        if (outPixels != null)
        {
            FractalSettings settings = (FractalSettings)parameters[0];
            for (int x = 0; x < ressolution.x; x++)
            {
                for (int y = 0; y < ressolution.y; y++)
                {
                    float value = fBM(x,y,settings);
                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = Color.Lerp(Color.black, Color.white, value);
                }
            }
        }
    }
    

    #endregion
}
