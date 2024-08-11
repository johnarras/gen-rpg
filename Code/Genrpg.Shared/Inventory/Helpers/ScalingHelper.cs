using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Inventory.Helpers
{
    public class ScalingHelper : BaseEntityHelper<ScalingTypeSettings,ScalingType>
    {
        public override long GetKey() { return EntityTypes.Scaling; }
    }
}
