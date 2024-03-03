using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    [MessagePackObject]
    public class ParentSettingsApi<TParent, TChild> : StubGameSettings, ITopLevelSettings
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
    {
        [Key(0)] public List<TChild> Data { get; set; } = new List<TChild>();
        [Key(1)] public TParent ParentObj { get; set; }

        public override void AddTo(GameData gameData)
        {
            ParentObj.SetData(Data);
            gameData.Set(ParentObj);
        }
    }
}
