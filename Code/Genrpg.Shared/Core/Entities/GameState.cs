using MessagePack;
using System;
using System.Collections;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logs.Interfaces;
using Genrpg.Shared.Spawns.WorldData;

namespace Genrpg.Shared.Core.Entities
{
    // Encapsulate all parameters necessary to call a command.

    [MessagePackObject]
    public class GameState
    {
        // Shared data
        public GameData data;
        public IServiceLocator loc;
        public IRepositorySystem repo;
        public ILogSystem logger;
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

        public virtual GameState CopySharedData()
        {
            GameState gsNew = (GameState)Activator.CreateInstance(GetType());

            gsNew.data = data;
            gsNew.loc = loc;
            gsNew.repo = repo;
            gsNew.map = map;
            gsNew.spawns = spawns;
            gsNew.pathfinding = pathfinding;
            gsNew.logger = logger;
            gsNew.rand = new MyRandom(DateTime.UtcNow.Ticks);
            return gsNew;
        }

    }
}
