using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.Stats.Helpers
{
    public class StatPctHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.StatPct; }
        public string GetDataPropertyName() { return "StatTypes"; }


        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<StatSettings>().GetStatType(id);
        }
    }
}
