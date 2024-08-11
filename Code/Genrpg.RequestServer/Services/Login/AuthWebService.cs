
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.RequestServer.AuthCommandHandlers;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.Services.Login
{
    public class AuthWebService : IAuthWebService
    {
        private IWebServerService _webServerService = null;

        public async Task HandleAuthCommand(WebContext context, string postData, CancellationToken token)
        {
            try
            {
                WebServerCommandSet commandSet = SerializationUtils.Deserialize<WebServerCommandSet>(postData);

                foreach (IAuthCommand authCommand in commandSet.Commands)
                {
                    IAuthCommandHandler handler = _webServerService.GetAuthCommandHandler(authCommand.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, authCommand, token);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

