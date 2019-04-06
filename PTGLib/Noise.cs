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
        public int octaves;
        public float lacunarity;
        public float frequency;
        public float persistance;
        public FastNoise.FractalType fractalType;
        public FastNoise.NoiseType noiseType;
        public float XScale;
        public float YScale;

        public FractalSettings(int seed, int octaves, float lacunarity, float frequency, float persistance,float XScale, float YScale, FastNoise.FractalType fractalType, FastNoise.NoiseType noiseType)
        {
            this.seed = seed;
            this.octaves = octaves;
            this.lacunarity = lacunarity;
            this.frequency = frequency;
            this.persistance = persistance;
            this.fractalType = fractalType;
            this.noiseType = noiseType;
            this.XScale = XScale;
            this.YScale = YScale;
        }
        
       
    }

    public struct CellularSettings
    {
        public int seed;
        public FastNoise.NoiseType noiseType;
        public FastNoise.CellularDistanceFunction CellularDistanceFunc;
        public FastNoise.CellularReturnType returnType;
        public float frequency;
        public Vector2Int cellularIndices; //  Both indicies must be >= 0, index1 must be < 4 &&  index0 should be lower than index1
        public float jitter;
        public FastNoise lookup; // optional

        public CellularSettings(int seed, FastNoise.NoiseType noiseType, FastNoise.CellularDistanceFunction cellularDistanceFunc, FastNoise.CellularReturnType returnType, float frequency, Vector2Int cellularIndices, float jitter, FastNoise lookup = null )
        {
            this.seed = seed;
            this.noiseType = noiseType;
            this.CellularDistanceFunc = cellularDistanceFunc;
            this.returnType = returnType;
            this.frequency = frequency;
            this.cellularIndices = cellularIndices;
            this.jitter = jitter;
            this.lookup = lookup;
        }
    }
    
   public static Color[] Cellular(Vector2Int ressolution, CellularSettings settings, FastNoise noise)
    {
        return TextureNoise(ressolution, CellularFunc, settings, noise);
    }

    public static Color[] Fractal(Vector2Int ressolution, FractalSettings settings, FastNoise noise)
    {
        return TextureNoise(ressolution, FractalFunc, settings, noise);
    }

    public static float GetSingleFractal(float x, float y, Vector2Int ressolution, FractalSettings settings, FastNoise noise)
    {
        float u = (float)x / (float)ressolution.x;
        float v = (float)y / ressolution.y;

        float noise00 = noise.GetNoise(x * settings.XScale, y*settings.YScale) * 0.5f + 0.5f;
        float noise01 = noise.GetNoise(x*settings.XScale, y*settings.YScale + ressolution.y) * 0.5f + 0.5f;
        float noise10 = noise.GetNoise(x*settings.XScale + ressolution.x, y*settings.YScale) * 0.5f + 0.5f;
        float noise11 = noise.GetNoise(x*settings.XScale + ressolution.x, y*settings.YScale + ressolution.y) * 0.5f + 0.5f;

        float totalNoise = u * v * noise00 + u * (1 - v) * noise01 + (1 - u) * v * noise10 + (1 - u) * (1 - v) * noise11;

        float totalValue = (int)(256 * totalNoise) + 50;
        float r = Mathf.Clamp((int)noise00, 0, 255);
        float g = Mathf.Clamp(totalValue, 0, 255);
        float b = Mathf.Clamp(totalValue + 50, 0, 255);

        return (r + g + b) / (3 * 255.0f);
    }

    public static float GetSingleCellular(float x, float y, Vector2Int ressolution, CellularSettings settigns, FastNoise noise)
    {
        return noise.GetNoise(x, y) * 0.5f + 0.5f;
    }

    private static void CellularFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        CellularSettings settings = (CellularSettings)parameters[0];
        FastNoise noise = (FastNoise)parameters[1];
        noise.SetCellularDistance2Indicies(settings.cellularIndices.x, settings.cellularIndices.y);
        float freq = settings.frequency;
        if (ressolution.x == 256)
            freq = settings.frequency;
        else if (ressolution.x == 512)
            freq = settings.frequency / 2;
        else if (ressolution.x == 1024)
            freq = settings.frequency / 4;
        else if (ressolution.x == 2048)
            freq = settings.frequency / 8;
        else if (ressolution.x == 4096)
            freq = settings.frequency / 16;
        noise.SetFrequency(freq);
        noise.SetNoiseType(settings.noiseType);
        noise.SetCellularDistanceFunction(settings.CellularDistanceFunc);
        noise.SetCellularJitter(settings.jitter);
        noise.SetCellularReturnType(settings.returnType);
        if(settings.lookup!= null)
            noise.SetCellularNoiseLookup(settings.lookup);

        if(outPixels!= null)
        {
            float minColor = 1f;
            float maxColor = 0f;
            Color newColor;

            for (int y = 0; y < ressolution.y; y++)
            {
                for (int x = 0; x < ressolution.x; x++)
                {
                    float value = GetSingleCellular(x, y, ressolution, settings, noise);
                    float colValue = value;
                    if (minColor > colValue) minColor = colValue;
                    if (maxColor < colValue) maxColor = colValue;
                    newColor = new Color(colValue, colValue, colValue, 1);
                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = newColor;
                }
            }

            for (int y = 0; y < ressolution.y; y++)
            {
                for (int x = 0; x < ressolution.x; x++)
                {
                    newColor = outPixels[GetIndex(x, y, ressolution.x, ressolution.y)];
                    float colValue = newColor.r;
                    colValue = Map(colValue, minColor, maxColor, 0, 1);
                    newColor.r = colValue;
                    newColor.g = colValue;
                    newColor.b = colValue;

                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = newColor;

                }
            }

        }
    }
    private static void FractalFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        FractalSettings settings = (FractalSettings)parameters[0];
        FastNoise noise = (FastNoise)parameters[1];
        noise.SetFractalOctaves(settings.octaves);
        noise.SetFractalGain(settings.persistance);
        noise.SetFractalLacunarity(settings.lacunarity);
        noise.SetSeed(settings.seed);
        float freq = settings.frequency;
        if (ressolution.x == 256)
            freq = settings.frequency;
        else if (ressolution.x == 512)
            freq = settings.frequency / 2;
        else if (ressolution.x == 1024)
            freq = settings.frequency / 4;
        else if (ressolution.x == 2048)
            freq = settings.frequency / 8;
        else if (ressolution.x == 4096)
            freq = settings.frequency / 16;
        noise.SetFrequency(freq);
        noise.SetFractalType(settings.fractalType);
        noise.SetNoiseType(settings.noiseType);

        if (outPixels != null)
        {
            float minColor = 1f;
            float maxColor = 0f;
            Color newColor; 

            for (int y = 0; y < ressolution.y; y++)
            {
                for (int x = 0; x < ressolution.x; x++)
                {
                    float value = GetSingleFractal(x, y,ressolution, settings,noise);
                    float colValue = value;
                    if (minColor > colValue) minColor = colValue;
                    if (maxColor < colValue) maxColor = colValue;
                    newColor = new Color(colValue, colValue, colValue, 1);
                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = newColor;
                }
            }

            for(int y = 0;y<ressolution.y;y++)
            {
                for(int x = 0; x<ressolution.x; x++)
                {
                    newColor = outPixels[GetIndex(x, y, ressolution.x, ressolution.y)];
                    float colValue = newColor.r;
                    colValue = Map(colValue, minColor, maxColor, 0, 1);
                    newColor.r = colValue;
                    newColor.g = colValue;
                    newColor.b = colValue;

                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = newColor;

                }
            }
        }
    }

    private static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
  

    #endregion
}
