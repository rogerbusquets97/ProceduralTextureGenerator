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
                return Input01[GetIndex(x, y, ressolution.x, ressolution.y)] - Input02[GetIndex(x, y, ressolution.x, ressolution.y)];
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


}
