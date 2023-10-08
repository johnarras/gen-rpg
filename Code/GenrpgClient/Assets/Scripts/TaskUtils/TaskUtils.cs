using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TaskUtils
{
    public static void AddTask(Task task)
    {
        if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !task.IsCanceled)
        {
            Task.Run(() => task);
        }
    }
}