﻿using Genrpg.InstanceServer.Managers;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public abstract class BaseInstanceMessageHandler<T> : IInstanceMessageHandler where T : IInstanceQueueMessage
    {

        protected IInstanceManagerService _instanceManagerService = null;

        protected ILogService _logService = null;

        protected abstract Task InnerHandleMessage(T message);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}
