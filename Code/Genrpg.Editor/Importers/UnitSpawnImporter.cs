using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Zones.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{

    public class UnitSpawnImporter : BaseDataImporter
    {
        public override string ImportDataFilename => "UnitSpawnImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UnitSpawns; }


        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            string[] firstLine = lines[0].Split(',');

            ZoneTypeSettings settings = gs.data.Get<ZoneTypeSettings>(null);

            gs.LookedAtObjects.Add(settings);

            IReadOnlyList<ZoneType> allZoneTypes = settings.GetData();

            gs.LookedAtObjects.AddRange(allZoneTypes);

            IReadOnlyList<UnitType> unitTypes = gs.data.Get<UnitSettings>(null).GetData();




            ZoneType[] zoneTypes = new ZoneType[firstLine.Length];

            for (int c = 1; c < firstLine.Length; c++)
            {
                string zoneName = firstLine[c];

                ZoneType zoneType = allZoneTypes.FirstOrDefault(x=>x.Name == zoneName);

                if (zoneType == null)
                {
                    ShowErrorDialog(gs, "Missing zone with name " + zoneName);
                    return false;
                }

                zoneTypes[c] = zoneType;

                zoneType.ZoneUnitSpawns = new List<ZoneUnitSpawn>();
            }

            for (int row = 1; row < lines.Length; row++)
            {
                string line = lines[row];

                string[] words = line.Split(",");

                if (words.Length < 1 || string.IsNullOrEmpty(words[0]))
                {
                    continue;
                }

                string unitName = words[0];

                List<long> okUnitTypes = new List<long>();

                foreach (UnitType utype in unitTypes)
                {
                    string[] namewords = utype.Name.Split(" ");

                    if (namewords.Length < 1 || string.IsNullOrEmpty(namewords[0]))
                    {
                        continue;
                    }

                    if (namewords[0] == unitName)
                    {
                        okUnitTypes.Add(utype.IdKey);
                    }
                }

                for (int col = 1; col < words.Length && col < zoneTypes.Length; col++)
                {
                    if (double.TryParse(words[col], out double spawnChance))
                    {
                        if (spawnChance > 0)
                        {
                            foreach (long unitTypeId in okUnitTypes)
                            {
                                zoneTypes[col].ZoneUnitSpawns.Add(new ZoneUnitSpawn()
                                {
                                   UnitTypeId = unitTypeId,
                                   Chance = spawnChance,
                                });
                            }                                 
                        }
                    }
                }
            }

            await Task.CompletedTask;
            return true;
        }
    }
}
