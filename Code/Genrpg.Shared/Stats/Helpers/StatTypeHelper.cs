using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Stats.Helpers
{
    public class StatTypeHelper : BaseEntityHelper<StatSettings,StatType>
    {
        public override long GetKey() { return EntityTypes.Stat; }
    }
}
