using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Buildings
{
    public abstract class BuildingStateHelper : BaseStateHelper
    {
        protected override bool OnlyUseBGImage() { return true; }
    }
}
