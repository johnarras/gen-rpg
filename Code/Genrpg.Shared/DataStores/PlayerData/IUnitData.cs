using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public interface IUnitData : IStringId, IEditorMetaDataTarget
    {
        void Save();
        void Delete();
        void AddTo(Unit unit);
        void SetRepo(IRepositoryService repoService);
    }
}
