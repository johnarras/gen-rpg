using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.DataStores.Categories.PlayerData
{
    [DataCategory(Category = DataCategoryTypes.PlayerData)]
    public abstract class BasePlayerData : IUnitData
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }

        public virtual void AddTo(Unit unit) { unit.Set(this); }

        public virtual void QueueSave(IRepositoryService repoService)
        {
            repoService.QueueSave(this);
        }

        public virtual void QueueDelete(IRepositoryService repoService)
        {
            repoService.QueueDelete(this);
        }

    }
}
