
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System.Threading;

namespace Genrpg.Shared.Interfaces
{

    public interface IInjectable
    {

    }

    // Used for services that need to have a "setup" function run at startup.
    public interface IInitializable : IInjectable
    {
        Task Initialize(GameState gs, CancellationToken token);
    }

    public interface IPriorityInitializable : IInitializable
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
