using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    public class UnitDataLoader<TServer> : IUnitDataLoader where TServer : class, ITopLevelUnitData, new()
    {

        protected IRepositoryService _repoService;

        public virtual Type GetServerType() { return typeof(TServer); }
        public virtual async Task Initialize(GameState g, CancellationToken token) { await Task.CompletedTask; }
        protected virtual bool IsUserData() { return false; }

        public IUnitData Create(Unit unit)
        {
            TServer t = Activator.CreateInstance<TServer>();
            t.Id = GetFileId(unit);
            t.SetRepo(_repoService);
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

        public virtual async Task<ITopLevelUnitData> LoadFullData(Unit unit)
        {
            ITopLevelUnitData tld = await _repoService.Load<TServer>(GetFileId(unit));
            tld?.SetRepo(_repoService);
            return tld;
        }

        public async Task<ITopLevelUnitData> LoadTopLevelData(Unit unit)
        {

            TServer currServer = unit.Get<TServer>();

            if (currServer != null)
            {
                return currServer;
            }

            currServer = await _repoService.Load<TServer>(GetFileId(unit));

            if (currServer != null)
            {
                unit.Set(currServer);
            }

            return currServer;
        }

        public virtual async Task<IChildUnitData> LoadChildByIdkey(Unit unit, long idkey)
        {
            await Task.CompletedTask;
            return default;
        }

        public virtual async Task<IChildUnitData> LoadChildById(Unit unit, string id)
        {
            await Task.CompletedTask;
            return default;
        }
    }
}
