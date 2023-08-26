using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class AbilityData : ObjectList<AbilityRank>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<AbilityRank> Data { get; set; } = new List<AbilityRank>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }

        public const int DefaultRank = 1;

        public int GetRank(GameState gs, long abilityCategoryId, long abilityTypeId)
        {
            AbilityRank ab = GetRankItem(gs, abilityCategoryId, abilityTypeId);
            return ab.Rank;
        }

        public void SetRank(GameState gs, long abilityCategoryId, long abilityTypeId, int rank)
        {
            AbilityRank ab = GetRankItem(gs, abilityCategoryId, abilityTypeId);
            ab.Rank = Math.Max(1, rank);
        }

        public void AddRank(GameState gs, long abilityCategoryId, long abilityTypeId, int points)
        {
            SetRank(gs, abilityCategoryId, points, GetRank(gs, abilityCategoryId, abilityTypeId) + points);
        }

        protected AbilityRank GetRankItem(GameState gs, long abilityCategoryId, long abilityTypeId)
        {
            if (Data == null)
            {
                Data = new List<AbilityRank>();
            }

            AbilityRank ab = Data.FirstOrDefault(x => x.AbilityCategoryId == abilityCategoryId && x.AbilityTypeId == abilityTypeId);
            if (ab == null)
            {
                ab = new AbilityRank() { AbilityCategoryId = abilityCategoryId, AbilityTypeId = abilityTypeId, Rank = DefaultRank };
                Data.Add(ab);
            }
            return ab;
        }

        public List<AbilityRank> GetAbilities()
        {
            if (Data == null)
            {
                Data = new List<AbilityRank>();
            }

            return Data;
        }

    }
}
