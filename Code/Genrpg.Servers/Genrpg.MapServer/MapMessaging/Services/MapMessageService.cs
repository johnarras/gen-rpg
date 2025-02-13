﻿
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.Runtime.InteropServices;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Maps;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using System.Reflection;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.MapMessages;
using Genrpg.MapServer.Maps.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.Entities;
using Genrpg.MapServer.Spells.Services;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.HelperClasses;

namespace Genrpg.MapServer.MapMessaging.Services
{
    public class MapMessageService : IMapMessageService
    {
        const int DefaultMessageQueueCount = 37;
        public const int CompleteTaskPause = 100; // Must be > 0

        private SetupDictionaryContainer<Type, IMapMessageHandler> _eventHandlerDict = new SetupDictionaryContainer<Type, IMapMessageHandler>();
        private ILogService _logService = null;

        private List<MapMessageQueue> _queues = new List<MapMessageQueue>();

        private IMapObjectManager _objectManager = null;
        private IAIService _aiService = null;
        private IMapProvider _mapProvider =null;
        private IPathfindingService _pathfindingService = null;

        private DateTime _startTime = DateTime.UtcNow;

        private int _messageQueueCount = DefaultMessageQueueCount;

        private CancellationToken _token;

        public async Task Initialize(CancellationToken token)
        {
            Type messageInterfaceType = typeof(IMapApiMessage);

            List<Type> messageTypes = ReflectionUtils.GetTypesImplementing(messageInterfaceType);

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

            await Task.CompletedTask;

        }

        private Task _countTask = null;
        public virtual void Init(CancellationToken token)
        {
            _token = token;
            _startTime = DateTime.UtcNow;

            _messageQueueCount = Math.Max(2, (int)(_mapProvider.GetMap().BlockCount * 0.90));

            if (MapInstanceConstants.ServerTestMode)
            {
                _messageQueueCount = 1;
            }

            _queues = new List<MapMessageQueue>();
            for (int q = 0; q < _messageQueueCount; q++)
            {
                _queues.Add(new MapMessageQueue(_startTime, q, _logService, this, _token));
            }

            _countTask = Task.Run(() => ShowMessageCounts(token));
        }

        public void UpdateGameData(IGameData gameData)
        {
            foreach (MapMessageQueue queue in _queues)
            {
                queue.UpdateGameData(gameData);
            }
        }

        private async Task ShowMessageCounts(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(20000, token).ConfigureAwait(false);
                    ServerMessageCounts counts = GetCounts();

                    long aiUpdPerSec = _aiService.GetUpdateTimes() / counts.Seconds;
                    long castPerSec = _aiService.GetCastTimes() / counts.Seconds;
                    long pfPerSec = counts.PathfindingCount / counts.Seconds;


                    _logService.Message("M: " + _mapProvider.GetMap().Id + " Min/Max " + counts.MinMessages + "/" + counts.MaxMessages
                        + " Tot: " + counts.TotalMessages + " PerSec: " + counts.MessagesPerSecond

                         + " AI/Sec: " + aiUpdPerSec + " Cast/Sec: " + castPerSec
                          + " PF/Sec: " + pfPerSec 
                        );

                    if (MapInstanceConstants.ServerTestMode)
                    {
                        MapObjectCounts mapCounts = _objectManager.GetCounts();

                        _logService.Message("M: " + _mapProvider.GetMap().Id + "MapObjs IdDict: " + mapCounts.IdLookupObjectAccount + " Grid: " + mapCounts.GridObjectCount + " Char: " +
                            mapCounts.ZoneObjectCount);

                    }
                }
            }
            catch
            {
                _logService.Info("Stopped info loop");
            }
        }

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
            messageCounts.TotalSpells = _aiService.GetCastTimes();
            messageCounts.TotalUpdates = _aiService.GetUpdateTimes();
            messageCounts.PathfindingCount = _pathfindingService.GetPathSearchCount();
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
                MapMessageQueue queue = _queues[mapObject.GetIdHash() % _queues.Count];

                queue.AddMessage(message, mapObject, handler, delaySeconds);
            }
            else
            {
                Trace.WriteLine("Missing event handler for event: " + ttype.Name);
            }
        }

        public void SendMessageNear(MapObject obj, IMapMessage message,
            float distance = MessageConstants.DefaultGridDistance,
            bool playersOnly = false,
            float delaySec = 0, List<long> filters = null)
        {
            // Copy this from IMapObjectManager.GetObjectsNear() to avoid making a lot of lists.
            int gxmin = _objectManager.GetGridIndexFromCoord(obj.X - distance, false);
            int gxmax = _objectManager.GetGridIndexFromCoord(obj.X + distance, true);
            int gzmin = _objectManager.GetGridIndexFromCoord(obj.Z - distance, false);
            int gzmax = _objectManager.GetGridIndexFromCoord(obj.Z + distance, true);

            for (int x = gxmin; x <= gxmax; x++)
            {
                for (int z = gzmin; z <= gzmax; z++)
                {
                    IReadOnlyList<MapObject> list = _objectManager.GetObjectsFromGridCell(x, z);

                    foreach (MapObject obj2 in list)
                    {
                        if (!playersOnly || obj2 is Character ch)
                        {
                            SendMessage(obj2, message, delaySec);
                        }
                    }
                }
            }
        }

        public void SendMessageToAllPlayers(IMapApiMessage message)
        {
            List<Character> allChars = _objectManager.GetAllCharacters();

            foreach (Character character in allChars)
            {
                character.AddMessage(message);
            }
        }
    }
}
