
public class AnimUtils 
{
    public const string RenderObjectName = "RenderObject";

    public static void SetInteger (GAnimator anim, string paramName, int value, float speed=0)
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
    public static void SetBool(GAnimator anim, string paramName, bool value, float speed=0)
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
    public static void SetFloat(GAnimator anim, string paramName, float value, float speed=0)
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
    public static void Trigger (GAnimator anim, string paramName, float speed=0)
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

    public static void ResetTrigger(GAnimator anim, string paramName, float speed=0)
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

