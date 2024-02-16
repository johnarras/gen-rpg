
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.MapServer.MapMessaging
{
    public interface IMapMessageHandler : ISetupDictionaryItem<Type>
    {
        void Process(GameState gs, MapMessagePackage package);
        void Setup(GameState gs);
    }
}
