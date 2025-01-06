
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Zones.WorldData;
namespace Genrpg.Shared.Zones.Helpers
{
    public class ZoneHelper : BaseMapEntityHelper<Zone>
    {
        public override long GetKey() { return EntityTypes.Zone; }
    }
}
