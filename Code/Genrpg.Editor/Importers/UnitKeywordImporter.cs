using Amazon.Runtime.Internal.Util;
using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Combat.Constants;
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
using Microsoft.UI.Xaml;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Text;
using Windows.UI.Popups;
using static Azure.Core.HttpHeader;

namespace Genrpg.Editor.Importers
{


    public class KeywordSummons
    {
        public UnitKeyword Keyword { get; set; }
        public string Summons { get; set; }
    }

    public class UnitKeywordImporter : BaseCrawlerDataImporter
    {
        public override string ImportDataFilename => "UnitKeywordImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UnitKeywords; }

        const int NameCol = 0;
        const int HpBonusCol = 1;
        const int DamBonusCol = 2;
        const int MinRangeCol = 3;
        const int MinLevelCol = 4;
        const int VulnerabilityCol = 5;
        const int ResistCol = 6;
        const int SummonCol = 7;
        const int ColumnCount = 7;

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            string[] firstLine = lines[0];

            UnitKeywordSettings settings = gs.data.Get<UnitKeywordSettings>(null);

            IReadOnlyList<UnitKeyword> startUnitKeywords = settings.GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = gs.data.Get<StatusEffectSettings>(null).GetData();  

            IReadOnlyList<ElementType> elementTypes = gs.data.Get<ElementTypeSettings>(null).GetData();

            gs.LookedAtObjects.Add(gs.data.Get<UnitKeywordSettings>(null));

            List<UnitKeyword> newList = new List<UnitKeyword>();
            List<KeywordSummons> keywordSummons = new List<KeywordSummons>();
            IReadOnlyList<TribeType> tribes = gs.data.Get<TribeSettings>(null).GetData();

            int nextIdKey = 1;
            for (int l = 1; l < lines.Count; l++)
            {
                string[] words = lines[l];

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

                unitKeyword.Effects.AddRange(ReadElementWords(words[VulnerabilityCol], EntityTypes.Vulnerability, elementTypes));
                unitKeyword.Effects.AddRange(ReadElementWords(words[ResistCol], EntityTypes.Resist, elementTypes));

                keywordSummons.Add(new KeywordSummons()
                {
                    Keyword = unitKeyword,
                    Summons = words[SummonCol],
                });

                newList.Add(unitKeyword);
            }
            settings.SetData(newList);

            List<UnitType> unitTypes = gs.data.Get<UnitSettings>(null).GetData().ToList();

            foreach (KeywordSummons summon in keywordSummons)
            {
                if (!string.IsNullOrEmpty(summon.Summons))
                {
                    string[] slist = summon.Summons.Split(' ');

                    foreach (string word in slist)
                    {
                        string lowerWord = word.ToLower();

                        if (lowerWord == "self")
                        {
                            summon.Keyword.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = CrawlerSpellConstants.SelfSummonPlaceholderSpellId, Quantity = 1 });
                        }
                        else if (lowerWord == "base")
                        {
                            summon.Keyword.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = CrawlerSpellConstants.BaseSummonPlaceholderSpellId, Quantity = 1 });
                        }
                        else
                        {
                            UnitType namedType = unitTypes.FirstOrDefault(x => x.Name.Replace(" ", "").Trim().ToLower() == lowerWord);

                            if (namedType != null)
                            {
                                summon.Keyword.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = namedType.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset, Quantity = 1 });
                            }
                        }
                    }
                }
            }

            foreach (UnitKeyword eyword in newList)
            {
                gs.LookedAtObjects.Add(eyword);
            }
            await Task.CompletedTask;
            return true;
        }
    }
}
