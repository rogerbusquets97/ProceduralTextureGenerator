using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
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
        public float Xscale;
        public float Yscale;
        public int octaves;
        public float persistance;
        public int offsetx;
        public int offsety;
    }
    public enum FractalType { Brownian, Ridged, Billow};
    public static float fBM(float x, float y, FractalSettings settings)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for(int i = 0; i<settings.octaves; i++)
        {
            total += Mathf.PerlinNoise((x) * frequency, (y) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= settings.persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public static Color[] Fractal(Vector2Int ressolution, FractalSettings settings, FractalType type)
    {
        return TextureNoise(ressolution, BrownianFunc, settings); 
    }

    private static void BrownianFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        Debug.Log(ressolution);
        if (outPixels != null)
        {
            FractalSettings settings = (FractalSettings)parameters[0];
            for (int x = 0; x < ressolution.x; x++)
            {
                for (int y = 0; y < ressolution.y; y++)
                {
                    float value = fBM((x+settings.offsetx) * settings.Xscale, (y+settings.offsety) * settings.Yscale, settings);
                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = Color.Lerp(Color.black, Color.white, value);
                }
            }
        }
    }
    

    #endregion
}
