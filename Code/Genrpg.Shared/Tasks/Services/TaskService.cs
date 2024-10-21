using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Tasks.Services
{
    public interface ITaskService : IInjectable
    {
        void ForgetTask(Task t);
        void ForgetTask<A>(Task<A> t);
    }

    public class TaskService : ITaskService
    {
        public void ForgetTask(Task t)
        {
            _ = Task.Run(() => t);
        }

        public void ForgetTask<A>(Task<A> t)
        {
            _ = Task.Run(() => t);
        }
    }
}
