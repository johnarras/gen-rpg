using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class RoleScalingInfoHelper : SimpleInfoHelper<RoleScalingTypeSettings, RoleScalingType>
    {
        public override long GetKey() { return EntityTypes.RoleScaling; }
    }
}
