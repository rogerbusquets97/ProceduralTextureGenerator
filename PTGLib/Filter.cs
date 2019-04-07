using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Filter
{
    public enum BlendMode { Multiply, Addition, Substraction, Mask}

    private delegate void FilterMethod(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters);

    private static Color[] TextureFilter(Vector2Int ressolution, FilterMethod method, params object[] parameters)
    {
        Color[] outPixels = new Color[ressolution.x * ressolution.y];

        if(method!= null)
        {
            method(ressolution, ref outPixels, parameters);
        }

        return outPixels;
    }

    public static int GetIndex(int x, int y, int width, int height)
    {
        int i = Mathf.Clamp(x, 0, width - 1) + Mathf.Clamp(y, 0, height - 1) * width;
        return i;
    }

    #region Blend
    public static Color[] Blend(Vector2Int ressolution, Color[]Input01,  Color[]Input02, BlendMode mode, Color[]mask = null)
    {
        return TextureFilter(ressolution, BlendFunc, Input01, Input02, mode, mask);
    }

    private static void BlendFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        Color[] img1 = (Color[])parameters[0];
        Color[] img2 = (Color[])parameters[1];
        BlendMode mode = (BlendMode)parameters[2];
        Color[] mask = (Color[])parameters[3];

        for(int y = 0; y< ressolution.y; y++)
        {
            for(int x = 0; x<ressolution.x; x++)
            { 
                if(mask == null)
                    outPixels[GetIndex(x,y,ressolution.x,ressolution.y)] = GetSingleBlendValue(ressolution, x, y, img1, img2, mode);
                else
                    outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = GetSingleBlendValue(ressolution, x, y, img1, img2, mode, mask);
            }
        }
    }

    public static Color GetSingleBlendValue(Vector2Int ressolution, int x, int y, Color[]Input01, Color[]Input02, BlendMode mode, Color[]mask = null)
    {
        switch(mode)
        {
            case BlendMode.Multiply:
                return Input01[GetIndex(x, y, ressolution.x, ressolution.y)] * Input02[GetIndex(x, y, ressolution.x, ressolution.y)];
            case BlendMode.Addition:
                return Input01[GetIndex(x, y, ressolution.x, ressolution.y)] + Input02[GetIndex(x, y, ressolution.x, ressolution.y)];
            case BlendMode.Substraction:
                float Rvalue = Mathf.Clamp01(Input01[GetIndex(x, y,ressolution.x,ressolution.y)].r - Input02[GetIndex(x, y,ressolution.x,ressolution.y)].r);
                float Gvalue = Mathf.Clamp01(Input01[GetIndex(x, y,ressolution.x,ressolution.y)].g - Input02[GetIndex(x, y,ressolution.x,ressolution.y)].g);
                float Bvalue = Mathf.Clamp01(Input01[GetIndex(x, y,ressolution.x,ressolution.y)].b - Input02[GetIndex(x, y,ressolution.x,ressolution.y)].b);
                return new Color(Rvalue, Gvalue, Bvalue);
            case BlendMode.Mask:
                if(mask!=null)
                {
                    return Color.Lerp(Input01[GetIndex(x, y, ressolution.x, ressolution.y)], Input02[GetIndex(x, y, ressolution.x, ressolution.y)], mask[GetIndex(x, y, ressolution.x, ressolution.y)].grayscale);
                }
                else
                {
                    return Input01[GetIndex(x, y, ressolution.x, ressolution.y)];
                }
            default:
                return Input01[GetIndex(x, y, ressolution.x, ressolution.y)];
        }
    }
    #endregion
    #region Levels
    public struct LevelsData
    {
        public Vector2 inputLevels;
        public Vector2 outputLevels;

        public LevelsData(Vector2 input, Vector2 output)
        {
            this.inputLevels = input;
            this.outputLevels = output;
        }
    }

    public static Color[] Levels(Vector2Int ressolution, Color[] source, LevelsData data)
    {
        return TextureFilter(ressolution, LevelsFunc, source, data);
    }

    private static void LevelsFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        Color[] source = (Color[])parameters[0];
        LevelsData data = (LevelsData)parameters[1];
        for (int y = 0; y < ressolution.y; y++)
        {
            for (int x = 0; x < ressolution.x; x++)
            {
                outPixels[GetIndex(x, y, ressolution.x, ressolution.y)] = GetSingleLevelsValue(ressolution,x,y,source,data);
            }
        }
    }

    public static Color GetSingleLevelsValue(Vector2Int ressolution, int x, int y, Color[] source, LevelsData data)
    {
        float rvalue = (source[GetIndex(x, y,ressolution.x,ressolution.y)].r - data.inputLevels.x) / (data.inputLevels.y - data.inputLevels.x);
        rvalue = (rvalue) * (data.outputLevels.y - data.outputLevels.x) + data.outputLevels.x;

        float gvalue = (source[GetIndex(x, y,ressolution.x,ressolution.y)].g - data.inputLevels.x) / (data.inputLevels.y - data.inputLevels.x);
        gvalue = (gvalue) * (data.outputLevels.y - data.outputLevels.x) + data.outputLevels.x;

        float bvalue = (source[GetIndex(x, y, ressolution.x, ressolution.y)].b - data.inputLevels.x) / (data.inputLevels.y - data.inputLevels.x);
        bvalue = (bvalue) * (data.outputLevels.y - data.outputLevels.x) + data.outputLevels.x;

        return new Color(rvalue, gvalue, bvalue);
    }


    #endregion
    #region Height to Normal
    public static Color[] Normal(Vector2Int ressolution, Color[] source, float strength)
    {
        return TextureFilter(ressolution, NormalFunc, source, strength);
    }

    private static void NormalFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        Color[] source = (Color[])parameters[0];
        float strength = (float)parameters[1];

        float xLeft, xRight, yUp, yDown, xDelta, yDelta;

        int y = 0;
        while (y < ressolution.y)
        {

            int x = 0;
            while (x < ressolution.x)
            {

                if (x == 0)
                    xLeft = source[GetIndex(x, y,ressolution.x,ressolution.y)].grayscale * strength;
                else
                    xLeft = source[GetIndex(x - 1, y,ressolution.x,ressolution.y)].grayscale * strength;

                if (x == ressolution.x - 1)
                    xRight = source[GetIndex(x, y,ressolution.x,ressolution.y)].grayscale * strength;
                else
                    xRight = source[GetIndex(x + 1, y,ressolution.x,ressolution.y)].grayscale * strength;

                if (y == 0)
                    yUp = source[GetIndex(x, y,ressolution.x, ressolution.y)].grayscale * strength;
                else
                    yUp = source[GetIndex(x, y - 1,ressolution.x,ressolution.y)].grayscale * strength;
                if (y == ressolution.y - 1)
                    yDown = source[GetIndex(x, y,ressolution.x,ressolution.y)].grayscale * strength;
                else
                    yDown = source[GetIndex(x, y + 1,ressolution.x,ressolution.y)].grayscale * strength;


                xDelta = ((xLeft - xRight) + 1) * 0.5f;
                yDelta = ((yUp - yDown) + 1) * 0.5f;


                outPixels[GetIndex(x, y,ressolution.x,ressolution.y)] = new Color(xDelta, yDelta, 1.0f, yDelta);
                x++;

            }

            y++;
        }


    }
    #endregion
    #region OneMinus
    public static Color[] OneMinus(Vector2Int ressolution, Color[] source)
    {
        return TextureFilter(ressolution, OneMinusFunc, source);
    }
    private static void OneMinusFunc(Vector2Int ressolution, ref Color[] outPixels, params object[] parameters)
    {
        Color[] source = (Color[])parameters[0];
        for (int i = 0; i < ressolution.x; i++)
        {
            for (int j = 0; j < ressolution.y; j++)
            {
                outPixels[GetIndex(i, j, ressolution.x, ressolution.y)] = GetSingleOneMinus(ressolution, i, j, source);
            }
        }

    }

    public static Color GetSingleOneMinus(Vector2Int ressolution, int x, int y, Color[] source)
    {
        float Rvalue;
        float Gvalue;
        float Bvalue;

        Rvalue = 1 - source[Filter.GetIndex(x, y, ressolution.x, ressolution.y)].r;
        Gvalue = 1 - source[Filter.GetIndex(x, y, ressolution.x, ressolution.y)].g;
        Bvalue = 1 - source[Filter.GetIndex(x, y, ressolution.x, ressolution.y)].b;

        return new Color(Rvalue, Gvalue, Bvalue);

    }
    #endregion

}
