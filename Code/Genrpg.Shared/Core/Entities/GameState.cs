using MessagePack;
using System;
using System.Collections;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Analytics.Services;
using System.Threading;

namespace Genrpg.Shared.Core.Entities
{
    // Encapsulate all parameters necessary to call a command.

    [MessagePackObject]
    public class GameState
    {
        // Shared data
        public IServiceLocator loc;
        public Map map = null;
        public MapSpawnData spawns;
        public bool[,] pathfinding;
        /// <summary>
        /// This is here to make sure that within any specific thread/task the random number generator is not shared
        /// where it might erroring out. And there's got to be overhead with Random.Shared [ThreadStatic] so it's easier to 
        /// explicitly manage this per thread/task. With 100 tasks, Shared.Random is about half as fast. Idk if that matters but
        /// I can also set seeds for each thread too.
        /// </summary>
        public IRandom rand;


        public GameState()
        {
            rand = new MyRandom((int)(DateTime.UtcNow.Ticks % 1000000000));
        }

        protected virtual GameState CreateGameStateInstance(ILogService logService = null, IAnalyticsService analyicsService = null)
        {
           return (GameState)Activator.CreateInstance(GetType());
        }

        public virtual GameState CreateGameStateCopy()
        {
            GameState gsNew = CreateGameStateInstance(loc.Get<ILogService>(), loc.Get<IAnalyticsService>());        
            gsNew.loc = loc;
            gsNew.map = map;
            gsNew.spawns = spawns;
            gsNew.pathfinding = pathfinding;
            gsNew.rand = new MyRandom(DateTime.UtcNow.Ticks);
            return gsNew;
        }

        public virtual T CreateInstance<T>() where T : class, IInjectable, new()
        {
            T t = new T();
            loc.Resolve(t);
            return t;
        }
    }
}
