using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Noise
{
    public enum FractalType { Brownian, Ridged, Billow };
    public enum InterpolationType { Linear, Hermite, Quintic};
    public enum GeneratorType { Checker, Bricks};
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
    #region Checker Pattern
    public static Color[] Checker(Vector2Int ressolution, int squares)
    {
        return TextureNoise(ressolution, CheckerFunc, squares);
    }
    private static void CheckerFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        int squares = (int)parameters[0];
        for(int i = 0; i<ressolution.x; i++)
        {
            for(int j = 0; j<ressolution.y;j++)
            {
                outPixels[GetIndex(i, j, ressolution.x, ressolution.y)] = GetSingleCheckerValue(ressolution,i,j, squares);
            }
        }
    }

    public static Color GetSingleCheckerValue(Vector2Int ressolution, int x, int y, int squares)
    {
       if ((((x/(ressolution.x/squares)) + y/(ressolution.y/squares)) % 2) == 1)
        {
            return Color.black;
        }
        else
        {
            return Color.white;
        }
    }
    #endregion
    #region Bricks
    public static Color[] TileGenerator(Vector2Int ressolution)
    {
        return TextureNoise(ressolution, GeneratorFunc);
    }

    private static void GeneratorFunc(Vector2Int ressolution,ref Color[] outPixels, params object[] parameters)
    {
        for(int x = 0; x<ressolution.x; x++)
        {
            for(int y = 0; y < ressolution.y; y++)
            {
                outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = GetSingleBricksValue(ressolution,x,y);
            }
        }
    }
    public static Color GetSingleBricksValue(Vector2Int ressolution, int x, int y)
    {
        Vector2 st = new Vector2((float)x / (float)ressolution.x, (float)y / (float)ressolution.y);
        st = Tile(st, 5.0f);
        st = rotate2d(st, (float)(Mathf.PI * 0.25));
        float value = box(st, new Vector2(0.9f, 0.9f));
        //float value = circle(st, 0.5f);
        return new Color(value, value, value);
    }

    private static Vector2 Tile(Vector2 _st, float _zoom)
    {
        _st *= _zoom;
        _st.x += Step(1f, mod(_st.y, 2.0f)) * 0.5f;
        return fract(_st);
    }

    private static float box(Vector2 _st, Vector2 _size)
    {
         _size = new Vector2(0.5f, 0.5f) - _size * 0.5f;
         float u = SmoothStep(_size.x, _size.x + (1*Mathf.Pow(10,-4)), _st.x);
         u *= SmoothStep(_size.x, _size.x + (1*Mathf.Pow(10,-4)), 1 - _st.x);

         float v = SmoothStep(_size.y, _size.y + (1*Mathf.Pow(10,-4)), _st.y);
         v *= SmoothStep(_size.y, _size.y + (1*Mathf.Pow(10,-4)), 1 - _st.y);

         return u * v;
    }
    private static float circle(Vector2 _st, float _radius)
    {
        Vector2 l = _st - new Vector2(0.5f, 0.5f);
        return 1f - SmoothStep(_radius - (_radius * 0.01f), _radius + (_radius * 0.01f), Vector2.Dot(l, l) * 4.0f);
    }
    #endregion

    #region Utilities

    public struct mat2x2
    {
        public Vector2[] elements;

        public mat2x2(Vector2 first, Vector2 second)
        {
            elements = new Vector2[2];
            elements[0] = first;
            elements[1] = second;
        }

        public static Vector2 operator* (mat2x2 v2, Vector2 v)
        {
            Vector2 result = new Vector2();
            result.x = v2.elements[0].x * v.x + v2.elements[0].y*v.y;
            result.y = v2.elements[1].x * v.x + v2.elements[1].y * v.y;

            return result;
        }
    }

    public static mat2x2 rotate2d(float _angle)
    {
        Vector2 first = new Vector2(Mathf.Cos(_angle), -Mathf.Sin(_angle));
        Vector2 second = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle));

        return new mat2x2(first, second);
    }

    public static Vector2 rotate2d(Vector2 _st, float _angle)
    {
        _st -= new Vector2(0.5f, 0.5f);
        _st = new mat2x2(new Vector2(Mathf.Cos(_angle), -Mathf.Sin(_angle)),
                new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle))) * _st;
        _st += new Vector2(0.5f, 0.5f);

        return _st;
    }

    public static float Step(float edge, float x)
    {
        return x < edge ? 0f : 1f;
    }
    public static float mod(float x, float y)
    {
        return x - y * Mathf.FloorToInt(x / y);
    }
    public static Vector2 fract(Vector2 x)
    {
        return new Vector2(x.x - Mathf.Floor(x.x), x.y - Mathf.Floor(x.y));
    }
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }
    #endregion
}
