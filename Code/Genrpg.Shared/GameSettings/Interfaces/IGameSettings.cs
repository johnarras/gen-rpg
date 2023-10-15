using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface IGameSettings : IStringId
    {
        void AddTo(GameData gameData);
        void SetInternalIds();
        Task SaveAll(IRepositorySystem repo);
        List<IGameSettings> GetChildren();
    }
}
