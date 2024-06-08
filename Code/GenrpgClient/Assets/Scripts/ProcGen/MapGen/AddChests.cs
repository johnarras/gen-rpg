using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.GroundObjects.Settings;

public class AddChests : BaseZoneGenerator
{
    public const float MaxSteepness = 25;
    public const float ChestChance = 0.1f;

    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);

        MyRandom placeRand = new MyRandom(_mapProvider.GetMap().Seed % 102839484);
        MyRandom choiceRand = new MyRandom(_mapProvider.GetMap().Seed % 329377421);

        int skipSize = 40;

        List<GroundObjType> chests = _gameData.Get<GroundObjTypeSettings>(_gs.ch).GetData().Where(x => x.GroupId == GroundObjType.ChestGroup).ToList();

        if (chests == null || chests.Count < 1)
        {
            return;
        }

        int totalWeight = chests.Sum(x => x.SpawnWeight);

        if (totalWeight < 1)
        {
            return;
        }

        for (int x = MapConstants.TerrainPatchSize; x < _mapProvider.GetMap().GetHwid() - MapConstants.TerrainPatchSize; x += skipSize)
        {
            for (int y = MapConstants.TerrainPatchSize; y < _mapProvider.GetMap().GetHhgt() - MapConstants.TerrainPatchSize; y += skipSize)
            {
                if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.BelowWater | MapGenFlags.IsLocation))
                {
                    continue;
                }
                if (placeRand.NextDouble() > ChestChance)
                {
                    continue;
                }

                GroundObjType chosenObj = null;

                int chestChoice = choiceRand.Next() % totalWeight;

                foreach (GroundObjType chest in chests)
                {
                    chestChoice -= chest.SpawnWeight;
                    if (chestChoice <= 0)
                    {
                        chosenObj = chest;
                        break;
                    }
                }

                if (chosenObj == null)
                {
                    continue;
                }

                int nearbyRadius = 3;

                for (int times = 0; times < 20; times++)
                {

                    int cx = x + MathUtils.IntRange(-skipSize / 3, skipSize / 3, placeRand);
                    int cy = y + MathUtils.IntRange(-skipSize / 3, skipSize / 3, placeRand);

                    bool haveNearbyItem = false;


                    for (int xx = cx - nearbyRadius; xx <= cx + nearbyRadius; xx++)
                    {
                        for (int yy = cy - nearbyRadius; yy <= cy + nearbyRadius; yy++)
                        {
                            if (_md.mapObjects[xx, yy] != 0)
                            {
                                haveNearbyItem = true;
                                break;
                            }
                        }
                        if (haveNearbyItem)
                        {
                            break;
                        }
                    }

                    if (haveNearbyItem)
                    {
                        continue;
                    }


                    int tx = cx - cx / (MapConstants.TerrainPatchSize - 1);
                    int ty = cy - cy / (MapConstants.TerrainPatchSize - 1);

                    if (_zoneGenService.FindMapLocation(tx, ty, 10) != null)
                    {
                        continue;
                    }


                    if (_md.roadDistances[cx, cy] < 30)
                    {
                        continue;
                    }


                    if (_terrainManager.GetSteepness(cx, cy) > MaxSteepness)
                    {
                        continue;
                    }

                    InitSpawnData initData = new InitSpawnData()
                    {
                        EntityTypeId = EntityTypes.GroundObject,
                        EntityId = chosenObj.IdKey,
                        SpawnX = cy,
                        SpawnZ = cx,
                        ZoneId = _md.mapZoneIds[cx, cy],
                        ZoneOverridePercent = (int)(_md.overrideZoneScales[cx, cy] * MapConstants.OverrideZoneScaleMax),
                    };
                    

                    _mapProvider.GetSpawns().AddSpawn (initData);
                
                    break;
                }
            }
        }
    }
}