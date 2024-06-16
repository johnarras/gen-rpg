using Genrpg.Shared.Crawler.Roles.Helpers.ClassHelpers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Roles.Services
{
    public interface IClassService : IInjectable
    {
        IClassHelper GetClassHelper(long classId);
    }
}
