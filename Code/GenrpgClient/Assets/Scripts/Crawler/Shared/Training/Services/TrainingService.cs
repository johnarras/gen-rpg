using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using TMPro;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;

namespace Genrpg.Shared.Crawler.Training.Services
{
    public class TrainingInfo
    {
        public long Cost { get; set; }
        public long PartyGold { get; set; }
        public long NextLevel { get; set; }
        public long TotalExp { get; set; }
        public long ExpLeft { get; set; }
    }

    public interface ITrainingService : IInitializable
    {
        TrainingInfo GetTrainingInfo(PartyData party, PartyMember member);
        void TrainPartyMemberLevel(PartyData party, PartyMember member);
        long GetTrainingCost(PartyMember member);
        long GetExpToLevel(PartyMember member);
    }

    public class TrainingService : ITrainingService
    {

        private ICrawlerStatService _statService = null;
        protected IGameData _gameData = null;
        protected IClientRandom _rand = null;
        protected IClientGameState _gs = null;
        private IRoguelikeUpgradeService _roguelikeUpgradeService;
        private IDispatcher _dispatcher;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public long GetTrainingCost(PartyMember member)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            long level = MathUtils.Clamp(1, member.Level, trainingSettings.MaxScalingExpLevel);

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(member.Roles);

            double goldScale = roles.Sum(x => x.TrainingGoldScale);

            if (goldScale <= 0)
            {
                goldScale = 1;
            }

            return (long)(Math.Ceiling(
                    1.0 * trainingSettings.LinearCostPerLevel * (level) +
                    trainingSettings.QuadraticCostPerLevel * (level - 1) * (level - 1)
                ));
        }

        public long GetExpToLevel(PartyMember member)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            long level = MathUtils.Clamp(1, member.Level, trainingSettings.MaxScalingExpLevel);

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(member.Roles);

            double expScale = roles.Sum(x => x.TrainingXpScale);

            if (expScale <= 0)
            {
                expScale = 1;
            }

            double killsNeeded = trainingSettings.StartKillsNeeded + trainingSettings.ExtraKillsNeeded * (level - 1);

            double monsterExp = trainingSettings.StartMonsterExp + trainingSettings.ExtraMonsterExp * (level - 1);

            double totalExp = killsNeeded * monsterExp * expScale;

            return (long)Math.Ceiling(totalExp);
        }


        public TrainingInfo GetTrainingInfo(PartyData party, PartyMember member)
        {

            CrawlerTrainingSettings settings = _gameData.Get<CrawlerTrainingSettings>(null);

            long cost = GetTrainingCost(member);

            long exp = GetExpToLevel(member);

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
                _dispatcher.Dispatch(new AddActionPanelText($"{member.Name} reaches level {member.Level}!"));
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

                    if (increment > 0)
                    {
                        _dispatcher.Dispatch(new AddActionPanelText($"+{increment} {stype.Name}"));
                    }
                }

                if (totalIncrements == 0)
                {
                    StatType stype = primaryStats[_rand.Next() % primaryStats.Count];
                    int increment = 1;
                    member.AddPermStat(stype.IdKey, increment);
                    _dispatcher.Dispatch(new AddActionPanelText($"+{increment} {stype.Name}"));
                }

                _statService.CalcUnitStats(party, member, true);

                long newPoints =  _roguelikeUpgradeService.UpdateOnLevelup(party, member.Level);

                if (newPoints > 0)
                {
                    _dispatcher.Dispatch(new AddActionPanelText($"You get {newPoints} upgrade points for a total of {party.UpgradePoints}."));
                }
            }
        }
    }
}
