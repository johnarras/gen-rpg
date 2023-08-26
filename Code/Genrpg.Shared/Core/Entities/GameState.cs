using MessagePack;
using System;
using System.Collections;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.Logs.Entities;

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


        /// <summary>
        /// General random number generator.
        /// 
        /// Do not use this for procgen. Only use for combat randomness and such.
        /// 
        /// </summary>
        private MyRandom _rand = new MyRandom((int)DateTime.UtcNow.Ticks);
        public MyRandom rand
        {
            get
            {
                return _rand;
            }
            set
            {
                if (value != null)
                {
                    _rand = value;
                }
            }
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
            gsNew._rand = new MyRandom(DateTime.UtcNow.Ticks);
            return gsNew;
        }

    }
}
