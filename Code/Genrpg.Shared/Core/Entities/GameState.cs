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

namespace Genrpg.Shared.Core.Entities
{
    // Encapsulate all parameters necessary to call a command.

    [MessagePackObject]
    public class GameState
    {
        // Shared data
        public GameData data;
        public IServiceLocator loc;
        public Map map = null;
        public MapSpawnData spawns;
        public bool[,] pathfinding;
        public IRandom rand;


        public GameState()
        {
            rand = new MyRandom((int)(DateTime.UtcNow.Ticks % 1000000000));
        }

        public virtual void StartCoroutine(IEnumerator enumer)
        {
        }

        protected virtual GameState CreateGameStateInstance(ILogService logService = null)
        {
           return (GameState)Activator.CreateInstance(GetType());
        }

        public virtual GameState CreateGameStateCopy()
        {
            GameState gsNew = CreateGameStateInstance(loc.Get<ILogService>());
            gsNew.data = data;
            gsNew.loc = loc;
            gsNew.map = map;
            gsNew.spawns = spawns;
            gsNew.pathfinding = pathfinding;
            gsNew.rand = new MyRandom(DateTime.UtcNow.Ticks);
            return gsNew;
        }

    }
}
