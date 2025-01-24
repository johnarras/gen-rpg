using Amazon.Runtime.Internal.Util;
using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
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

    public class UnitSummons
    {
        public UnitType BaseUnitType { get; set; }
        public UnitType UnitType { get; set; }
        public string Summons { get; set; }
    }

    public class UnitTypeImporter : BaseCrawlerDataImporter
    {
        public override string ImportDataFilename => "UnitTypeImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UnitTypes; }

        const int NameCol = 0;
        const int PluralCol = 1;
        const int TribeCol = 2;
        const int SuffixCol = 3;
        const int MinLevelCol = 4;
        const int HpBonusCol = 5;
        const int DamBonusCol = 6;
        const int RoleScalingPercentCol = 7;
        const int SpawnQuantityCol = 8;
        const int VulnerabilityCol = 9;
        const int ResistCol = 10;
        const int SummonCol = 11;
        const int ColumnCount = 11;

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            string[] firstLine = lines[0];


            List<UnitSummons> summons = new List<UnitSummons>();

            UnitSettings settings = gs.data.Get<UnitSettings>(null);

            IReadOnlyList<UnitType> startUnitTypes = settings.GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = gs.data.Get<StatusEffectSettings>(null).GetData();  

            IReadOnlyList<ElementType> elementTypes = gs.data.Get<ElementTypeSettings>(null).GetData(); 

            gs.LookedAtObjects.Add(gs.data.Get<UnitSettings>(null));

            List<UnitType> newList = new List<UnitType>();

            IReadOnlyList<TribeType> tribes = gs.data.Get<TribeSettings>(null).GetData();

            int nextIdKey = 1;
            for (int l = 1; l < lines.Count; l++)
            {
                string[] words = lines[l];

                if (words.Length < ColumnCount) // so far need 8 columns.
                {
                    continue;
                }

                if (string.IsNullOrEmpty(words[NameCol]) || string.IsNullOrEmpty(words[PluralCol]) || string.IsNullOrEmpty(words[TribeCol]))
                {
                    continue;
                }

                TribeType tribe = tribes.FirstOrDefault(x => x.Name == words[TribeCol]);

                if (tribe == null)
                {
                    ShowErrorDialog(gs, "Missing tribe named " + words[TribeCol] + " for monster " + words[NameCol]);
                    return false;
                }

                List<string> suffixes = new List<string>();
                suffixes.Add("");


                if (!string.IsNullOrEmpty(words[SuffixCol]))
                {
                    string[] newSuffixes = words[SuffixCol].Trim().Split(' ');
                    foreach (string suffix in newSuffixes)
                    {
                        suffixes.Add(suffix);
                    }
                }

                UnitType baseUnitType = null;
                for (int s = 0; s < suffixes.Count; s++)
                {
                    string suffix = suffixes[s];
                    UnitType unitType = new UnitType() { IdKey = nextIdKey++ };

                    if (s == 0)
                    {
                        baseUnitType = unitType;    
                    }


                    summons.Add(new UnitSummons()
                    {
                        UnitType = unitType,
                        BaseUnitType = baseUnitType,
                        Summons = words[SummonCol],
                    });

                

                    _importService.ImportLine<UnitType>(gs, l, words, firstLine, unitType);

                    if (string.IsNullOrEmpty(suffix))
                    {
                        unitType.Name = words[NameCol];
                        unitType.PluralName = words[PluralCol];
                    }
                    else
                    {
                        unitType.Name = words[NameCol] + " " + suffix;
                        unitType.PluralName = words[NameCol] + " " + suffix + "s";
                    }

                    unitType.Icon = unitType.Name.Replace(" ", "");
                    unitType.Art = unitType.Icon;

                    unitType.TribeTypeId = tribe.IdKey;
                    unitType.Effects = new List<UnitEffect>();


                    if (Int64.TryParse(words[HpBonusCol], out long hpBonus))
                    {
                        if (hpBonus > 0)
                        {
                            unitType.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.StatPct, EntityId = StatTypes.Health, Quantity = hpBonus });
                        }
                    }

                    

                    if (Int64.TryParse(words[DamBonusCol], out long damBonus))
                    {
                        if (damBonus > 0)
                        {
                            unitType.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.StatPct, EntityId = StatTypes.DamagePower, Quantity = damBonus });
                        }
                    }

                    if (Int64.TryParse(words[RoleScalingPercentCol], out long scalingPercent))
                    {
                        if (scalingPercent != 0)
                        {
                            unitType.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.Stat, EntityId = StatTypes.RoleScalingPercent, Quantity = scalingPercent });
                        }
                    }

                    for (int col = ColumnCount; col < ColumnCount+20; col++)
                    {
                        if (words.Length <= col)
                        {
                            break;
                        }

                        string word = words[col];

                        if (string.IsNullOrEmpty(word))
                        {
                            continue;
                        }

                        CrawlerSpell matchingSpell = crawlerSpells.FirstOrDefault(x=>x.Name.ToLower() == 
                        word.ToLower());

                      
                        if (matchingSpell != null)
                        {
                            if (word == "Defend")
                            {
                                Console.WriteLine("Found Defend");
                            }
                            unitType.Effects.Add(new UnitEffect()
                            {
                                EntityTypeId = EntityTypes.CrawlerSpell,
                                EntityId = matchingSpell.IdKey
                            });
                            continue;
                        }

                        StatusEffect eff = statusEffects.FirstOrDefault(x=>x.Name.ToLower() == word.ToLower());

                        if (eff != null)
                        {
                            unitType.Effects.Add(new UnitEffect()
                            {
                                EntityTypeId = EntityTypes.StatusEffect,
                                EntityId = eff.IdKey,
                            });
                            continue;
                        }
                    }
                    unitType.Effects.AddRange(ReadElementWords(words[VulnerabilityCol], EntityTypes.Vulnerability, elementTypes));
                    unitType.Effects.AddRange(ReadElementWords(words[ResistCol], EntityTypes.Resist, elementTypes));


                    newList.Add(unitType);
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
                            UnitType namedType = newList.FirstOrDefault(x => x.Name.Replace(" ", "").Trim().ToLower() == lowerWord);

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
            foreach (UnitType utype in newList)
            {
                gs.LookedAtObjects.Add(utype);
            }
            await Task.CompletedTask;
            return true;
        }
    }
}
