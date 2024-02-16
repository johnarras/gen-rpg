using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.Units.Helpers
{
    public class UnitHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.Unit; }
        public string GetDataPropertyName() { return "UnitTypes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.Get<UnitSettings>(obj).Get(id);
        }
    }
}
