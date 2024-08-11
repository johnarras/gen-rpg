using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Entities.Helpers;
namespace Genrpg.Shared.Units.Helpers
{
    public class UnitHelper : BaseEntityHelper<UnitSettings,UnitType>
    {
        public override long GetKey() { return EntityTypes.Unit; }
    }
}
