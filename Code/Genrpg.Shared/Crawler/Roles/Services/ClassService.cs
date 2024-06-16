using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.Roles.Helpers.ClassHelpers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.HelperClasses;

namespace Genrpg.Shared.Crawler.Roles.Services
{

    public class ClassService : IClassService
    {

        private SetupDictionaryContainer<long, IClassHelper> _classHelpers = new SetupDictionaryContainer<long, IClassHelper> ();
       
        public IClassHelper GetClassHelper(long classId)
        {
            if (_classHelpers.TryGetValue(classId, out IClassHelper classHelper))
            {
                return classHelper;
            }
            return null;
        }
    }
}
