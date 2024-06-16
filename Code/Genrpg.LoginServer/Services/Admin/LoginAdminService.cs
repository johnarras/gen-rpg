
using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.Services.LoginServer;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Categories;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Admin
{
    public class LoginAdminService : BaseAdminService, IAdminService
    {
        private ILoginServerService _loginServerService;
        public override async Task OnMapUploaded(MapUploadedAdminMessage message)
        {
            if (message.WorldDataEnv != _config.DataEnvs[DataCategoryTypes.WorldData])
            {
                return;
            }

            await _loginServerService.ResetCommandHandlers();

        }
    }
}
