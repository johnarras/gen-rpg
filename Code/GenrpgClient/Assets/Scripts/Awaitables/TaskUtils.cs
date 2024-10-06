using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskUtils
{
    public static void ForgetAwaitable(Awaitable aw)
    {
        _ = Task.Run(() => aw);
    }

    public static void ForgetAwaitable<T>(Awaitable<T> aw)
    {
        _ = Task.Run(() => aw);
    }

    public static void ForgetTask(Task t)
    {
        _ = Task.Run(() => t);
    }

    public static void ForgetTask<T>(Task<T> t)
    {
        _ = Task.Run(() => t);
    }
}
