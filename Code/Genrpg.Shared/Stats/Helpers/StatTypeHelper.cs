using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Settings.Stats;

namespace Genrpg.Shared.Stats.Helpers
{
    public class StatTypeHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.Stat; }
        public string GetDataPropertyName() { return "StatTypes"; }


        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.Get<StatSettings>(obj).Get(id);
        }
    }
}
