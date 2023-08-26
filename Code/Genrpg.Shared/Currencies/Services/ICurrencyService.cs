using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Currencies.Services
{
    public interface ICurrencyService : IService
    {
        bool Add(GameState gs, Unit unit, long currencyTypeId, long quantity);
        bool Set(GameState gs, Unit unit, long currencyTypeId, long newQuantity);
    }
}
