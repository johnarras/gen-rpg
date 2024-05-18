using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.DataStores.Categories.PlayerData
{
    [DataCategory(Category = DataCategoryTypes.PlayerData)]
    public abstract class BasePlayerData : IUnitData
    {
        protected IRepositoryService _repoService;

        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }

        public virtual void AddTo(Unit unit) { unit.Set(this); }
        public virtual void SetRepo(IRepositoryService repoService)
        {
            _repoService = repoService;
        }

        public virtual void Save()
        {
            _repoService.QueueSave(this);
        }

        public virtual void Delete()
        {
            _repoService.QueueDelete(this);
        }

    }
}
