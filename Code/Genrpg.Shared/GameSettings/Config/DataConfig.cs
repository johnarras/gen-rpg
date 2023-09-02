using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Config
{
    [MessagePackObject]
    public class DataConfig : BaseGameData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<ConfigItem> Items { get; set; } = new List<ConfigItem>();
        public override void Set(GameData gameData)
        {
            gameData.Set(this);
        }
    }
}
