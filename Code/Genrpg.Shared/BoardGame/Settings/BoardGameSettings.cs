using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Achievements.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.BoardGame.Settings
{

    [MessagePackObject]
    public class BoardGameSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        /// <summary>
        /// On average, each unity energy gives this times the player's energy mult. (So 100 energy mult * this = 2500 credits per roll)
        /// </summary>
        [Key(1)] public double PlayCreditsMult { get; set; } = 25;
        [Key(2)] public double PlaysPerGem { get; set; } = 0.5; // A gem equals half a roll worth of gold.
        [Key(3)] public double PlayMultBonusDivisor { get; set; } = 20.0;


        /// <summary>
        /// Get the credits to gem mult. The credits per play is PlayGoldMult*creditMult
        /// So mult that by PlaysPerGem....so 0.5 = each play is 2 gems.
        /// </summary>
        /// <param name="creditsNeeded"></param>
        /// <param name="userCreditMult"></param>
        /// <returns></returns>
        public long CreditsToGemConversion(long creditsNeeded, long userCreditMult)
        {
            double creditsPerGem = PlayCreditsMult * PlaysPerGem * userCreditMult;

            return (long)Math.Ceiling(creditsNeeded / creditsPerGem);
        }
    }

    [MessagePackObject]
    public class BoardGameSettingsLoader : NoChildSettingsLoader<BoardGameSettings> { }

    [MessagePackObject]
    public class BoardGameSettingsMapper : NoChildSettingsMapper<BoardGameSettings> { }
}
