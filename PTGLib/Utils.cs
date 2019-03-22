using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Utils
{
    private const int X_PRIME = 1619;
    private const int Y_PRIME = 31337;
    private const int Z_PRIME = 6971;
    private const int W_PRIME = 1013;

    private static readonly Vector2[] GRAD_2D = {
        new Vector2(-1,-1), new Vector2( 1,-1), new Vector2(-1, 1), new Vector2( 1, 1),
        new Vector2( 0,-1), new Vector2(-1, 0), new Vector2( 0, 1), new Vector2( 1, 0),
    };

    static int FastFloor(float f) { return (f >= 0 ? (int)f : (int)f - 1); }
    static float Lerp(float a, float b, float t) { return a + t * (b - a); }

    static float Gradient2D(int seed, int x, int y, float xd, float yd)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        Vector2 g = GRAD_2D[hash & 7];

        return xd * g.x + yd * g.y;
    }
    public static float PerlinNoise(int seed, float x, float y)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);

        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float xs = x - x0;
        float ys = y - y0;

        float xd0 = x - x0;
        float yd0 = y - y0;
        float xd1 = xd0 - 1;
        float yd1 = yd0 - 1;

        float xf0 = Lerp(Gradient2D(seed, x0, y0, xd0, yd0), Gradient2D(seed, x1, y0, xd1, yd0), xs);
        float xf1 = Lerp(Gradient2D(seed, x0, y1, xd0, yd1), Gradient2D(seed, x1, y1, xd1, yd1), xs);

        return Lerp(xf0, xf1, ys);
    }

    

}
