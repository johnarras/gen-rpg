using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Logs.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared
{
    public class DbQueue
    {
        private ConcurrentQueue<IDbAction> _queue = new ConcurrentQueue<IDbAction>();
        private ILogSystem _logger;
        public DbQueue(ILogSystem logger)
        {
            _logger = logger;
            _ = Task.Run(() => ActionLoop());
        }

        protected async Task ActionLoop()
        {
            while (true)
            {
                try
                {
                    while (_queue.TryDequeue(out IDbAction item))
                    {
                        await item.Execute().ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "DbActionLoop");
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
        }

        public void Enqueue(IDbAction action)
        {
            _queue.Enqueue(action);
        }
    }
}
