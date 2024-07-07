using Amazon.Runtime.Internal.Util;
using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Genrpg.Editor.Importers
{

    public class UnitTypeImporter : BaseCSVImporter
    {
        public override string CSVFilename => "UnitTypeImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UnitTypes; }

        const int NameCol = 0;
        const int PluralCol = 1;
        const int TribeCol = 2;
        const int SuffixCol = 3;
        const int IsPlayerCol = 4;
        const int MinLevelCol = 5;
        const int HpBonusCol = 6;
        const int DamBonusCol = 7;

        const int ColumnCount = 8;

        protected override async Task<bool> ImportFromLines(Window window, EditorGameState gs, string[] lines)
        {
            string[] firstLine = lines[0].Split(',');

            string missingWords = "";

            UnitSettings settings = gs.data.Get<UnitSettings>(null);

            IReadOnlyList<UnitType> startUnitTypes = settings.GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = gs.data.Get<StatusEffectSettings>(null).GetData();  

            gs.LookedAtObjects.Add(gs.data.Get<UnitSettings>(null));

            List<UnitType> newList = new List<UnitType>();
            newList.Add(new UnitType() { IdKey = 0, Name = "None" });

            IReadOnlyList<TribeType> tribes = gs.data.Get<TribeSettings>(null).GetData();

            int nextIdKey = 1;
            for (int l = 1; l < lines.Length; l++)
            {
                string[] words = lines[l].Split(",");

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

                foreach (string suffix in suffixes)
                {
                    UnitType unitType = new UnitType() { IdKey = nextIdKey++ };

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

                    if (string.IsNullOrEmpty(suffix) && bool.TryParse(words[IsPlayerCol], out bool isPlayer))
                    {
                        unitType.PlayerRace = isPlayer;
                    }

                    if (Int64.TryParse(words[MinLevelCol], out long minLevel))
                    {
                        unitType.MinLevel = minLevel;
                    }

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

                    unitType.Effects = new List<UnitEffect>();

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
                    newList.Add(unitType);
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
