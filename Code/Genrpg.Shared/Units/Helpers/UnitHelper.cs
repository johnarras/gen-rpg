using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.Units.Helpers
{
    public class UnitHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.Unit; }
        public string GetDataPropertyName() { return "UnitTypes"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<UnitSettings>().GetUnitType(id);
        }
    }
}
