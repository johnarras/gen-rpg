using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class RoleInfoHelper : BaseInfoHelper<RoleSettings, Role>
    {
        public override long GetKey() { return EntityTypes.Role; }

        public override List<string> GetInfoLines(long entityId)
        {

            Role role = _gameData.Get<RoleSettings>(_gs.ch).Get(entityId);

            List<string> allLines = new List<string>();
            string categoryName = role.RoleCategoryId == RoleCategories.Class ? "(Class) "
            : role.RoleCategoryId == RoleCategories.Origin ? "(Race) "
            : "";
            allLines.Add(categoryName + role.Name + ": " + role.Desc);

            StatSettings statSettings = _gameData.Get<StatSettings>(_gs.ch);

            allLines.Add($"{role.HealthPerLevel} {_infoService.CreateInfoLink(statSettings.Get(StatTypes.Health))}/Level,"
                + $"{role.ManaPerLevel} {_infoService.CreateInfoLink(statSettings.Get(StatTypes.Mana))}/Level,"
                + $"{role.CritPercent}% {_infoService.CreateInfoLink(statSettings.Get(StatTypes.Crit))}");

            List<NameIdValue> bonuses = _statService.GetInitialStatBonuses(role.IdKey);

            IReadOnlyList<RoleScalingType> scalingTypes = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).GetData();

            if (bonuses.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Initial Stat Bonuses: ");

                foreach (NameIdValue nid in bonuses)
                {
                    if (nid.Val != 0)
                    {
                        sb.Append(_infoService.CreateInfoLink(statSettings.Get(nid.IdKey)) + ": " + nid.Val + " ");
                    }
                }

                allLines.Add(sb.ToString());
            }


            StringBuilder scalingBuilder = new StringBuilder();
            int roleScalingCount = 0;
            foreach (RoleBonusAmount amount in role.AmountBonuses)
            {
                if (amount.EntityTypeId == EntityTypes.RoleScaling)
                {
                    RoleScalingType scalingType = scalingTypes.FirstOrDefault(x => x.IdKey == amount.EntityId);
                    if (scalingType != null)
                    {
                        if (scalingBuilder.Length == 0)
                        {
                            scalingBuilder.Append("Tiers/Level: ");
                        }
                        scalingBuilder.Append(_infoService.CreateInfoLink(scalingType) + ": " + amount.Amount + " ");
                        roleScalingCount++;
                        if (roleScalingCount % 3 == 0)
                        {
                            allLines.Add(scalingBuilder.ToString());
                            scalingBuilder.Clear();
                        }
                    }
                }
            }
            allLines.Add(scalingBuilder.ToString());

            ShowBuffs(role, EntityTypes.Stat, allLines, "Stats: ", _gameData.Get<StatSettings>(null).GetData(), true);
            ShowBuffs(role, EntityTypes.PartyBuff, allLines, "Buffs: ", _gameData.Get<PartyBuffSettings>(null).GetData(), true);
            ShowBuffs(role, EntityTypes.CrawlerSpell, allLines, "Spells: ", _gameData.Get<CrawlerSpellSettings>(null).GetData(), true);


            return allLines;

        }
        protected virtual void ShowBuffs<T>(Role role, long entityTypeId, List<string> lines, string header, IReadOnlyList<T> gameDataList, bool inOneRow) where T : IIndexedGameItem
        {
            int quantityPerRow = 4;
            List<RoleBonusBinary> bonuses = role.BinaryBonuses.Where(x => x.EntityTypeId == entityTypeId).ToList();

            if (bonuses.Count < 1)
            {
                return;
            }

            List<T> dataItems = new List<T>();
            foreach (RoleBonusBinary bonus in bonuses)
            {
                T dataItem = gameDataList.FirstOrDefault(x => x.IdKey == bonus.EntityId);
                if (dataItem != null)
                {
                    dataItems.Add(dataItem);
                }
            }

            if (dataItems.Count < 1)
            {
                return;
            }

            if (inOneRow)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(header + ": ");
                for (int d = 0; d < dataItems.Count; d++)
                {
                    sb.Append(_infoService.CreateInfoLink(dataItems[d]));
                    if (d < dataItems.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    if (d % quantityPerRow == quantityPerRow - 1 || d == dataItems.Count - 1)
                    {
                        lines.Add(sb.ToString());
                        sb.Clear();
                        sb.Append(" ");
                    }
                }
            }
            else
            {
                lines.Add(header);

                if (typeof(IOrderedItem).IsAssignableFrom(typeof(T)))
                {
                    List<IOrderedItem> orderedItems = dataItems.Cast<IOrderedItem>().ToList();

                    orderedItems = orderedItems.OrderBy(x => x.GetOrder()).ToList();

                    dataItems = orderedItems.Cast<T>().ToList();

                }
                else
                {
                    dataItems = dataItems.OrderBy(x => x.Name).ToList();
                }
                foreach (T dataItem in dataItems)
                {
                    lines.Add(_infoService.CreateInfoLink(dataItem));
                }
            }
        }
    }
}
