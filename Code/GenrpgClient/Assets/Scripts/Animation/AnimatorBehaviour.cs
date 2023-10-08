using System;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;
using System.Threading;

public class AnimatorBehaviour : BaseBehaviour
{
    public GAnimator MainAnimator;

    public void SetAnimInt(string intName, int value, float delay, Action<CancellationToken> action, CancellationToken token)
    {
        if (MainAnimator != null)
        {
            MainAnimator.SetInteger(intName, value);
        }

        StartAnimDelay(delay, action, token);
    }

    public void SetAnimBool(string boolName, bool value, float delay, Action<CancellationToken> action, CancellationToken token)
    {
        if (MainAnimator != null)
        {
            MainAnimator.SetBool(boolName, value);
        }

        StartAnimDelay(delay, action, token);
    }

    public void TriggerAnimation(string triggerName, float delay, Action<CancellationToken> action, CancellationToken token)
    {
        if (MainAnimator != null)
        {
            MainAnimator.SetTrigger(triggerName);
        }

        StartAnimDelay(delay, action, token);
    }

    public void SetAnimFloat(string floatName, float val, float delay, Action<CancellationToken> action, CancellationToken token)
    {
        if (MainAnimator != null)
        {
            MainAnimator.SetFloat(floatName, val);
        }

        StartAnimDelay(delay, action, token);
    }

    private void StartAnimDelay(float delay, Action<CancellationToken> action, CancellationToken token)
    {   
        if (action == null)
        {
            return;
        }

        if (delay <= 0)
        {
            //action(token);
            //return;
        }

        _updateService.AddDelayedUpdate(null, action, token, delay);
    }
}
