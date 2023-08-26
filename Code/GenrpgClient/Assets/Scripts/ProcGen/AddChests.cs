using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;


using Services;

using UnityEngine;
using Cysharp.Threading.Tasks;
using Entities;
using Genrpg.Shared.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Services.ProcGen;
using System.Threading;
using Genrpg.Shared.Entities.Constants;

public class AddChests : BaseZoneGenerator
{
    public const float MaxSteepness = 25;
    public const float ChestChance = 0.1f;

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        MyRandom placeRand = new MyRandom(gs.map.Seed % 102839484);
        MyRandom choiceRand = new MyRandom(gs.map.Seed % 329377421);

        int skipSize = 40;

        List<GroundObjType> chests = gs.data.GetGameData<ProcGenSettings>().GroundObjects.Where(x => x.GroupId == GroundObjType.ChestGroup).ToList();

        if (chests == null || chests.Count < 1)
        {
            return;
        }

        int totalWeight = chests.Sum(x => x.SpawnWeight);

        if (totalWeight < 1)
        {
            return;
        }

        for (int x = MapConstants.TerrainPatchSize; x < gs.map.GetHwid() - MapConstants.TerrainPatchSize; x += skipSize)
        {
            for (int y = MapConstants.TerrainPatchSize; y < gs.map.GetHhgt() - MapConstants.TerrainPatchSize; y += skipSize)
            {
                if (FlagUtils.IsSet(gs.md.flags[x, y], MapGenFlags.BelowWater | MapGenFlags.IsLocation))
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
                            if (gs.md.mapObjects[xx, yy] != 0)
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

                    if (_zoneGenService.FindMapLocation(gs, tx, ty, 10) != null)
                    {
                        continue;
                    }


                    if (gs.md.roadDistances[cx, cy] < 30)
                    {
                        continue;
                    }


                    if (gs.md.GetSteepness(gs, cx, cy) > MaxSteepness)
                    {
                        continue;
                    }

                    gs.spawns.AddSpawn(EntityType.GroundObject, chosenObj.IdKey, cy, cx, gs.md.mapZoneIds[cx,cy]);
                
                    break;
                }
            }
        }
    }
}