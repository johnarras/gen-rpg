using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Quests.Constants;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Zones.WorldData;
using Unity.Collections;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;

public interface IQuestGenService : IInitializable
{
    void GenerateQuests(UnityGameState gs);
}

public class QuestGenService : IQuestGenService
{

    protected IMapGenService _mapGenService;
    protected IGameData _gameData;

    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }


    public void GenerateQuests(UnityGameState gs)
    {
        if ( gs.map == null || gs.map.Zones == null)
        {
            return;
        }

        foreach (Zone zone in gs.map.Zones)
        {
            GenerateZoneQuests(gs, zone);
        }
    }

    protected void GenerateZoneQuests(UnityGameState gs, Zone zone)
    {
        if ( gs.map == null || zone == null || zone.Units.Count < 1)
        {
            return;
        }

        ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(gs.ch).Get(zone.ZoneTypeId);

        Map map = gs.map;
        MyRandom rand = new MyRandom(zone.Seed / 2 + 32324 + map.Seed / 3);
        if (map.Quests == null)
        {
            map.Quests = new List<QuestType>();
        }

        List<MapSpawn> zoneNPCs = GetNPCsForZone(gs, map, zone.IdKey);

        List<long> unitTypeIds = zoneType.UnitSpawns.Select(x => x.EntityId).ToList();

        // Make standard quests first.
        foreach (MapSpawn npc in zoneNPCs)
        {
            int numQuests = MathUtils.IntRange(1, 3, rand);

            for (int q = 0; q < numQuests; q++)
            {

                if (unitTypeIds.Count < 1)
                {
                    continue;
                }

                long questId = GetNextQuestTypeId(gs);

                QuestType questType = new QuestType() { IdKey = questId };
                questType.Name = "Quest" + questId;
                questType.Desc = "Quest Desc " + questId + "\n";

                for (int i = 0; i < 3; i++)
                {
                    questType.Desc += questType.Desc;
                }
                questType.StartObjId = npc.Id;
                questType.EndObjId = npc.Id;
                questType.MinLevel = Math.Max(1, zone.Level - QuestConstants.QuestLevelBelowZoneLevel);
                questType.ZoneId = zone.IdKey;
                questType.MapId = gs.map.Id;
                questType.MapVersion = gs.map.MapVersion;

                int numTasks = (int)MathUtils.FloatRange(1, 2.2f, rand);

                for (int t = 0; t < numTasks; t++)
                {

                    if (unitTypeIds.Count < 1)
                    {
                        break;
                    }

                    long currUnitTypeId = unitTypeIds[gs.rand.Next() % unitTypeIds.Count];

                    unitTypeIds.Remove(currUnitTypeId);

                    ZoneUnitStatus unitStatus = zone.Units.FirstOrDefault(x => x.UnitTypeId == currUnitTypeId);

                    if (unitStatus == null)
                    {
                        continue;
                    }

                    long taskEntityTypeId = 0;
                    long taskEntityId = 0;
                    long onEntityTypeId = 0;
                    long onEntityId = 0;
                    int taskQuantity = 1;
                    float dropChance = 1.0f;
                    int minDrop = 1;
                    int maxDrop = 1;


                    if (rand.NextDouble() < 0.5f)
                    {
                        taskEntityTypeId = EntityTypes.QuestItem;
                        double onLocation = rand.NextDouble();
                        if (onLocation < 0.4f)
                        {
                            onEntityTypeId = EntityTypes.Unit;
                            taskQuantity = MathUtils.IntRange(1, 20, rand);
                            dropChance = MathUtils.FloatRange(0.3, 1.0f, rand);
                            if (rand.NextDouble() < 0.3f)
                            {
                                dropChance = 1.0f;
                            }
                        }
                        else 
                        {
                            onEntityTypeId = EntityTypes.GroundObject;
                            taskQuantity = MathUtils.IntRange(4, 10, rand);
                        }
                    }
                    else
                    {
                        taskEntityTypeId = EntityTypes.Unit;
                        taskQuantity = MathUtils.IntRange(2, 10, rand);
                    }

                    if (taskQuantity > 5 && rand.NextDouble() < 0.5f)
                    {
                        maxDrop++;
                    }

                    if (taskQuantity > 10 && rand.NextDouble() < 0.5f)
                    {
                        maxDrop++;
                    }

                    QuestItem questItem = null;

                    if (taskEntityTypeId == EntityTypes.Unit)
                    {
                        taskEntityId = unitStatus.UnitTypeId;
                    }
                    if (onEntityTypeId == EntityTypes.Unit)
                    {
                        onEntityId = unitStatus.UnitTypeId;
                    }
                    if (taskEntityTypeId == EntityTypes.QuestItem)
                    {
                        questItem = new QuestItem() { IdKey = GetNextQuestItemId(gs) };
                        questItem.Name = "QuestItem" + questType.IdKey;
                        questItem.Icon = "Acorn_001";
                        questItem.Art = "Pottery" + MathUtils.IntRange(1, 27, rand);
                        gs.map.QuestItems.Add(questItem);
                    }


                    QuestTask qtask = new QuestTask()
                    {
                        TaskEntityTypeId = taskEntityTypeId,
                        TaskEntityId = taskEntityId,
                        OnEntityTypeId = onEntityTypeId,
                        OnEntityId = onEntityId,
                        Quantity = taskQuantity,
                        Index = questType.Tasks.Count + 1,
                        MinDrop = minDrop,
                        MaxDrop = maxDrop,
                    };

                    questType.Tasks.Add(qtask);

                    // Create NPC code needs changing, NPCGenData add to map.
                    // Create QuestItem code needs changing, QuestItemGenData add to map.

                    // MAKE THESE RETURN THE NEW NPC/QUESTITEM SO THEY CAN BE WORKED WITH

                    // Then depending on quest type add spawns to the spawns chosen, and maybe make an NPC
                    // spawn.

                    // Add GroundItem/QuestItem spawns for the map 


                    // Pick a location where the quest will happen.
                    // For each quest task within a quest
                    // Kill normal mobs, find spawns near spot not set to a type, set them to that mob type.
                    // Kill npc, add npc spawn near a position...create new NPC
                    // Loot from mob, set mobs to that type, then set the OnEntityTypeId/Key...create new QuestItem
                    // Loot from zone (ground) check random spots near the desired location and if
                    // the spot is only grass or nothing, and not too close to a road, place an item there.
                    // Also create new QuestItem and use that.

                }

                map.Quests.Add(questType);
            }


        }

    }

    protected long GetNextQuestTypeId(UnityGameState gs)
    {
        if (gs.map == null || gs.map.Quests == null ||
            gs.map.Quests.Count < 1)
        {
            return 1;
        }

        return gs.map.Quests.Max(X => X.IdKey) + 1;
    }

    public long GetNextQuestItemId(UnityGameState gs)
    {
        if (gs.map == null || gs.map.QuestItems == null ||
            gs.map.QuestItems.Count < 1)
        {
            return 1;
        }

        return gs.map.QuestItems.Max(X => X.IdKey) + 1;
    }

    protected List<MapSpawn> GetNPCsForZone(GameState gs, Map map, long zoneId)
    {
        List<MapSpawn> zoneSpawns = gs.spawns.Data.Where(x=>x.ZoneId == zoneId &&
        x.Addons != null && x.Addons.Count > 0).ToList();

        return zoneSpawns;
    }


    protected List<MapSpawn> GetUnusedSpawnsNearPoint(GameState gs, long zoneId, int px, int py, float radius)
    {
        if (gs.spawns == null || gs.spawns.Data == null)
        {
            return new List<MapSpawn>();
        }

        List<MapSpawn> desiredUnits = gs.spawns.Data.Where(u =>
        (u.EntityTypeId == EntityTypes.ZoneUnit || u.EntityTypeId == EntityTypes.Unit) &&
        u.EntityId == 0 &&
        Math.Sqrt((u.X - px) * (u.X - px) + (u.Z - py) * (u.Z - py)) <= radius).ToList();

        return desiredUnits;
    }
}
