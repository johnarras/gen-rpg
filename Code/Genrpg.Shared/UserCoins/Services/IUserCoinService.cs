using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserCoins.Services
{
    public interface IUserCoinService : IService
    {
        bool Add(GameState gs, Unit unit, long userCoinTypeId, long quantity);
        bool Set(GameState gs, Unit unit, long userCoinTypeId, long newQuantity);
    }
}
