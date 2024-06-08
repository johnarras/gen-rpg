using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;
using GEntity = UnityEngine.GameObject;

// https://redirect.cs.umbc.edu/~olano/s2002c36/ch02.pdf Java Impl pg 24

public class NoiseGen
{
    private int _i, _j, _k;
    private int[] _a = new int[3];
    private float _u, _v, _w;
    private int[] _t; // = {0x15, 0x38, 0x32, 0x2c, 0x0d, 0x13, 0x07, 0x2a}


    public const int TSize = 8;

    public float[,] Generate(double pers, double freq, double amp, int octaves, long seedIn,
        int width, int height, double lacunarity = 2)
    {
        if (width < 1 || height < 1)
        {
            return new float[0, 0];
        }
        _t = new int[TSize];

        int seed = (int)(seedIn % (1 << 31));

        MyRandom rand = new MyRandom(seed);

        for (int i = 0; i < TSize; i++)
        {
            _t[i] = rand.Next();
        }

        float[,] retval = new float[width, height];

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                retval[w, h] = Noise3d((float)freq * w / width, (float)freq * h / height, 0, (float)pers, (float)amp, octaves, (float)lacunarity);
            }
        }


        return retval;

    }

    private float Noise3d(float x, float y, float z, float pers, float amp, int octaves, float lacunarity)
    {
        float val = 0;
        for (int n = 0; n < octaves; n++)
        {
            val += GetNoise(x, y, z) * amp;
            x *= lacunarity;
            y *= lacunarity;
            z *= lacunarity;
            amp *= pers;
        }
        return val;
    }

    private float GetNoise(float x, float y, float z)
    {
        float s = (x + y + z) / 3;
        _i = (int)System.Math.Floor(x + s);
        _j = (int)System.Math.Floor(y + s);
        _k = (int)System.Math.Floor(z + s);

        s = (_i + _j + _k)/6.0f;
        _u = x - _i + s;
        _v = y - _j + s;
        _w = z - _k + s;

        _a[0] = 0; _a[1] = 0; _a[2] = 0;

        int hi = _u >= _w ? _u >= _v ? 0 : 1 : _v >= _w ? 1 : 2;
        int lo = _u < _w ? _u < _v ? 0 : 1 : _v < _w ? 1 : 2;

        return Kernel(hi) + Kernel(3 - hi - lo) + Kernel(lo) + Kernel(0);
    }

    private float Kernel(int a)
    {
        float s = (_a[0] + _a[1] + _a[2]) / 6.0f;
        float x = _u - _a[0] + s;
        float y = _v - _a[1] + s;
        float z = _w - _a[2] + s;
        float t = 0.6f - x * x - y * y - z * z;
        int h = Shuffle(_i + _a[0], _j + _a[1], _k + _a[2]);
        _a[a]++;

        if (t < 0)
        {
            return 0;
        }

        int b5 = h >> 5 & 1;
        int b4 = h >> 4 & 1;
        int b3 = h >> 3 & 1;
        int b2 = h >> 2 & 1;
        int b1 = h & 3;

        float p = b1 == 1 ? x : b1 == 2 ? y : z;
        float q = b1 == 1 ? y : b1 == 2 ? z : x;
        float r = b1 == 1 ? z : b1 == 2 ? x : y;

        p = b5 == b3 ? -p : p;
        q = b5 == b4 ? -q : q;
        r = b5 != (b4 ^ b3) ? -r : r;
        t *= t;

        return 8 * t * t * (p + (b1 == 0 ? q + r : b2 == 0 ? q : r));
    }

    private int Shuffle(int i, int j, int k)
    {
        return BCoord(i, j, k, 0) + BCoord(j, k, i, 1) + 
            BCoord(k, i, j, 2) + BCoord(i, j, k, 3) + 
            BCoord(j, k, i, 4) + BCoord(k, i, j, 5) + 
            BCoord(i, j, k, 6) + BCoord(j, k, i, 7);
    }

    private int BCoord(int i, int j, int k, int index)
    {
        return _t[BIndex(i, index) << 2 | BIndex(j, index) << 1 | BIndex(k, index)];
    }

    private int BIndex(int coord, int index)
    {
        return coord >> index & 1;
    }
}