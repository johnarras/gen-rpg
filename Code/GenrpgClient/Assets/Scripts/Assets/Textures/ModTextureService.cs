using System;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using UnityEngine;


public interface IModTextureService: IInjectable
{
    void SetupTexture(Texture tex);
    Color ConvertMyColorToColor(MyColorF mc);
    void MoveCurrToTargetColor(MyColorF curr, MyColorF target, float stepSize);
    Color MoveCurrToTargetColor(Color curr, Color target, float stepSize);
    float MoveCurrFloatToTarget(float curr, float target, float step);
}

public class ModTextureService : IModTextureService
{

    public void SetupTexture(Texture tex)
    {
        if (tex == null)
        {
            return;
        }

        tex.mipMapBias = -0.5f;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 0;
    }


    public Color ConvertMyColorToColor(MyColorF mc)
    {
        if (mc == null)
        {
            return Color.white;
        }

        return new Color(mc.R, mc.G, mc.B, mc.A);
    }


    public void MoveCurrToTargetColor(MyColorF curr, MyColorF target, float stepSize)
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

    public Color MoveCurrToTargetColor(Color curr, Color target, float stepSize)
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


    public float MoveCurrFloatToTarget (float curr, float target, float step)
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

