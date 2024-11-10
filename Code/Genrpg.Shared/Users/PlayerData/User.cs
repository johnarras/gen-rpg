using MessagePack;

using Genrpg.Shared.Interfaces;
using System;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.GameSettings.PlayerData;

namespace Genrpg.Shared.Users.PlayerData
{
    public class UserFlags
    {
        public const int ChatActive = 1 << 0;
        public const int SoundActive = 1 << 1;
        public const int MusicActive = 1 << 2;
    }

    [MessagePackObject]
    public class User : NoChildPlayerData, IFilteredObject
    {
        /// <summary>
        /// Used for the id found in the relational database
        /// </summary>
        /// 
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string SessionId { get; set; }
        [Key(2)] public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [Key(3)] public string CurrCharId { get; set; }


        [Key(4)] public long Level { get; set; }

        [Key(5)] public string ClientVersion { get; set; }

        [Key(6)] public GameDataOverrideList DataOverrides { get; set; } = new GameDataOverrideList();



        [Key(7)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }


    }
}
