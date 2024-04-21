using MessagePack;

using Genrpg.Shared.Interfaces;
using System;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.GameSettings.PlayerData;

namespace Genrpg.Shared.Users.Entities
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

        [Key(4)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(5)] public GameDataOverrideList OverrideList { get; set; }
        public virtual void SetGameDataOverrides (GameDataOverrideList list)
        {
            OverrideList = list;
        }

        public virtual string GetName(string settingName)
        {
            if (OverrideList == null)
            {
                return GameDataConstants.DefaultFilename;
            }
            PlayerSettingsOverrideItem item = OverrideList.Items.FirstOrDefault(x => x.SettingId == settingName);
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }
    }
}
