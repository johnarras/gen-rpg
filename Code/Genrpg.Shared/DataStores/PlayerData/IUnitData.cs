using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public interface IUnitData : IStringId, IEditorMetaDataTarget
    {
        void QueueSave(IRepositoryService repoService);
        void QueueDelete(IRepositoryService repoService);
        void AddTo(Unit unit);
    }
}
