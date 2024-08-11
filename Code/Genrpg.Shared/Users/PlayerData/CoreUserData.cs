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

namespace Genrpg.Shared.Users.PlayerData
{
    /// <summary>
    /// Core data about the board user
    /// </summary>
    [MessagePackObject]
    public class CoreUserData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime LastResetTime { get; set; }

        [Key(2)] public long EnergyMult { get; set; }

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

        [Key(8)] public long Level { get; set; } = 1;

        [Key(9)] public DateTime CreationDate { get; set; }

    }

    [MessagePackObject]
    public class CoreBoardDataLoader : UnitDataLoader<CoreUserData> { }

    [MessagePackObject]
    public class CoreBoardDataMapper : UnitDataMapper<CoreUserData> { }
}
