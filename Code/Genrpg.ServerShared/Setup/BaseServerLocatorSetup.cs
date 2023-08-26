using Genrpg.ServerShared.CloudMessaging;
using Genrpg.ServerShared.CloudMessaging.Services;
using Genrpg.ServerShared.GameDatas.Services;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Setup
{
    public class BaseServerLocatorSetup : LocatorSetup
    {
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
            gs.loc.Set<ICloudMessageService>(new CloudMessageService());
            gs.loc.Set<IGameDataService>(new GameDataService<GameData>());
            gs.loc.Set<IMapSpawnService>(new MapSpawnService());
            gs.loc.Set<IMapDataService>(new MapDataService());
        }
    }
}
