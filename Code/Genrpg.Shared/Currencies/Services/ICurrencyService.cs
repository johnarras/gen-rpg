using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Currencies.Services
{
    public interface ICurrencyService : IInjectable
    {
        bool Add(Unit unit, long currencyTypeId, long quantity);
        bool Set(Unit unit, long currencyTypeId, long newQuantity);
    }
}
