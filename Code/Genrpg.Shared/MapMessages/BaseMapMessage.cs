using MessagePack;
using Genrpg.Shared.MapMessages.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMessages
{
    [MessagePackObject]
    public class BaseMapMessage : IMapMessage
    {
        private DateTime _lastExecuteTime { get; set; } = DateTime.UtcNow;
        private bool _isCancelled = false;
        public virtual bool IsCancelled()
        {
            return _isCancelled;
        }

        public DateTime GetLastExecuteTime()
        {
            return _lastExecuteTime;
        }

        public void SetLastExecuteTime(DateTime dateTime)
        {
            _lastExecuteTime = dateTime;
        }

        public void SetCancelled(bool val)
        {
            _isCancelled = val;
        }

        public BaseMapMessage()
        {
            _lastExecuteTime = DateTime.UtcNow;
        }
    }
}
