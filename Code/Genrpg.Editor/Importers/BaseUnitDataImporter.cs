using Amazon.Runtime.Internal.Util;
using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Interfaces;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using MessagePack.Resolvers;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace Genrpg.Editor.Importers
{

    public class UnitImportRow
    {
        public string Name { get; set; }
        public string PluralName { get; set; }
        public string TribeName { get; set; }
        public string Suffixes { get; set; }
        public int MinLevel { get; set; }
        public int MinRange { get; set; }
        public string StatPercents { get; set; }
        public string Stats { get; set; }
        public string Vulns { get; set; }
        public string Resists { get; set; }
        public string Summons { get; set; }
        public float SpawnQuantityScale { get; set; }
        public string Procs { get; set; }
        public string Spells { get; set; }
        public string CommonSpawns { get; set; }
        public string UncommonSpawns { get; set; }
        public string RareSpawns { get; set; }
    }

    public class UnitSummons
    {
        public IUnitRole BaseUnitType { get; set; }
        public IUnitRole UnitType { get; set; }
        public string Summons { get; set; }
    }

    public abstract class BaseUnitDataImporter<TParent, TChild> : BaseCrawlerDataImporter where TParent : ParentSettings<TChild> where TChild : ChildSettings, IUnitRole, new()
    {

        IEditorReflectionService _reflectionService;

        public abstract override string ImportDataFilename { get; }

        public abstract override EImportTypes GetKey();

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            string[] firstLine = lines[0];

            List<UnitSummons> summons = new List<UnitSummons>();

            TParent settings = gs.data.Get<TParent>(null);

            IReadOnlyList<TChild> startUnitTypes = settings.GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = gs.data.Get<StatusEffectSettings>(null).GetData();

            IReadOnlyList<ElementType> elementTypes = gs.data.Get<ElementTypeSettings>(null).GetData();

            IReadOnlyList<TribeType> tribes = gs.data.Get<TribeSettings>(null).GetData();

            IReadOnlyList<UnitType> unitTypes = gs.data.Get<UnitSettings>(null).GetData();

            IReadOnlyList<UnitKeyword> unitKeywords = gs.data.Get<UnitKeywordSettings>(null).GetData();

            IReadOnlyList<ZoneType> zoneTypes = gs.data.Get<ZoneTypeSettings>(null).GetData();

            foreach (ZoneType zoneType in zoneTypes)
            {
                zoneType.ZoneUnitSpawns = new List<ZoneUnitSpawn>();
                gs.LookedAtObjects.Add(zoneType);   
            }

            gs.LookedAtObjects.Add(gs.data.Get<TParent>(null));

            List<TChild> newList = new List<TChild>();

            int nextIdKey = 1;
            for (int l = 1; l < lines.Count; l++)
            {
                string[] words = lines[l];

                UnitImportRow importRow = _importService.ImportLine<UnitImportRow>(gs, l, words, firstLine, null, true);

                if (string.IsNullOrEmpty(importRow.Name))
                {
                    continue;
                }

                List<string> suffixes = new List<string>();
                suffixes.Add("");

                if (!string.IsNullOrEmpty(importRow.Suffixes))
                {
                    string[] newSuffixes = importRow.Suffixes.Split(",");
                    foreach (string suffix in newSuffixes)
                    {
                        suffixes.Add(suffix.Trim());
                    }
                }

                long tribeTypeId = 0;
                if (!string.IsNullOrEmpty(importRow.TribeName))
                {
                    TribeType ttype = tribes.FirstOrDefault(x => x.Name.ToLower().Trim() == importRow.TribeName.ToLower().Trim());
                    if (ttype != null)
                    {
                        tribeTypeId = ttype.IdKey;
                    }
                }

                TChild baseChild = null;
                for (int s = 0; s < suffixes.Count; s++)
                {

                    UnitKeyword suffixKeyword = null;

                    if (!string.IsNullOrEmpty(suffixes[s]))
                    {
                        suffixKeyword = unitKeywords.FirstOrDefault(x => x.Name.ToLower() == suffixes[s].ToLower());
                        if (suffixKeyword == null)
                        {
                            continue;
                        }
                    }

                    string suffix = suffixes[s];
                    TChild child = SerializationUtils.ConvertType<UnitImportRow, TChild>(importRow);
                    child.IdKey = nextIdKey++;
                    newList.Add(child);

                    child.Icon = child.Name.Replace(" ", "");
                    child.Art = child.Name.Replace(" ", "");
                    if (s == 0)
                    {
                        baseChild = child;
                    }

                    summons.Add(new UnitSummons()
                    {
                        UnitType = child,
                        BaseUnitType = baseChild,
                        Summons = importRow.Summons,
                    });


                    if (string.IsNullOrEmpty(suffix))
                    {
                        child.Name = importRow.Name;
                        child.PluralName = importRow.PluralName;
                    }
                    else
                    {
                        child.Name = importRow.Name + " " + suffix;
                        if (suffixKeyword == null || string.IsNullOrEmpty(suffixKeyword.PluralName))
                        {
                            child.PluralName = importRow.Name + " " + suffix + "s";
                        }
                        else
                        {
                            child.PluralName = importRow.Name + " " + suffixKeyword.PluralName;
                        }
                    }

                    child.Icon = child.Name.Replace(" ", "");
                    child.Art = child.Icon;

                    if (tribeTypeId > 0)
                    {
                        _reflectionService.SetObjectValue(child, "TribeTypeId", tribeTypeId);
                    }


                    child.Effects = new List<UnitEffect>();


                    _importService.AddEffectList<TParent, StatSettings, StatType, UnitEffect>(gs, l, "StatPercents", EntityTypes.StatPct, child.Effects, importRow.StatPercents);
                    _importService.AddEffectList<TParent, StatSettings, StatType, UnitEffect>(gs, l, "Stats", EntityTypes.Stat, child.Effects, importRow.Stats);
                    _importService.AddEffectList<TParent, ElementTypeSettings, ElementType, UnitEffect>(gs, l, "Vulns", EntityTypes.Vulnerability, child.Effects, importRow.Vulns);
                    _importService.AddEffectList<TParent, ElementTypeSettings, ElementType, UnitEffect>(gs, l, "Resists", EntityTypes.Resist, child.Effects, importRow.Resists);
                    _importService.AddEffectList<TParent, StatusEffectSettings, StatusEffect, UnitEffect>(gs, l, "Procs", EntityTypes.StatusEffect, child.Effects, importRow.Procs);
                    _importService.AddEffectList<TParent, CrawlerSpellSettings, CrawlerSpell, UnitEffect>(gs, l, "Spells", EntityTypes.CrawlerSpell, child.Effects, importRow.Spells);

                    ImportSpawns(gs, child.IdKey, 1, importRow.RareSpawns);
                    ImportSpawns(gs, child.IdKey, 10, importRow.UncommonSpawns);
                    ImportSpawns(gs, child.IdKey, 100, importRow.CommonSpawns);
                }

            }

            foreach (UnitSummons summon in summons)
            {
                if (!string.IsNullOrEmpty(summon.Summons))
                {
                    string[] slist = summon.Summons.Split(' ');

                    foreach (string word in slist)
                    {
                        string lowerWord = word.ToLower();

                        long summonIdkey = 0;

                        if (lowerWord == "self")
                        {
                            summonIdkey = summon.UnitType.IdKey;
                        }
                        else if (lowerWord == "base")
                        {
                            summonIdkey = summon.BaseUnitType.IdKey;
                        }
                        else
                        {
                            UnitType namedType = unitTypes.FirstOrDefault(x => x.Name.Replace(" ", "").Trim().ToLower() == lowerWord);

                            if (namedType != null)
                            {
                                summonIdkey = namedType.IdKey;
                            }
                        }

                        if (summonIdkey > 0)
                        {
                            summon.UnitType.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = summonIdkey + CrawlerSpellConstants.MonsterSummonSpellIdOffset, Quantity = 1 });
                        }

                    }
                }
            }

            settings.SetData(newList);
            foreach (TChild utype in newList)
            {
                gs.LookedAtObjects.Add(utype);
            }
            await Task.CompletedTask;
            return true;
        }

        private void ImportSpawns(EditorGameState gs, long unitTypeId, double spawnWeight, string spawnZoneList)
        {

            if (string.IsNullOrEmpty(spawnZoneList))
            {
                return;
            }

            string[] words = spawnZoneList.Split(',');


            IReadOnlyList<ZoneType> zoneTypes = gs.data.Get<ZoneTypeSettings>(null).GetData();

            IReadOnlyList<ZoneCategory> zoneCategories = gs.data.Get<ZoneCategorySettings>(null).GetData();

            for (int w = 0; w < words.Length; w++)
            {
                string word = StrUtils.NormalizeWord(words[w]);


                ZoneType ztype = zoneTypes.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == word);

                if (ztype != null)
                {
                    AddSpawnWeight(unitTypeId, spawnWeight, ztype.ZoneUnitSpawns);
                }
                else
                {
                    ZoneCategory category = zoneCategories.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == word);

                    if (category != null)
                    {
                        List<ZoneType> categoryZones = zoneTypes.Where(x => x.ZoneCategoryId == category.IdKey).ToList();

                        foreach (ZoneType zoneType in categoryZones)
                        {
                            AddSpawnWeight(unitTypeId, spawnWeight, zoneType.ZoneUnitSpawns);
                        }
                    }
                }
            }
        }

        private void AddSpawnWeight(long unitTypeId, double spawnWeight, List<ZoneUnitSpawn> zoneUnitSpawns)
        {

            ZoneUnitSpawn currSpawn = zoneUnitSpawns.FirstOrDefault(x => x.UnitTypeId == unitTypeId);

            if (currSpawn == null)
            {
                currSpawn = new ZoneUnitSpawn() { UnitTypeId = unitTypeId };
                zoneUnitSpawns.Add(currSpawn);
            }

            currSpawn.Weight = Math.Max(currSpawn.Weight, spawnWeight);
        }
    }
}
