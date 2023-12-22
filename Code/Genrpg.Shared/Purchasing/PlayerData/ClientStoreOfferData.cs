using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class ClientStoreOfferData : IUnitData
    {
        [Key(0)] public string Id { get; set; }

        [Key(1)] public List<ClientStoreOffer> Offers { get; set; }

        public void AddTo(Unit unit)
        {
            unit.Set(this);
        }

        public void Delete(IRepositorySystem repoSystem)
        {
            return;
        }

        public List<BasePlayerData> GetSaveObjects(bool saveClean)
        {
            return new List<BasePlayerData>();
        }

        public bool IsDirty()
        {
            return false;
        }

        public void Save(IRepositorySystem repoSystem, bool saveClean)
        {
            return;
        }

        public void SetDirty(bool val)
        {
            return;
        }
    }
}
