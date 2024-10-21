using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guild.CreateMember
{
    public abstract class BaseRoleStateHelper : BaseStateHelper
    {
        protected virtual void OnPointerEnter(Role role)
        {
            List<string> allLines = new List<string>();

            string categoryName = role.RoleCategoryId == RoleCategories.Class ? "(Class) "
                : role.RoleCategoryId == RoleCategories.Origin ? "(Race) "
                : "";

            allLines.Add(categoryName + role.Name + ": " + role.Desc);

            allLines.Add($"{role.HealthPerLevel} Hp/Level, {role.ManaPerLevel} Mana/Level, {role.CritPercent}% Crit");

            List<NameIdValue> bonuses = _statService.GetInitialStatBonuses(role.IdKey);

            if (bonuses.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Initial Stat Bonuses: ");

                foreach (NameIdValue nid in bonuses)
                {
                    if (nid.Val != 0)
                    {
                        sb.Append(nid.Name + ": " + nid.Val + " ");
                    }
                }

                allLines.Add(sb.ToString());
            }



            allLines.Add("Bonuses Per Level::");
            allLines.Add($"Melee: {role.MeleeScaling}, Ranged: {role.RangedScaling}, SpellDam: {role.SpellDamScaling}, Heal: {role.HealingScaling} Summon: {role.SummonScaling}");

            ShowBuffs(role, EntityTypes.Stat, allLines, "Stats: ", _gameData.Get<StatSettings>(null).GetData(), true);
            ShowBuffs(role, EntityTypes.PartyBuff, allLines, "Buffs: ", _gameData.Get<PartyBuffSettings>(null).GetData(), true);
            ShowBuffs(role, EntityTypes.CrawlerSpell, allLines, "Spells: ", _gameData.Get<CrawlerSpellSettings>(null).GetData(), false);

            _dispatcher.Dispatch(new ShowCrawlerTooltipEvent() { Lines = allLines });
        }

        protected virtual void ShowBuffs<T>(Role role, long entityTypeId, List<string> lines, string header, IReadOnlyList<T> gameDataList, bool inOneRow) where T : IIndexedGameItem
        {
            List<RoleBonus> bonuses = role.Bonuses.Where(x => x.EntityTypeId == entityTypeId).ToList();

            if (bonuses.Count < 1)
            {
                return;
            }

            List<T> dataItems = new List<T>();
            foreach (RoleBonus bonus in bonuses)
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
                string fullText = header + " ";

                for (int d = 0; d < dataItems.Count; d++)
                {
                    fullText += dataItems[d].Name;
                    if (d < dataItems.Count - 1)
                    {
                        fullText += ", ";
                    }
                }
                lines.Add(fullText);
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

                    if (dataItem is IExtraDescItem extraItem)
                    {

                        lines.Add("   " + dataItem.Name + ": [" + extraItem.GetExtraDesc(_gameData) + "] " + dataItem.Desc);
                    }
                    else
                    {
                        lines.Add("   " + dataItem.Name + ": " + dataItem.Desc);
                    }
                }
            }
        }
    }
}
