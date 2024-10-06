using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.BoardGame.Constants;

namespace Genrpg.Shared.Users.PlayerData
{
    /// <summary>
    /// Core data about the board user
    /// </summary>
    [MessagePackObject]
    public class CoreUserData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime LastDailyReset { get; set; }

        [Key(2)] public long PlayMult { get; set; } = BoardGameConstants.MinPlayMult;

        [Key(3)] public double AvgMult { get; set; } = 1.0f;

        /// <summary>
        /// Stored here to avoid loading the activity data till we get a reward.
        /// </summary>
        [Key(4)] public long ActivityPoints { get; set; }
        /// <summary>
        /// Stored here to avoid loading ActivityData till we need to do so.
        /// </summary>
        [Key(5)] public long NextActivityReward { get; set; }

        [Key(6)] public IdValList Coins { get; set; } = new IdValList();
        [Key(7)] public IdValList Abilties { get; set; } = new IdValList();

        [Key(8)] public IdValList Stats { get; set; } = new IdValList();

        [Key(9)] public DateTime CreationDate { get; set; }

        [Key(10)] public DateTime LastHourlyReset { get; set; }

        [Key(11)] public long MarkerId { get; set; } = 1;
        [Key(12)] public long MarkerTier { get; set; } = 1;
    }

    [MessagePackObject]
    public class CoreBoardDataLoader : UnitDataLoader<CoreUserData> { }

    [MessagePackObject]
    public class CoreBoardDataMapper : UnitDataMapper<CoreUserData> { }
}
