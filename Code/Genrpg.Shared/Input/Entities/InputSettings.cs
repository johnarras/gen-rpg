using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Input.Entities
{
    [MessagePackObject]
    public class InputSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<KeyComm> DefaultInputs { get; set; }
        [Key(2)] public List<ActionInput> DefaultActions { get; set; }
    }
}