using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    public class UnitDataLoader<UD> : IUnitDataLoader where UD : class, IUnitData, new()
    {
        public virtual Type GetServerType() { return typeof(UD); }
        public virtual bool SendToClient() { return true; }
        public virtual async Task Setup(GameState gs) { await Task.CompletedTask; }
        protected virtual bool IsUserData() { return false; }

        public IUnitData Create(Unit unit)
        {
            UD t = Activator.CreateInstance<UD>();
            t.Id = GetFileId(unit);
            t.SetDirty(true);
            return t;
        }

        protected virtual string GetFileId(Unit unit)
        {
            if (!IsUserData())
            {
                return unit.Id;
            }
            if (unit is Character ch)
            {
                return ch.UserId;
            }
            return unit.Id;
        }

        public virtual async Task<IUnitData> LoadData(IRepositoryService repoSystem, Unit unit)
        {
            return await repoSystem.Load<UD>(GetFileId(unit));
        }

        public virtual IUnitData MapToAPI(IUnitData serverObject)
        {
            return serverObject;
        }
    }
}
