using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TaskUtils
{

    private static Dictionary<string, int> _taskCount = new Dictionary<string, int>();

    public static void AddTask(Task task, string loc, CancellationToken token)
    {

        if (!_taskCount.ContainsKey(loc))
        {
            _taskCount[loc] = 0;
        }
        _taskCount[loc]++;

        if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !task.IsCanceled)
        {
            Task.Factory.StartNew(() => { task.Start(); }, token);
        }
    }
}