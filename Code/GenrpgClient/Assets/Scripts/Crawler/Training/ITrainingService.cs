using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Services.Training
{
    public interface ITrainingService : IService
    {
        TrainingInfo GetTrainingInfo(GameState gs, PartyData party, PartyMember member);
        void TrainPartyMemberLevel(GameState gs, PartyData party, PartyMember member);
    }
}
