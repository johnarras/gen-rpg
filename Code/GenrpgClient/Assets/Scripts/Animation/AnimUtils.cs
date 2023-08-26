using UnityEngine;
using System.Collections;

public class AnimUtils 
{
    public const string RenderObjectName = "RenderObject";

    public static void SetInteger (Animator anim, string paramName, int value, float speed=0)
    {
        if (anim == null || string.IsNullOrEmpty(paramName))
        {
            return;
        }

        if (speed > 0)
        {
            anim.speed = speed;
        }

        anim.SetInteger(paramName, value);
    }
    public static void SetBool(Animator anim, string paramName, bool value, float speed=0)
    {
        if (anim == null || string.IsNullOrEmpty(paramName))
        {
            return;
        }

        if (speed > 0)
        {
            anim.speed = speed;
        }

        anim.SetBool(paramName, value);
    }
    public static void SetFloat(Animator anim, string paramName, float value, float speed=0)
    {
        if (anim == null || string.IsNullOrEmpty(paramName))
        {
            return;
        }

        if (speed > 0)
        {
            anim.speed = speed;
        }

        anim.SetFloat(paramName, value);
    }
    public static void Trigger (Animator anim, string paramName, float speed=0)
    {
        if (anim == null || string.IsNullOrEmpty(paramName))
        {
            return;
        }

        if (speed > 0)
        {
            anim.speed = speed;
        }

        anim.SetTrigger(paramName);
    }

    public static void ResetTrigger(Animator anim, string paramName, float speed=0)
    {
        if (anim == null || string.IsNullOrEmpty(paramName))
        {
            return;
        }

        if (speed > 0)
        {
            anim.speed = speed;
        }

        anim.ResetTrigger(paramName);
    }

}

