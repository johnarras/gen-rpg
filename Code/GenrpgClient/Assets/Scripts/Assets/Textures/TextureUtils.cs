using System;
using UnityEngine;
using Genrpg.Shared.Core.Entities;

using Services;
using Entities;
using Genrpg.Shared.Utils.Data;

public class TextureUtils
{

    public static void SetupTexture(Texture tex)
    {
        if (tex == null)
        {
            return;
        }

        tex.mipMapBias = -0.5f;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 0;
    }


    public static Color ConvertMyColorToColor(MyColorF mc)
    {
        if (mc == null)
        {
            return Color.white;
        }

        return new Color(mc.R, mc.G, mc.B, mc.A);
    }


    public static void MoveCurrToTargetColor(MyColorF curr, MyColorF target, float stepSize)
    {
        if (curr == null || target == null || stepSize == 0)
        {
            return;
        }

        curr.R = MoveCurrFloatToTarget(curr.R, target.R, stepSize);
        curr.G = MoveCurrFloatToTarget(curr.G, target.G, stepSize);
        curr.B = MoveCurrFloatToTarget(curr.B, target.B, stepSize);
        curr.A = MoveCurrFloatToTarget(curr.A, target.A, stepSize);
    }

    public static Color MoveCurrToTargetColor(Color curr, Color target, float stepSize)
    {
        if (stepSize == 0)
        {
            return curr;
        }

        return new Color(
            MoveCurrFloatToTarget(curr.r, target.r, stepSize),
            MoveCurrFloatToTarget(curr.g, target.g, stepSize),
            MoveCurrFloatToTarget(curr.b, target.b, stepSize),
            MoveCurrFloatToTarget(curr.a, target.a, stepSize));
    }


    public static float MoveCurrFloatToTarget (float curr, float target, float step)
    {
        if (step < 0)
        {
            return target;
        }

        if (Math.Abs(curr - target) < 0.00001f)
        {
            return target;
        }

        if (curr > target)
        {
            curr -= step;
            if (curr < target)
            {
                curr = target;
            }
        }

        if (curr < target)
        {
            curr += step;
            if (curr > target)
            {
                curr = target;
            }
        }

        return curr;
    }




}

