using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class UnitDataLoader<UD> : IUnitDataLoader where UD : class, IUnitData, new()
    {
        public virtual Type GetServerType() { return typeof(UD); }
        public virtual bool SendToClient() { return true; }
        public virtual async Task Setup(ServerGameState gs) { await Task.CompletedTask; }
        protected virtual bool IsUserData() { return false; }

        public IUnitData Create(Unit unit)
        {
            UD t = Activator.CreateInstance<UD>();
            t.Id = GetFileId(unit);
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

        public virtual async Task<IUnitData> LoadData(IRepositorySystem repoSystem, Unit unit)
        {
            return await repoSystem.Load<UD>(GetFileId(unit));
        }

        public virtual IUnitData MapToAPI(IUnitData serverObject)
        {
            return serverObject;
        }
    }
}
