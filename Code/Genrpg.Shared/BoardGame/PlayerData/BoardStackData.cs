using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Interfaces;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    [MessagePackObject]
    public class BoardStackData : NoChildPlayerData, IUserData, IServerOnlyData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<BoardData> Boards { get; set; } = new List<BoardData>();
    }


    [MessagePackObject]
    public class BoardStackLoader : UnitDataLoader<BoardStackData> { }

    [MessagePackObject]
    public class BoardStackMapper : UnitDataMapper<BoardStackData> { }
}
