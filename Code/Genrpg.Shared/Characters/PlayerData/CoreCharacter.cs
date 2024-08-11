using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.GameSettings.PlayerData;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Genrpg.Shared.Characters.PlayerData
{
    [MessagePackObject]
    public class CoreCharacter : NoChildPlayerData, ICoreCharacter
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string UserId { get; set; }
        [Key(3)] public int Version { get; set; }
        [Key(4)] public string MapId { get; set; }
        [Key(5)] public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [Key(6)] public string ClientVersion { get; set; }
        [Key(7)] public long EntityTypeId { get; set; }
        [Key(8)] public long EntityId { get; set; }
        [Key(9)] public float X { get; set; }
        [Key(10)] public float Y { get; set; }
        [Key(11)] public float Z { get; set; }
        [Key(12)] public float Rot { get; set; }
        [Key(13)] public float Speed { get; set; }
        [Key(14)] public long ZoneId { get; set; }
        [Key(15)] public long Level { get; set; } = 1;
        [Key(16)] public long FactionTypeId { get; set; }
        [Key(17)] public long AddonBits { get; set; }
        [Key(18)] public long SexTypeId { get; set; }
        [Key(19)] public GameDataOverrideList DataOverrides { get; set; } = new GameDataOverrideList();

        public string GetDocName(string settingName)
        {
            if (_overrideList == null)
            {
                return GameDataConstants.DefaultFilename;
            }
            PlayerSettingsOverrideItem item = _overrideList.Items.FirstOrDefault(x => x.SettingId == settingName);
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }


        private GameDataOverrideList _overrideList = null;

        public GameDataOverrideList GetGameDataOverrides()
        {
            return _overrideList;
        }

        public void SetGameDataOverrides(GameDataOverrideList overrideList)
        {
            _overrideList = overrideList;
        }
    }
}
