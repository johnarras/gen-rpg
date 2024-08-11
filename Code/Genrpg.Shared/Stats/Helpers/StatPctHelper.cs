using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Stats.Entities;

namespace Genrpg.Shared.Stats.Helpers
{
    /// <summary>
    /// Note: This returns a list of Stat NameValues since the pct needs it.
    /// </summary>
    public class StatPctHelper : BaseEntityHelper<StatSettings,StatType>
    {
        public override long GetKey() { return EntityTypes.StatPct; }
    }
}
