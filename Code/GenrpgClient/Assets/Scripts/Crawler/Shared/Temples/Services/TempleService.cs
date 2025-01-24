using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Temples.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Temples.Services
{


    public class TempleResult
    {
        public string Message;
        public bool Success;
        public long Cost;
        public PartyMember Member;
    }

    public interface ITempleService : IInjectable
    {
        long GetHealingCostForMember(PartyData partyData, PartyMember member);
        void HealPartyMember(PartyData partyData, PartyMember member, TempleResult result);
    }

    public class TempleService : ITempleService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IStatService _statService = null;

        public long GetHealingCostForMember(PartyData party, PartyMember member)
        {
            TempleSettings settings = _gameData.Get<TempleSettings>(_gs.ch);

            List<StatType> mutableStatTypes = _statService.GetMutableStatTypes(member);

            long cost = 0;

            bool missingStats = false;

            foreach (StatType statType in mutableStatTypes)
            {
                if (member.Stats.Curr(statType.IdKey) < member.Stats.Max(statType.IdKey))
                {
                    missingStats = true;
                    break;
                }
            }

            if (missingStats)
            {
                cost += settings.HealingCostPerLevel * Math.Min(member.Level, settings.MaxCostLevel);
            }

            List<IDisplayEffect> statusEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect).ToList();

            if (statusEffects.Count > 0)
            {
                long maxStatusIndex = statusEffects.Max(x => x.EntityId);

                cost += settings.StatusEffectCostPerLevel * Math.Min(member.LastCombatCrawlerSpellId, settings.MaxCostLevel);
            }

            return cost;
        }

        public void HealPartyMember(PartyData partyData, PartyMember member, TempleResult result)
        {
            result.Member = member;
            result.Cost = GetHealingCostForMember(partyData, member);
            result.Success = false;

            if (result.Cost == 0)
            {
                result.Message = member.Name + " is already fine.";
                return;
            }

            if (result.Cost > partyData.Gold)
            {
                result.Message = "You need " + result.Cost + " Gold to heal " + member.Name;
                return;
            }

            partyData.Gold -= result.Cost;

            List<StatType> mutableStatTypes = _statService.GetMutableStatTypes(member);

            foreach (StatType statType in mutableStatTypes)
            {
                member.Stats.SetCurr(statType.IdKey, member.Stats.Max(statType.IdKey));
            }

            List<IDisplayEffect> statusEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect).ToList();


            foreach (IDisplayEffect effect in statusEffects)
            {
                member.RemoveEffect(effect);
            }

            result.Success = true;
            result.Message = member.Name + " is fully healed.";
            return;
        }
    }
}
