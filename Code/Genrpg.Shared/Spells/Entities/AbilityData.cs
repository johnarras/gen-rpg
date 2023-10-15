using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.PlayerData;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class AbilityData : OwnerObjectList<AbilityRank>
    {
        [Key(0)] public override string Id { get; set; }

        private List<AbilityRank> _data { get; set; } = new List<AbilityRank>();

        public override List<AbilityRank> GetData()
        {
            return _data;
        }

        public override void SetData(List<AbilityRank> data)
        {
            _data = data;
        }

        public const int DefaultRank = 1;

        public int GetRank(GameState gs, long abilityCategoryId, long abilityTypeId)
        {
            AbilityRank ab = GetRankItem(gs, abilityCategoryId, abilityTypeId);
            return ab.Rank;
        }

        public void SetRank(GameState gs, long abilityCategoryId, long abilityTypeId, int rank)
        {
            AbilityRank abilityRank = GetRankItem(gs, abilityCategoryId, abilityTypeId);
            long oldRank = abilityRank.Rank;
            abilityRank.Rank = Math.Max(1, rank);
            if (abilityRank.Rank != oldRank)
            {
                abilityRank.SetDirty(true);
            }
        }

        public void AddRank(GameState gs, long abilityCategoryId, long abilityTypeId, int points)
        {
            SetRank(gs, abilityCategoryId, points, GetRank(gs, abilityCategoryId, abilityTypeId) + points);
        }

        protected AbilityRank GetRankItem(GameState gs, long abilityCategoryId, long abilityTypeId)
        {
            AbilityRank abilityRank = _data.FirstOrDefault(x => x.AbilityCategoryId == abilityCategoryId && x.AbilityTypeId == abilityTypeId);
            if (abilityRank == null)
            {
                abilityRank = new AbilityRank() 
                { 
                    Id = HashUtils.NewGuid(),
                    OwnerId = Id,
                    AbilityCategoryId = abilityCategoryId, 
                    AbilityTypeId = abilityTypeId, 
                    Rank = DefaultRank 
                };
                _data.Add(abilityRank);
                abilityRank.SetDirty(true);
            }
            return abilityRank;
        }
    }
}
