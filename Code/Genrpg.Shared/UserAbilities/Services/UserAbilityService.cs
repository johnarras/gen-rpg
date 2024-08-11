using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.UserAbilities.Services
{
    public class UserAbilityService : IUserAbilityService
    {
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}
