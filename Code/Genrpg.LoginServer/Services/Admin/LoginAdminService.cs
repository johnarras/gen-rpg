
using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Categories;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Admin
{
    public class LoginAdminService : BaseAdminService, IAdminService
    {
        public override async Task OnMapUploaded(ServerGameState gs, MapUploadedAdminMessage message)
        {
            if (message.WorldDataEnv != _config.DataEnvs[DataCategoryTypes.WorldData])
            {
                return;
            }

            if (gs is LoginGameState lgs)
            {
                foreach (IClientCommandHandler handler in lgs.commandHandlers.Values)
                {
                    await handler.Reset();
                }

                foreach (INoUserCommandHandler handler in lgs.noUserCommandHandlers.Values)
                {
                    await handler.Reset();
                }
            }
        }
    }
}
