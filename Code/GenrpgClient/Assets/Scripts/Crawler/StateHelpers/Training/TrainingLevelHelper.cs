using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services.Training;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Combat.Utils;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Training
{
    public class TrainingLevelHelper : BaseStateHelper
    {

        ITrainingService _trainingService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.TrainingLevel; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            PartyMember member = action.ExtraData as PartyMember;

            TrainingInfo info = _trainingService.GetTrainingInfo(party, member);

            stateData.Actions.Add(new CrawlerStateAction($"{member.Name}: Exp for level {member.Level + 1}: {info.TotalExp}.\nYour Exp: {member.Exp}"));
            stateData.Actions.Add(new CrawlerStateAction($"Cost: {info.Cost} Party Gold: {info.PartyGold}"));
            if (info.ExpLeft < 1)
            {
                stateData.Actions.Add(new CrawlerStateAction($"Cost: {info.Cost} Party Gold: {info.PartyGold}"));
                if (info.PartyGold < info.Cost)
                {
                    stateData.Actions.Add(new CrawlerStateAction("You need more gold before you can train."));
                }
                else
                {
                    stateData.Actions.Add(new CrawlerStateAction($"Train level {member.Level + 1} for {info.Cost} Gold", KeyCode.T, ECrawlerStates.TrainingLevel,
                        onClickAction: delegate()
                        {
                            _trainingService.TrainPartyMemberLevel(party, member);                           
                        }, extraData:member));
                }
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction($"You need {info.ExpLeft} more Exp before you can level up."));
            }

            foreach (PartyMember pm in party.GetActiveParty())
            {
                if (pm != member)
                {
                    stateData.Actions.Add(new CrawlerStateAction("", (KeyCode)(pm.PartySlot + '0'), ECrawlerStates.TrainingLevel, extraData: pm));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Back to the city", KeyCode.Escape, ECrawlerStates.TrainingMain));
            
            return stateData;
        }
    }
}
