using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface IGameSettings : IStringId, IEditorMetaDataTarget
    {
        void AddTo(GameData gameData);
        void SetInternalIds();
        void ClearIndex();
        Task SaveAll(IRepositoryService repo);
        List<IGameSettings> GetChildren();
        DateTime UpdateTime { get; set; }
    }
}
