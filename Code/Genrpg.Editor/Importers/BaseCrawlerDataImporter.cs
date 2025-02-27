using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Genrpg.Editor.Importers
{
    public abstract class BaseCrawlerDataImporter : BaseDataImporter
    {
        protected override async Task<bool> UpdateAfterImport(Window win, EditorGameState gs)
        {
            RoleSettings roleSettings = gs.data.Get<RoleSettings>(null);

            IReadOnlyList<Role> roles = roleSettings.GetData();

            CrawlerSpellSettings spellSettings = gs.data.Get<CrawlerSpellSettings>(null);

            List<CrawlerSpell> spells = spellSettings.GetData().ToList();

            List<long> buffStatIds =
                 roles.SelectMany(x => x.BinaryBonuses)
                 .Where(y => y.EntityTypeId == EntityTypes.Stat && (y.EntityId < StatConstants.PrimaryStatStart || y.EntityId > StatConstants.PrimaryStatEnd))                 
                 .Select(x => x.EntityId)
                 .Distinct().ToList();

            IReadOnlyList<StatType> stats = gs.data.Get<StatSettings>(null).GetData();

            List<long> statIds = stats.Select(x => x.IdKey).ToList();

            buffStatIds = buffStatIds.Where(x => stats.Select(x => x.IdKey).Contains(x)).ToList();


            spells = spells
                .Where(x => x.IdKey < CrawlerSpellConstants.StatBuffSpellIdOffset 
            || x.IdKey >= CrawlerSpellConstants.StatBuffSpellIdOffset + StatTypes.Max)
                .ToList();

            foreach (long buffStatId in buffStatIds)
            {
                CrawlerSpell spell = new CrawlerSpell()
                {
                    IdKey = buffStatId + CrawlerSpellConstants.StatBuffSpellIdOffset,
                    Name = "Enhance " + stats.First(x => x.IdKey == buffStatId).Name,
                    PowerCost = spellSettings.StatBuffPowerCost,
                    PowerPerLevel = spellSettings.StatBuffPowerPerLevel,
                    MinRange = 0,
                    MaxRange = 100,
                    TargetTypeId = TargetTypes.Self,
                    Level = 1,
                    CombatActionId = CombatActions.Cast,
                    RoleScalingTypeId = RoleScalingTypes.Healing,
                };

                spells.Add(spell);

                spell.Effects.Add(new CrawlerSpellEffect()
                {
                    EntityTypeId = EntityTypes.Stat,
                    EntityId = buffStatId,
                    MinQuantity = 1,
                    MaxQuantity = 1,
                });

                gs.LookedAtObjects.Add(spell);
            }


            // For each role, remove crawler buff spells that no longer apply and add crawler buff spells that need to be there.
            foreach (Role role in roles)
            {
                role.BinaryBonuses = role.BinaryBonuses
                    .Where(x => x.EntityTypeId != EntityTypes.Stat
                    || x.EntityId < CrawlerSpellConstants.StatBuffSpellIdOffset
                    || x.EntityId > CrawlerSpellConstants.StatBuffSpellIdOffset + StatTypes.Max)
                    .ToList();

                List<RoleBonusBinary> bonusesToAdd = new List<RoleBonusBinary>();
                foreach (RoleBonusBinary bonus in role.BinaryBonuses)
                {
                    if (bonus.EntityTypeId == EntityTypes.Stat)
                    {
                        RoleBonusBinary spellBonus = role.BinaryBonuses
                            .Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell
                        && x.EntityId == bonus.EntityId + CrawlerSpellConstants.StatBuffSpellIdOffset)
                            .FirstOrDefault();
                        if (spellBonus == null)
                        {
                            bonusesToAdd.Add(new RoleBonusBinary()
                            {
                                EntityTypeId = EntityTypes.CrawlerSpell,
                                EntityId = bonus.EntityId + CrawlerSpellConstants.StatBuffSpellIdOffset
                            });
                        }
                    }
                }
                if (bonusesToAdd.Count > 0)
                {
                    role.BinaryBonuses.AddRange(bonusesToAdd);
                    gs.LookedAtObjects.Add(role);
                }
            }

            List<UnitType> unitTypes = gs.data.Get<UnitSettings>(null).GetData().ToList();


            IReadOnlyList<UnitKeyword> keywordList = gs.data.Get<UnitKeywordSettings>(null).GetData();

            foreach (UnitType utype in unitTypes)
            {

                List<UnitEffect> allEffects = new List<UnitEffect>();

                TribeType ttype = gs.data.Get<TribeSettings>(null).Get(utype.TribeTypeId);

                bool shouldSaveUnitType = false;

                if (ttype != null)
                {
                    UnitKeyword tribeKeyword = keywordList.FirstOrDefault(x => x.Name.ToLower() == ttype.Name.ToLower());

                    if (tribeKeyword != null)
                    {
                        allEffects.AddRange(tribeKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell &&
                        x.EntityId >= CrawlerSpellConstants.MinPlaceholderSpellId && x.EntityId <= CrawlerSpellConstants.MaxPlaceholderSpellId));
                    }
                }

                string[] nameWords = utype.Name.Split(' ');


                foreach (string nword in nameWords)
                {
                    UnitKeyword nameKeyword = keywordList.FirstOrDefault(x => x.Name.ToLower() == nword.ToLower());

                    if (nameKeyword != null)
                    {
                        allEffects.AddRange(nameKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell &&
                        x.EntityId >= CrawlerSpellConstants.MinPlaceholderSpellId && x.EntityId <= CrawlerSpellConstants.MaxPlaceholderSpellId));
                    }
                }

                foreach (UnitEffect effect in allEffects)
                {
                    if (effect.EntityId == CrawlerSpellConstants.SelfSummonPlaceholderSpellId)
                    {
                        long spellId = effect.EntityId + CrawlerSpellConstants.MonsterSummonSpellIdOffset;

                        UnitEffect currEffect = utype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId >= spellId);

                        if (currEffect == null)
                        {
                            utype.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = spellId, Quantity = 1 });
                            shouldSaveUnitType = true;
                        }
                    }
                    else if (effect.EntityId == CrawlerSpellConstants.BaseSummonPlaceholderSpellId)
                    {
                        int index = unitTypes.IndexOf(utype);

                        if (index > 0)
                        {
                            for (int idx = index - 1; idx > 0; idx--)
                            {
                                UnitType prevUnitType = unitTypes[idx];

                                if (utype.Name.Contains(prevUnitType.Name))
                                {
                                    long spellId = prevUnitType.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset;
                                    UnitEffect currEffect = utype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId >= spellId);

                                    if (currEffect == null)
                                    {
                                        utype.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = spellId, Quantity = 1 });

                                        shouldSaveUnitType = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (shouldSaveUnitType && !gs.LookedAtObjects.Contains(utype))
                {
                    gs.LookedAtObjects.Add(utype);
                }
            }





            foreach (UnitType utype in unitTypes)
            {
                long elementTypeId = ElementTypes.Arcane;

                CrawlerSpell currSpell = spells.FirstOrDefault(x => x.IdKey == utype.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset);

                if (currSpell != null)
                {
                    spells.Remove(currSpell);
                }

                UnitEffect effect = utype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Resist);

                if (effect != null && effect.EntityId > 0)
                {
                    elementTypeId = effect.EntityId;
                }

                CrawlerSpell spell = new CrawlerSpell()
                {
                    IdKey = utype.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset,
                    Name = "Monster Call " + utype.Name,
                    PowerCost = 100,
                    PowerPerLevel = 1,
                    MinRange = 0,
                    MaxRange = 100,
                    TargetTypeId = TargetTypes.Self,
                    Level = 1,
                    CombatActionId = CombatActions.Cast,
                    RoleScalingTypeId = RoleScalingTypes.Summon,
                };

                spell.Effects.Add(new CrawlerSpellEffect()
                {
                    EntityTypeId = EntityTypes.Unit,
                    EntityId = utype.IdKey,
                    MinQuantity = 1,
                    MaxQuantity = 1,
                    ElementTypeId = elementTypeId,               
                });

                gs.LookedAtObjects.Add(spell);
                spells.Add(spell);
            }

            spellSettings.SetData(spells);

            await Task.CompletedTask;
            return true;
        }
    }
}
