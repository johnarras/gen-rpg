
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using System.Runtime.InteropServices;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Maps;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using System.Reflection;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.MapMessages;

namespace Genrpg.MapServer.MapMessaging
{
    public class MapMessageService : IMapMessageService
    {
        const int DefaultMessageQueueCount = 37;
        public const int CompleteTaskPause = 100; // Must be > 0

        private Dictionary<Type, IMapMessageHandler> _eventHandlerDict = null;

        private List<MapMessageQueue> _queues = new List<MapMessageQueue>();

        private IMapObjectManager _objectManager = null;
        private IAIService _aiService = null;
        private IReflectionService _reflectionService;
        private DateTime _startTime = DateTime.UtcNow;

        private int _messageQueueCount = DefaultMessageQueueCount;

        private CancellationToken _token;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            _eventHandlerDict = _reflectionService.SetupDictionary<Type, IMapMessageHandler>(gs);

            foreach (IMapMessageHandler handler in _eventHandlerDict.Values)
            {
                handler.Setup(gs);
            }
            await Task.CompletedTask;

            Type messageInterfaceType = typeof(IMapApiMessage);

            List<Type> messageTypes = _reflectionService.GetTypesImplementing(messageInterfaceType);

            StringBuilder sb = new StringBuilder();
            foreach (Type t in messageTypes)
            {
                if (!t.IsClass || t.IsAbstract || t.IsSealed || t == typeof(BaseMapMessage))
                {
                    continue;
                }
                sb.Append(t.Name + " ");
            }

            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                throw new Exception("Found nonsealed IMapApiMessage implementation: " + sb.ToString());
            }

        }

        private Task _countTask = null;
        public virtual void Init(GameState gs, CancellationToken token)
        {
            _token = token;
            _startTime = DateTime.UtcNow;

            _messageQueueCount = (int)(gs.map.BlockCount * 0.39);
            _queues = new List<MapMessageQueue>();
            for (int q = 0; q < _messageQueueCount; q++)
            {
                _queues.Add(new MapMessageQueue(gs, _startTime, q, _token));
            }

            _countTask = Task.Run(() => ShowMessageCounts(gs));
        }

        //#if DEBUG
        private async Task ShowMessageCounts(GameState gs)
        {
            while (true)
            {
                await Task.Delay(20000).ConfigureAwait(false);
                ServerMessageCounts counts = GetCounts();
#if DEBUG
                long aiUpdPerSec = _aiService.GetUpdateTimes() / counts.Seconds;
                long castPerSec = _aiService.GetCastTimes() / counts.Seconds;
#endif

                gs.logger.Message("M: " + gs.map.Id + " Min/Max " + counts.MinMessages + "/" + counts.MaxMessages
                    + " Total: " + counts.TotalMessages + " PerSec: " + counts.MessagesPerSecond
#if DEBUG
                     +" AI/Sec: " + aiUpdPerSec + " Cast/Sec: " + castPerSec
#endif
                    );
            }
        }
        //#endif

        public ServerMessageCounts GetCounts()
        {
            List<long> counts = new List<long>();

            foreach (MapMessageQueue queue in _queues)
            {
                counts.Add(queue.GetMessagesProcessed());
            }

            ServerMessageCounts messageCounts = new ServerMessageCounts();

            if (counts.Count < 1)
            {
                return messageCounts;
            }

            messageCounts.QueueCount = counts.Count;
            messageCounts.MinMessages = counts.Min();
            messageCounts.MaxMessages = counts.Max();
            messageCounts.TotalMessages = counts.Sum();
            messageCounts.Seconds = (long)Math.Max(1, (DateTime.UtcNow - _startTime).TotalSeconds);
            messageCounts.MessagesPerSecond = messageCounts.TotalMessages / messageCounts.Seconds;

            return messageCounts;
        }

        public virtual void SendMessage(MapObject mapObject, IMapMessage message, float delaySeconds = 0)
        {
            if (mapObject == null || _queues.Count < 1 || _objectManager == null)
            {
                return;
            }

            Type ttype = message.GetType();

            if (_eventHandlerDict.TryGetValue(ttype, out IMapMessageHandler handler))
            {
                MapMessageQueue queue = _queues[StrUtils.GetIdHash(mapObject.Id) % _queues.Count];

                queue.AddMessage(message, mapObject, handler, delaySeconds);
            }
            else
            {
                Trace.WriteLine("Missing event handler for event: " + ttype.Name);
            }
        }

        public void SendMessageNear(MapObject obj, IMapMessage message,
            float dist = MessageConstants.DefaultGridDistance,
            bool playersOnly = false,
            float delaySec = 0, List<long> filters = null, bool checkDistinct = false)
        {
            if (_objectManager == null)
            {
                return;
            }

            List<MapObject> nearbyObjects = _objectManager.GetObjectsNear(obj.X, obj.Z, obj, dist, false, filters);

            if (checkDistinct)
            {
                nearbyObjects = nearbyObjects.Distinct().ToList();
            }

            if (playersOnly)
            {
                nearbyObjects = nearbyObjects.Where(x => x is Character ch).ToList();
            }

            foreach (MapObject nearbyObj in nearbyObjects)
            {
                SendMessage(nearbyObj, message, delaySec);
            }
        }
    }
}
