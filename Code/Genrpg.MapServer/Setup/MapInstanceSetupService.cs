using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Spawns.Entities;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Setup
{
    public class MapInstanceSetupService : SetupService
    {
        private string _mapId;
        private IMapSpawnService _mapSpawnService;
        private IMapDataService _mapDataService;
        public MapInstanceSetupService(string mapId)
        {
            _mapId = mapId;
        }


        public override void SetupObjectLocator(GameState gs)
        {
            MapInstanceLocatorSetup ss = new MapInstanceLocatorSetup();
            ss.Setup(gs);
        }

        public override async Task FinalSetup(GameState gs)
        {
            ServerGameState sgs = gs as ServerGameState;

            gs.map = await _mapDataService.LoadMap(sgs, _mapId);
            gs.spawns = await _mapSpawnService.LoadMapSpawnData(gs, Map.GetMapOwnerId(gs.map));
            await base.FinalSetup(gs);
        }
    }
}
