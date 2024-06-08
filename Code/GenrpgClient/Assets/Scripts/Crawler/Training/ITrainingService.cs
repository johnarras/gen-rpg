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
    public interface ITrainingService : IInitializable
    {
        TrainingInfo GetTrainingInfo(PartyData party, PartyMember member);
        void TrainPartyMemberLevel(PartyData party, PartyMember member);
    }
}
