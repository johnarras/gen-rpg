
using System;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.Shared.MapServer.Entities
{
    public interface IMapMessageHandler : ISetupDictionaryItem<Type>
    {
        void Process(GameState gs, MapMessagePackage package);
        void Setup(GameState gs);
    }
}
