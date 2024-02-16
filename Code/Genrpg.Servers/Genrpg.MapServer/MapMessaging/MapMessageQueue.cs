using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logs.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Genrpg.MapServer.MapMessaging
{
    public class MapMessageQueue
    {
        const int DelayedMessageBufferSize = 10000;

        protected ConcurrentQueue<MapMessagePackage> _currentQueue = new ConcurrentQueue<MapMessagePackage>();
        protected ConcurrentQueue<MapMessagePackage> _delayedQueue = new ConcurrentQueue<MapMessagePackage>();
        protected List<MapMessagePackage>[] _delayedMessages = null;
        protected int _tick = 0;
        protected int _queueIndex = -1;
        protected long _messagesProcessed = 0;
        protected CancellationToken _token;
        private ILogSystem _logger;
        private IMapMessageService _mapMessageService;

        private DateTime _startTime = DateTime.UtcNow;

        public MapMessageQueue(GameState gs, DateTime startTime, int queueIndex, IMapMessageService mapMessageService, CancellationToken token)
        {
            _logger = gs.logger;
            _token = token;
            _startTime = startTime;
            _queueIndex = queueIndex;
            _mapMessageService = mapMessageService;
            _delayedMessages = new List<MapMessagePackage>[DelayedMessageBufferSize];
            for (int d = 0; d < _delayedMessages.Length; d++)
            {
                _delayedMessages[d] = new List<MapMessagePackage>();
            }

            _ = Task.Run(() => ProcessDelayQueue(gs), _token);

            _ = Task.Run(() => ProcessQueue(gs), _token);
        }

        public long GetMessagesProcessed()
        {
            return _messagesProcessed;
        }

        public void UpdateGameData(GameData gameData)
        {
            _pgs.data = gameData;
            _dgs.data = gameData;
            _dgs.logger.Message("Update Message Queue Game Data!");
        }

        public void AddMessage(IMapMessage message, MapObject mapObject, IMapMessageHandler handler, float delaySeconds)
        {
            MapMessagePackage package = _mapMessageService.GetPackage();

            package.message = message;
            package.mapObject = mapObject;
            package.handler = handler;
            package.delaySeconds = delaySeconds;
            
            if (package.delaySeconds <= 0)
            {
                package.message.SetLastExecuteTime(DateTime.UtcNow);
                _currentQueue.Enqueue(package);
            }
            else
            {
                _delayedQueue.Enqueue(package);
            }
        }

        private GameState _dgs = null;
        protected async Task ProcessDelayQueue(GameState gsIn)
        {
            try
            {
                _dgs = gsIn.CopySharedData();
                int currentTick = 0;

                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(MessageConstants.DelayedMessageTimeGranularity)))
                {
                    await Task.Delay(1);

                    while (true)
                    {
                        DateTime tickStartTime = DateTime.UtcNow;
                        while (_delayedQueue.TryDequeue(out MapMessagePackage item))
                        {
                            DateTime nextExecuteTime = item.message.GetLastExecuteTime().AddSeconds(item.delaySeconds);
                            double messageTimeDiff = Math.Max(0, (nextExecuteTime - DateTime.UtcNow).TotalSeconds);
                            int messageTimeTicks = (int)(messageTimeDiff /= MessageConstants.DelayedMessageTimeGranularity);
                            int offset = MathUtils.Clamp(1, messageTimeTicks, DelayedMessageBufferSize - 1);
                            int index = (currentTick + offset) % DelayedMessageBufferSize;
                            item.message.SetLastExecuteTime(nextExecuteTime);
                            _delayedMessages[index].Add(item);
                        }

                        // Find the new tick based on the time elapsed from start of game.
                        int newTick = (int)((DateTime.UtcNow - _startTime).TotalSeconds / MessageConstants.DelayedMessageTimeGranularity);

                        for (int i = currentTick + 1; i <= newTick; i++)
                        {
                            // For each tick in between currentTick and newTick, pull all messages into the main queue.
                            int idx = i % DelayedMessageBufferSize;
                            List<MapMessagePackage> newMessages = _delayedMessages[idx];
                            _delayedMessages[idx] = new List<MapMessagePackage>();
                            foreach (MapMessagePackage package in newMessages)
                            {
                                _currentQueue.Enqueue(package);
                            }
                        }
                        // Update the current tick as far as we need to go.
                        currentTick = newTick;

                        await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                _logger.Info("Map Instance Shutdown MessageQueue.ProcessDelayed Index: " + _queueIndex);
            }
            catch (Exception e)
            { 
                _logger.Exception(e, "MessageQueueDelay");
            }
        }

        private GameState _pgs = null;
        protected async Task ProcessQueue(GameState gsIn)
        {
            try
            {
                _pgs = gsIn.CopySharedData();
                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1)))
                {
                    while (true)
                    {
                        while (_currentQueue.TryDequeue(out MapMessagePackage package))
                        {
                            try
                            {
                                // This is intentionally synchronous
                                package.Process(_pgs);
                                _messagesProcessed++;
                                _mapMessageService.AddPackage(package);
                            }
                            catch (Exception e)
                            {
                                _logger.Exception(e, "Process Message");
                            }
                        }
                        await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                _logger.Info("Map Instance Shutdown MessageQueue.Process Index: " + _queueIndex);
            }
            catch (Exception e)
            {
                _logger.Exception(e, "MessageQueueProcess");
            }
        }
    }
}
