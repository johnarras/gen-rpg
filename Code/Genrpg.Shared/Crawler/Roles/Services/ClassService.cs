using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.Roles.Helpers.ClassHelpers;

namespace Genrpg.Shared.Crawler.Roles.Services
{

    public class ClassService : IClassService
    {

        private Dictionary<long, IClassHelper> _classHelpers = new Dictionary<long, IClassHelper>();
        public async Task Setup(GameState gs, CancellationToken token)
        {
            _classHelpers = ReflectionUtils.SetupDictionary<long, IClassHelper>(gs);
        }

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
