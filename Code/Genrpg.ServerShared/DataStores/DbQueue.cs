using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Logs.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores
{
    public class DbQueue
    {
        private ConcurrentQueue<IDbAction> _queue = new ConcurrentQueue<IDbAction>();
        public DbQueue(ILogSystem logger, CancellationToken token)
        {
            _ = Task.Run(() => ActionLoop(logger,token));
        }

        protected async Task ActionLoop(ILogSystem logger, CancellationToken token) 
        {
            using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1)))
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
                        logger.Exception(e, "DbActionLoop");
                    }
                    await timer.WaitForNextTickAsync(token).ConfigureAwait(false);
                }
            }
        }

        public void Enqueue(IDbAction action)
        {
            _queue.Enqueue(action);
        }
    }
}
