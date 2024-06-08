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

    
    public interface IGameState
    {
        IServiceLocator loc { get; set; }
    }

    [MessagePackObject]
    public class GameState : IGameState
    {
        // Shared data
        public IServiceLocator loc { get; set; }
        /// <summary>
        /// This is here to make sure that within any specific thread/task the random number generator is not shared
        /// where it might erroring out. And there's got to be overhead with Random.Shared [ThreadStatic] so it's easier to 
        /// explicitly manage this per thread/task. With 100 tasks, Shared.Random is about half as fast. Idk if that matters but
        /// I can also set seeds for each thread too.
        /// </summary>
       

        public GameState()
        {
        }

    }
}
