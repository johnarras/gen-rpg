using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMessages.Interfaces
{
    public interface IMapMessage
    {
        DateTime GetLastExecuteTime();
        void SetLastExecuteTime(DateTime lastExecuteTime);
        bool IsCancelled();
        void SetCancelled(bool val);
    }
}
