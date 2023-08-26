using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserCoins.Entities
{
    [MessagePackObject]
    public class UserCoinSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<UserCoinType> UserCoinTypes { get; set; }

        public UserCoinType GetUserCoinType(long idkey) { return _lookup.Get<UserCoinType>(idkey); }   
    }
}
