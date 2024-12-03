using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Roguelikes.Services
{
    public interface IRoguelikeUpgradeService : IInjectable
    {
        double GetBonus(PartyData partyData, long roguelikeUpgradeId, long tierOverride = 0);

        bool PayForUpgrade(PartyData partyData, long roguelikeUpgradeId);

        long UpdateOnLevelup(PartyData partyData, long newLevel);

        bool ResetPoints(PartyData partyData);

        long GetLevelupPoints(long level);

        long GetUpgradeCost(long roguelikeUpgradeId, long newTier);
    }


    public class RoguelikeUpgradeService : IRoguelikeUpgradeService
    {

        private IGameData _gameData;
        private IClientGameState _gs;

        public double GetBonus(PartyData party, long roguelikeUgradeId, long tierOverride = 0)
        {

            if (party.GameMode != EGameModes.Roguelike)
            {
                return 0;
            }

            RoguelikeUpgrade upgradeSetting = _gameData.Get<RoguelikeUpgradeSettings>(_gs.ch).Get(roguelikeUgradeId);

            if (upgradeSetting == null)
            {
                return 0;
            }

            long finalTier = (tierOverride == 0 ? party.GetUpgradeTier(roguelikeUgradeId) : tierOverride);

            return upgradeSetting.BonusPerTier * finalTier;
        }


        public long GetUpgradeCost(long roguelikeUpgradeId, long newTier)
        {

            if (newTier < 1)
            {
                return 0;
            }

            RoguelikeUpgradeSettings settings = _gameData.Get<RoguelikeUpgradeSettings>(_gs.ch);

            RoguelikeUpgrade upgrade = settings.Get(roguelikeUpgradeId);

            if (upgrade == null || newTier > upgrade.MaxTier)
            {
                return 0;
            }


            return upgrade.BasePointCost * newTier;

        }

        public bool PayForUpgrade(PartyData party, long roguelikeUpgradeId)
        {

            if (party.Members.Count > 0 || party.GameMode != EGameModes.Roguelike)
            {
                return false;
            }

            long currTier = party.GetUpgradeTier(roguelikeUpgradeId);

            long nextTier = currTier + 1;


            long newCost = GetUpgradeCost(roguelikeUpgradeId, nextTier);

            if (newCost < 1)
            {
                return false;
            }

            if (newCost > party.UpgradePoints)
            {
                return false;
            }

            party.UpgradePoints -= newCost;


            party.SetUpgradeTier(roguelikeUpgradeId, nextTier);

            return true;
        }

        public long UpdateOnLevelup(PartyData partyData, long newLevel)
        {

            if (partyData.GameMode  != EGameModes.Roguelike)
            {
                return 0;
            }

            if (partyData.MaxLevel >= newLevel)
            {
                return 0;
            }

            partyData.MaxLevel = newLevel;

            long newPoints = GetLevelupPoints(newLevel);
            partyData.UpgradePoints += newPoints;
            return newPoints;
        }

        public bool ResetPoints(PartyData party)
        {
            if (party.Members.Count > 0 || party.GameMode != EGameModes.Roguelike)
            {
                return false;
            }

            party.UpgradePoints = 0;
            party.RoguelikeUpgrades.Clear();

            for (int level = 1; level <= party.MaxLevel; level++)
            {
                party.UpgradePoints += GetLevelupPoints(level);
            }

            return true;
        }

        public long GetLevelupPoints(long level)
        {
            RoguelikeUpgradeSettings settings = _gameData.Get<RoguelikeUpgradeSettings>(_gs.ch);

            if (level <= 1)
            {
                return 0;
            }

            return settings.BasePointsPerLevel + (long)(level * settings.ExtraPointsPerLevel);

        }
    }
}
