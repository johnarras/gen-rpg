using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserCoins.Services
{
    public interface IUserCoinService : IInjectable
    {
        bool Add(Unit unit, long userCoinTypeId, long quantity);
        bool Set(Unit unit, long userCoinTypeId, long newQuantity);
    }
}
