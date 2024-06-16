
using System;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.MapServer.Entities
{
    public interface IMapMessageHandler : ISetupDictionaryItem<Type>
    {
        void Process(IRandom rand, MapMessagePackage package);
    }
}
