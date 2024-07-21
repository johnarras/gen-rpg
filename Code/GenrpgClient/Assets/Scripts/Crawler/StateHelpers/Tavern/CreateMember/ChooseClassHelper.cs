using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class ChooseClassHelper : BaseStateHelper
    {

        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseClass; }


        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            IReadOnlyList<Class> classes = _gameData.Get<ClassSettings>(null).GetData();

            foreach (Class cl in classes)
            {
                if (cl.IdKey < 1)
                {
                    continue;
                }

                if (member.Classes.Any(x=>x.ClassId == cl.IdKey))
                {
                    continue;
                }

                string desc = cl.Desc;


                stateData.Actions.Add(new CrawlerStateAction(cl.Name + ": " + desc, (KeyCode)char.ToLower(cl.Abbrev[0]), 
                    (member.Classes.Count < ClassConstants.MaxClasses-1 ? ECrawlerStates.ChooseClass : ECrawlerStates.ChoosePortrait), 
                    delegate
                    {
                        member.Classes.Add(new UnitClass() { ClassId = cl.IdKey });
                    }, member, null, () => { OnPointerEnter(cl); }

                    ));
            }

            if (member.Classes.Count < ClassConstants.MaxClasses)
            {

                stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.RollStats,
                    delegate
                    {
                        member.Stats = new StatGroup();
                        member.Classes.Clear();
                    },
                    extraData: member));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChooseClass,
                    delegate
                    {
                        member.Classes.RemoveAt(member.Classes.Count - 1);
                    },
                    extraData: member));
            }

            await Task.CompletedTask;
            return stateData;

        }

        private void OnPointerEnter(Class cl)
        {
            List<string> allLines = new List<string>();

            allLines.Add(cl.Name + ": " + cl.Desc);

            allLines.Add($"{cl.HealthPerLevel} Hp/Level, {cl.ManaPerLevel} Mana/Level");

            allLines.Add("Levels before an extra attack or spell hit:");
            allLines.Add($"Melee: {cl.LevelsPerMelee}, Ranged: {cl.LevelsPerRanged}, SpellDam: {cl.LevelsPerDamage}, Healing: {cl.LevelsPerHeal}");

            ShowBuffs(cl, EntityTypes.Stat, allLines, "Stats: ", _gameData.Get<StatSettings>(null).GetData(), true);
            ShowBuffs(cl, EntityTypes.PartyBuff, allLines, "Buffs: ", _gameData.Get<PartyBuffSettings>(null).GetData(), true);
            ShowBuffs(cl, EntityTypes.CrawlerSpell, allLines, "Spells: ", _gameData.Get<CrawlerSpellSettings>(null).GetData(), false); 

            _dispatcher.Dispatch(new ShowCrawlerTooltipEvent() { Lines = allLines });
        }

        private void ShowBuffs<T>(Class cl, long entityTypeId, List<string> lines, string header, IReadOnlyList<T> gameDataList, bool inOneRow) where T : IIndexedGameItem
        {
            List<ClassBonus> bonuses = cl.Bonuses.Where(x=>x.EntityTypeId == entityTypeId).ToList();   

            if (bonuses.Count < 1)
            {
                return;
            }

            List<T> dataItems = new List<T>();
            foreach (ClassBonus bonus in bonuses)
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
