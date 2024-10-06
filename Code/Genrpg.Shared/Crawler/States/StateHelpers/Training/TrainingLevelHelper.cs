
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{
    public class TrainingLevelHelper : BaseStateHelper
    {

        ITrainingService _trainingService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.TrainingLevel; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.WorldSpriteName = CrawlerClientConstants.TrainerImage;

            PartyData party = _crawlerService.GetParty();

            PartyMember member = action.ExtraData as PartyMember;

            TrainingInfo info = _trainingService.GetTrainingInfo(party, member);

            stateData.Actions.Add(new CrawlerStateAction($"{member.Name}: Exp for level {member.Level + 1}: {info.TotalExp}.\nYour Exp: {member.Exp}"));
            stateData.Actions.Add(new CrawlerStateAction($"Cost: {info.Cost} Party Gold: {info.PartyGold}"));
            if (info.ExpLeft < 1)
            {
                if (info.PartyGold < info.Cost)
                {
                    stateData.Actions.Add(new CrawlerStateAction("You need more gold before you can train."));
                }
                else
                {
                    stateData.Actions.Add(new CrawlerStateAction($"Train level {member.Level + 1} for {info.Cost} Gold", 'T', ECrawlerStates.TrainingLevel,
                        onClickAction: delegate ()
                        {
                            _trainingService.TrainPartyMemberLevel(party, member);
                        }, extraData: member));
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
                    stateData.Actions.Add(new CrawlerStateAction("", (char)(pm.PartySlot + '0'), ECrawlerStates.TrainingLevel, extraData: pm));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Back to the city", CharCodes.Escape, ECrawlerStates.TrainingMain));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
