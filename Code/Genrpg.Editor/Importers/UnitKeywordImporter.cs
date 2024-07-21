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

    public class UnitKeywordImporter : BaseDataImporter
    {
        public override string ImportDataFilename => "UnitKeywordImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UnitKeywords; }

        const int NameCol = 0;
        const int HpBonusCol = 1;
        const int DamBonusCol = 2;
        const int MinRangeCol = 3;
        const int MinLevelCol = 4;
        const int ColumnCount = 5;

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            string[] firstLine = lines[0].Split(',');

            UnitKeywordSettings settings = gs.data.Get<UnitKeywordSettings>(null);

            IReadOnlyList<UnitKeyword> startUnitKeywords = settings.GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = gs.data.Get<StatusEffectSettings>(null).GetData();  

            gs.LookedAtObjects.Add(gs.data.Get<UnitKeywordSettings>(null));

            List<UnitKeyword> newList = new List<UnitKeyword>();
            newList.Add(new UnitKeyword() { IdKey = 0, Name = "None" });

            IReadOnlyList<TribeType> tribes = gs.data.Get<TribeSettings>(null).GetData();

            int nextIdKey = 1;
            for (int l = 1; l < lines.Length; l++)
            {
                string[] words = lines[l].Split(",");

                if (words.Length < ColumnCount) // so far need 8 columns.
                {
                    continue;
                }

                if (string.IsNullOrEmpty(words[NameCol]))
                {
                    continue;
                }

                UnitKeyword unitKeyword = new UnitKeyword() { IdKey = nextIdKey++ };

                unitKeyword.Name = words[NameCol];

                unitKeyword.Icon = unitKeyword.Name.Replace(" ", "");
                unitKeyword.Art = unitKeyword.Icon;

                if (Int64.TryParse(words[MinLevelCol], out long minLevel))
                {
                    unitKeyword.MinLevel = minLevel;
                }

                if (Int32.TryParse(words[MinRangeCol], out int minRange))
                {
                    unitKeyword.MinRange = minRange;
                }

                if (Int64.TryParse(words[HpBonusCol], out long hpBonus))
                {
                    if (hpBonus > 0)
                    {
                        unitKeyword.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.StatPct, EntityId = StatTypes.Health, Quantity = hpBonus });
                    }
                }

                if (Int64.TryParse(words[DamBonusCol], out long damBonus))
                {
                    if (damBonus > 0)
                    {
                        unitKeyword.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.StatPct, EntityId = StatTypes.DamagePower, Quantity = damBonus });
                    }
                }

                for (int col = ColumnCount; col < ColumnCount + 20; col++)
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

                    CrawlerSpell matchingSpell = crawlerSpells.FirstOrDefault(x => x.Name.ToLower() ==
                    word.ToLower());

                    if (matchingSpell != null)
                    {
                        unitKeyword.Effects.Add(new UnitEffect()
                        {
                            EntityTypeId = EntityTypes.CrawlerSpell,
                            EntityId = matchingSpell.IdKey
                        });
                        continue;
                    }

                    StatusEffect eff = statusEffects.FirstOrDefault(x => x.Name.ToLower() == word.ToLower());

                    if (eff != null)
                    {
                        unitKeyword.Effects.Add(new UnitEffect()
                        {
                            EntityTypeId = EntityTypes.StatusEffect,
                            EntityId = eff.IdKey,
                        });
                        continue;
                    }
                }
                newList.Add(unitKeyword);
            }
            settings.SetData(newList);
            foreach (UnitKeyword eyword in newList)
            {
                gs.LookedAtObjects.Add(eyword);
            }
            await Task.CompletedTask;
            return true;
        }
    }
}
