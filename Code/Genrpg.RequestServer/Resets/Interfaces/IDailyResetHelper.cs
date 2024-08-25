using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Resets.Interfaces
{
    public interface IDailyResetHelper : IOrderedSetupDictionaryItem<Type>
    {
        Task DailyReset(WebContext context, DateTime currentResetDay, double resetHour);
    }
}
