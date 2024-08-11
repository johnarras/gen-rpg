using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Levels.Messages;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Utils;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace Genrpg.MapServer.Levelup.Services
{
    public interface ILevelService : IInjectable
    {
        void UpdateLevel(IRandom rand, Character ch);
        void SetupLevels(GameData data);
        bool GiveLevelRewards(IRandom rand, Character ch, LevelInfo lev);

    }

    public class LevelService : ILevelService
    {
        private IRewardService _rewardService = null;
        private IStatService _statService = null;
        private IMapMessageService _messageService = null;
        private IGameData _gameData = null;

        public void UpdateLevel(IRandom rand, Character ch)
        {
            CurrencyData currencies = ch.Get<CurrencyData>();

            long startLevel = ch.Level;
            long maxLevel = _gameData.Get<LevelSettings>(ch).MaxLevel;
            long startExp = currencies.GetQuantity(CurrencyTypes.Exp);
            long currExp = startExp;
            long endLevel = startLevel;
            for (endLevel = startLevel; endLevel < maxLevel; endLevel++)
            {
                LevelInfo ldata = _gameData.Get<LevelSettings>(ch).Get(endLevel);
                if (ldata == null)
                {
                    break;
                }

                if (ldata.CurrExp > currExp)
                {
                    break;
                }

                currExp -= ldata.CurrExp;
                currExp = 0;
                ch.Level = endLevel + 1;
                NewLevel levelMessage = new NewLevel()
                {
                    Level = ch.Level,
                    UnitId = ch.Id,
                };
                _messageService.SendMessageNear(ch, levelMessage);
                GiveLevelRewards(rand, ch, ldata);
            }

            if (endLevel > startLevel)
            {
                _rewardService.Set(ch, EntityTypes.Currency, CurrencyTypes.Exp, currExp);
                _statService.CalcStats(ch, true);
            }
        }

        public virtual bool GiveLevelRewards(IRandom rand, Character ch, LevelInfo lev)
        {

            if (lev == null)
            {
                return false;
            }
            /// Don't give rewards more than once.
            if (lev.IdKey <= ch.Level)
            {
                return true;
            }

            ch.Level = lev.IdKey;

            if (lev.RewardList != null)
            {
                _rewardService.GiveRewards(rand, ch, lev.RewardList);
            }

            ch.AbilityPoints += lev.AbilityPoints;

            return true;
        }


        public void SetupLevels(GameData data)
        {
            return;
        }

        // Scale with damage player does.
        protected float StatPercentPoints(long lev)
        {
            return (float)Math.Round(1 + (0.1f + PlayerDamage(lev) * 0.027f), 1);
        }

        // Health functions

        protected long MonsterHealth(long lev)
        {
            return (long)(1.00f * (30 +
                5 * Math.Pow(lev, 1.4f) +
                0.19f * Math.Pow(lev, 2.6f) +
                1.8 * Math.Pow(2, lev / 8f)));
        }
        protected float MobDieTime(long lev)
        {
            return (float)(3.8f + lev / 17.0f);
        }

        protected long PlayerHealth(long lev)
        {
            return (long)(MonsterHealth(lev) * (1.5f + lev / 130.0f));
        }

        // Average damage functions

        protected long MonsterDamage(long lev)
        {
            return (long)(0.7f * (MonsterHealth(lev) / MobDieTime(lev)));
        }

        protected long PlayerDamage(long lev)
        {
            return (long)(MonsterHealth(lev) / MobDieTime(lev));

        }

        // Advancement functions


        protected float MobCount(long lev)
        {
            return 6 + 1.3f * lev;
        }

        protected float QuestCount(long lev)
        {
            return 1.3f + lev / 6.5f;
        }

        protected long MobExp(long lev)
        {
            return (long)(50 + 20 * lev + 2 * Math.Pow(lev, 1.5f));
        }

        protected long QuestExp(long lev)
        {
            return 100 + 75 * lev;
        }

        protected int SkillPoints(long lev)
        {
            int val = 3;
            if (lev % 5 == 1)
            {
                val += 2;
            }
            return val;
        }

        protected const int MinStatPercentPoints = 1;


    }
}
