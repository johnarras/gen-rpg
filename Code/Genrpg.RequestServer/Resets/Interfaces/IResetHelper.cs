using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Resets.Interfaces
{
    public interface IResetHelper : ISetupDictionaryItem<Type>
    {

        long Priority { get; }

        Task Reset(WebContext context);

    }
}
