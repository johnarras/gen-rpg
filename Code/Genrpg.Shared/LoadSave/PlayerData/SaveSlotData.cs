using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.LoadSave.PlayerData
{
    [MessagePackObject]
    public class SaveSlotData : BasePlayerData
    {
        public const string Filename = "Default";

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long SlotId { get; set; }
    }
}
