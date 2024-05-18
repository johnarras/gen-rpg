using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Sexes.Settings;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class RollStatsHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.RollStats; }


        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(null).GetData().Where(x=>x.IdKey >=
            StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

            List<Class> memberClasses = _gameData.Get<ClassSettings>(null).GetClasses(member.Classes);



            int minStatVal = 3;
            int maxStatVal = 18;

            int rollTimes = 2;

            member.PermStats = new List<MemberStat>();
            member.Stats = new StatGroup();

            foreach (StatType st in allStats)
            {
                int valChosen = -1;

                for (int i = 0; i < rollTimes; i++)
                {
                    int currVal = MathUtils.IntRange(minStatVal, maxStatVal, gs.rand);

                    if (currVal > valChosen)
                    {
                        valChosen = currVal;
                    }
                }

                member.AddPermStat(st.IdKey, valChosen);

                stateData.Actions.Add(new CrawlerStateAction(st.Name + ": " + valChosen, KeyCode.None, ECrawlerStates.None));

            }

            stateData.Actions.Add(new CrawlerStateAction("Accept", KeyCode.A, ECrawlerStates.ChooseClass, extraData:member));

            stateData.Actions.Add(new CrawlerStateAction("Reroll", KeyCode.R, ECrawlerStates.RollStats,
                delegate { member.Stats = new StatGroup(); }, member));

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChooseRace,
                delegate { member.Stats = new StatGroup(); member.RaceId = 0; }, member));
            await UniTask.CompletedTask;
            return stateData;

        }

    }
}
