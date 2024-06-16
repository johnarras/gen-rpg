
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Interfaces
{

    public interface IInjectable
    {

    }

    public interface IInitOnResolve
    {
        void Init();
    }

    // Used for services that need to have a "setup" function run at startup.
    public interface IInitializable : IInjectable
    {
        Task Initialize(CancellationToken token);
    }

    public interface IPriorityInitializable : IInitializable
    {
        int SetupPriorityAscending();
        Task PrioritySetup(CancellationToken token);
    }

    /// <summary>
    /// Use this to set up dictionaries of classes for things like handlers such that the 
    /// interface must contain some key.
    /// </summary>
    public interface ISetupDictionaryItem<T>
    {
        T GetKey();
    }

}
