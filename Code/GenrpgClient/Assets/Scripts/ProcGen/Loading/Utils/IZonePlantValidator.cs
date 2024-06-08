using Assets.Scripts.ProcGen.RandomNumbers;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.ProcGen.Settings.Plants;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.ProcGen.Loading.Utils
{
    public interface IZonePlantValidator : IInjectable
    {
        void UpdateValidPlantTypeList<DP>(Zone zone, int gx, int gy, List<DP> fullList,
        bool isMainTerrain, CancellationToken token) where DP : BaseDetailPrototype, new();
    }


    public class ZonePlantValidator : IZonePlantValidator
    {
        private IGameData _gameData;
        private IMapProvider _mapProvider;
        private IUnityGameState _gs;
        protected IClientRandom _rand;
        public void UpdateValidPlantTypeList<DP>(Zone zone, int gx, int gy, List<DP> fullList,
        bool isMainTerrain, CancellationToken token) where DP : BaseDetailPrototype, new()
        {
            if (fullList.Count >= 2 * MapConstants.MaxGrass)
            {
                return;
            }

            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId);

            List<ZonePlantType> plist = new List<ZonePlantType>(zone.PlantTypes);

            int maxQuantity = isMainTerrain ? MapConstants.MaxGrass : MapConstants.OverrideMaxGrass;

            while (plist.Count > maxQuantity)
            {
                plist.RemoveAt(_rand.Next() % plist.Count);
            }

            for (int p = 0; p < plist.Count; p++)
            {
                ZonePlantType zpt = plist[p];

                DP currProto = fullList.FirstOrDefault(x => x.zonePlant.PlantTypeId == zpt.PlantTypeId);

                if (currProto != null)
                {
                    currProto.zoneIds.Add(zone.IdKey);
                    continue;
                }

                PlantType pt = _gameData.Get<PlantTypeSettings>(_gs.ch).Get(zpt.PlantTypeId);
                if (pt == null || string.IsNullOrEmpty(pt.Art))
                {
                    continue;
                }

                DP full = new DP();
                full.zonePlant = zpt;
                full.plantType = pt;
                full.Index = fullList.Count;
                full.XGrid = gx;
                full.YGrid = gy;
                full.noiseSeed = zone.Seed % 12783428 + _mapProvider.GetMap().Seed % 543333 + p * 13231;
                full.zoneIds.Add(zone.IdKey);
                fullList.Add(full);
            }
        }
    }
}
