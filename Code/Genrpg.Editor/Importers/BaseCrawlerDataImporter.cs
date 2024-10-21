using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
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
                 roles.SelectMany(x => x.Bonuses)
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
                role.Bonuses = role.Bonuses
                    .Where(x => x.EntityTypeId != EntityTypes.Stat
                    || x.EntityId < CrawlerSpellConstants.StatBuffSpellIdOffset
                    || x.EntityId > CrawlerSpellConstants.StatBuffSpellIdOffset + StatTypes.Max)
                    .ToList();

                List<RoleBonus> bonusesToAdd = new List<RoleBonus>();
                foreach (RoleBonus bonus in role.Bonuses)
                {
                    if (bonus.EntityTypeId == EntityTypes.Stat)
                    {
                        RoleBonus spellBonus = role.Bonuses
                            .Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell
                        && x.EntityId == bonus.EntityId - CrawlerSpellConstants.StatBuffSpellIdOffset)
                            .FirstOrDefault();
                        if (spellBonus == null)
                        {
                            bonusesToAdd.Add(new RoleBonus()
                            {
                                EntityTypeId = EntityTypes.CrawlerSpell,
                                EntityId = bonus.EntityId + CrawlerSpellConstants.StatBuffSpellIdOffset
                            });
                        }
                    }
                }
                if (bonusesToAdd.Count > 0)
                {
                    role.Bonuses.AddRange(bonusesToAdd);
                    gs.LookedAtObjects.Add(role);
                }
            }

            spellSettings.SetData(spells);

            await Task.CompletedTask;
            return true;
        }
    }
}
