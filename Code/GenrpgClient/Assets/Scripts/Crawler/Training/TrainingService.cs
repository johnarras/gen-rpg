using Assets.Scripts.ProcGen.RandomNumbers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Services.Training
{
    public class TrainingInfo
    {
        public long Cost { get; set; }
        public long PartyGold { get; set; }
        public long NextLevel { get; set; }
        public long TotalExp { get; set; }
        public long ExpLeft { get; set; }
    }


    public class TrainingService : ITrainingService
    {

        private ICrawlerStatService _statService;
        protected IGameData _gameData;
        protected IClientRandom _rand;
        protected IUnityGameState _gs;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }


        public TrainingInfo GetTrainingInfo(PartyData party, PartyMember member)
        {

            CrawlerTrainingSettings settings = _gameData.Get<CrawlerTrainingSettings>(null);

            long cost = settings.GetNextLevelTrainingCost(member.Level);
            long exp = settings.GetExpToLevel(member.Level);

            TrainingInfo info = new TrainingInfo()
            {
                Cost = cost,
                TotalExp = exp,
                ExpLeft = Math.Max(0, exp - member.Exp),
                PartyGold = party.Gold,
                NextLevel = member.Level + 1,
            };

            return info;


        }

        public void TrainPartyMemberLevel(PartyData party, PartyMember member)
        {
            TrainingInfo info = GetTrainingInfo(party, member);

            CrawlerTrainingSettings settings = _gameData.Get<CrawlerTrainingSettings>(null);

            if (info.Cost <= party.Gold && info.TotalExp <= member.Exp)
            {
                party.Gold -= info.Cost;
                member.Exp -= info.TotalExp;
                member.Level++;
                party.ActionPanel.AddText($"{member.Name} reaches level {member.Level}!");
                List<StatType> primaryStats = _gameData.Get<StatSettings>(null).GetData().Where(
                    x => x.IdKey >= StatConstants.PrimaryStatStart &&
                    x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

                long maxStat = -1;
                long minStat = 1000000;
                foreach (StatType stype in primaryStats) 
                {
                    if (member.GetPermStat(stype.IdKey) > maxStat) 
                    {
                        maxStat = member.GetPermStat(stype.IdKey);
                    }
                    if (member.GetPermStat(stype.IdKey) < minStat)
                    {
                        minStat = member.GetPermStat(stype.IdKey);  
                    }
                }

                int totalIncrements = 0;
                foreach (StatType stype in primaryStats)
                {
                    long currPermStat = member.GetPermStat(stype.IdKey);
                    long increment = 0;
                    if (currPermStat < maxStat && _rand.NextDouble() < settings.LowerStatIncreaseChance || currPermStat == minStat)
                    {
                        member.AddPermStat(stype.IdKey, 1);
                        increment++;
                        totalIncrements++;
                    }
                    if (_rand.NextDouble() < settings.MaxStatIncreaseChance)
                    {
                        member.AddPermStat(stype.IdKey, 1);
                        increment++;
                        totalIncrements++;
                    }

                    if (increment> 0)
                    {
                        party.ActionPanel.AddText($"+{increment} {stype.Name}");
                    }
                }

                if (totalIncrements == 0)
                {
                    StatType stype = primaryStats[_rand.Next() % primaryStats.Count];
                    int increment = 1;
                    member.AddPermStat(stype.IdKey, increment);
                    party.ActionPanel.AddText($"+{increment} {stype.Name}");
                }

                _statService.CalcUnitStats(party, member, true);

            }
        }
    }
}
