using System;
using System.Collections.Generic;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

/// <summary>
/// Add items nearby a tree or a rock or something of that sort.
/// </summary>
public class AddNearbyItemsHelper
{

    public void AddItemsNear(UnityGameState gs, IMapTerrainManager terrainManager, MyRandom rand, ZoneType zoneType, Zone zone, int x, int y, double placeChance, int maxPlaceQuantity, float minOffset, float maxOffset, bool canPlaceTrees = true)
    {

        float posHeight = terrainManager.GetInterpolatedHeight(gs, y, x);

        if (posHeight < MapConstants.OceanHeight)
        {
            return;       
        }

        if (rand.NextDouble() > placeChance)
        {
            return;
        }

        int maxNumPlants = maxPlaceQuantity;

        int bushesToAdd = MathUtils.IntRange(maxNumPlants / 2, maxNumPlants, rand);

        if (rand.NextDouble() < 0.3f)
        {
            bushesToAdd += MathUtils.IntRange(1, maxNumPlants, rand);
        }

        int treesToAdd = bushesToAdd / 10;

        if (treesToAdd == 0 && rand.Next() < 0.10f)
        {
            treesToAdd++;
        }

        if (treesToAdd > 2)
        {
            treesToAdd = 2;
        }

        GenZone genZone = gs.GetGenZone(zone.IdKey);

        if (zoneType.TreeTypes == null || genZone.TreeTypes == null)
        {
            return;
        }

        List<ZoneTreeType> bushList = new List<ZoneTreeType>();
        List<ZoneTreeType> treeList = new List<ZoneTreeType>();



        foreach (ZoneTreeType zt in zoneType.TreeTypes)
        {
            if (zt.PopulationScale <= 0)
            {
                continue;
            }
            TreeType tt = gs.data.GetGameData<TreeTypeSettings>(gs.ch).GetTreeType(zt.TreeTypeId);
            if (tt == null || tt.Name == null)
            {
                continue;
            }
            if (tt.HasFlag(TreeFlags.IsWaterItem))
            {
                continue;
            }

            if (tt.HasFlag(TreeFlags.IsBush))
            {
                bushList.Add(zt);
            }
            else
            {
                treeList.Add(zt);
            }
        }

        if (!canPlaceTrees)
        {
            treeList = new List<ZoneTreeType>();
        }

        if (minOffset < 0.99f)
        {
            minOffset = 0.99f;
        }

        float maxTreeOffset = Math.Max(2, maxOffset * 2 / 3);
        float maxBushOffset = Math.Max(2, maxOffset + 1);

        maxTreeOffset = Math.Max(minOffset + 2, maxTreeOffset);
        maxBushOffset = Math.Max(minOffset + 1, maxBushOffset);

        for (int plantTimes = 0; plantTimes < 2; plantTimes++)
        {
            int numToPlace = treesToAdd;
            double offset = maxTreeOffset;
            List<ZoneTreeType> itemList = treeList;


            if (plantTimes == 1)
            {
                numToPlace = bushesToAdd;
                offset = maxBushOffset;
                itemList = bushList;
            }

            if (itemList.Count < 1)
            {
                continue;
            }

            int numPlaced = 0;
            for (int tries = 0; tries < numToPlace * 30 && numPlaced < numToPlace; tries++)
            {
                int plantx = (int)(MathUtils.FloatRange(x - offset, x + offset, rand)+0.5f);
                int planty = (int)(MathUtils.FloatRange(y - offset, y + offset, rand) + 0.5f);




                int pdx = plantx - x;
                int pdy = planty - y;


                float dist = (float)Math.Sqrt(pdx * pdx + pdy * pdy);
                if (dist < minOffset)
                {
                    continue;
                }
                if (dist > offset)
                {
                    continue;
                }


                int ipplantx = (int)(plantx); //+ (int)(plantx / (MapConstants.TerrainPatchSize - 1));
                int ipplanty = (int)(planty); //+ (int)(planty / (MapConstants.TerrainPatchSize - 1));

                if (ipplantx < 0 || ipplantx >= gs.map.GetHwid() || ipplanty <= 0 || ipplanty >= gs.map.GetHhgt())
                {
                    continue;
                }


                if (gs.md.roadDistances[ipplantx, ipplanty] < 3)
                {
                    continue;
                }

                ZoneTreeType item = itemList[rand.Next() % itemList.Count];

                if (gs.md.mapObjects != null && gs.md.mapObjects[ipplantx, ipplanty] == 0)
                {
                    gs.md.mapObjects[ipplantx, ipplanty] = (int)(MapConstants.TreeObjectOffset + item.TreeTypeId);
                    numPlaced++;
                }

            }
        }
    }

    public int GetNearbyItemsCount (UnityGameState gs, int radius, MyRandom rand)
    {
        int nearbyItemsCount = 1;

        for (int newItemTimes = 0; newItemTimes < 3; newItemTimes++)
        {
            if (rand.Next() % 5 > newItemTimes)
            {
                nearbyItemsCount += MathUtils.IntRange(0, 2, rand);
            }
        }

        if (nearbyItemsCount < 1)
        {
            nearbyItemsCount = 1;
        }

        for (int rad = 1; rad <= radius; rad++)
        {
            nearbyItemsCount = nearbyItemsCount * 3 / 2;
        }

        nearbyItemsCount = MathUtils.IntRange(nearbyItemsCount * 3 / 4, nearbyItemsCount * 5 / 4, rand);


        return nearbyItemsCount;
    }
}
