using MessagePack;
using System;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;

namespace Genrpg.Shared.Ftue.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class FtueData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public BitList CompletedFtues { get; set; } = new BitList();

        [Key(2)] public long CurrentFtueStepId { get; set; }

        public bool HaveCompletedFtue(long ftueId)
        {
            return CompletedFtues.HasBit(ftueId);
        }

        public void SetFtueCompleted(long ftueId)
        {
            CompletedFtues.SetBit(ftueId);
        }
    }
    [MessagePackObject]
    public class FtueDataLoader : UnitDataLoader<FtueData> { }

    [MessagePackObject]
    public class FtueDataMapper : UnitDataMapper<FtueData> { }
}
