
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System.Threading;

namespace Genrpg.Shared.Interfaces
{

    /// <summary>
    /// This is used to mark everything that is considered a service
    /// This will be used to turn setup into a 2 step process where all
    /// services are put into a service loc and then for each service,
    /// all other serviecs needed will be looked up at startup and
    /// then used from then on instead of using the loc
    /// </summary>
    public interface IService
    {
    }

    // Used for services that need to have a "setup" function run at startup.
    public interface ISetupService : IService
    {
        Task Setup(GameState gs, CancellationToken token);
    }

    public interface IPrioritySetupService : IService
    {
        int SetupPriorityAscending();
        Task PrioritySetup(GameState gs, CancellationToken token);
    }


    /// <summary>
    /// Use this to set up dictionaries of classes for things like handlers such that the 
    /// interface must contain some key.
    /// </summary>
    public interface ISetupDictionaryItem<T>
    {
        T GetKey();
    }

    public interface IFactorySetupService
    {
        void Setup(GameState gs);
    }

}
