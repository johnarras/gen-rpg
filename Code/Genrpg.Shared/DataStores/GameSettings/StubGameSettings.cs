using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.GameSettings
{
    [MessagePackObject]
    public class StubGameSettings : IGameSettings
    {
        [Key(0)] public string Id { get; set; }

        public virtual void SetInternalIds() { }
        public virtual void AddTo(GameData gameData) { gameData.Set(this); }

    }
}
