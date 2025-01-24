
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Auth.RequestHandlers;

namespace Genrpg.RequestServer.Services.Login
{
    public class AuthWebService : IAuthWebService
    {
        private IWebServerService _webServerService = null;

        public async Task HandleAuthRequest(WebContext context, string postData, CancellationToken token)
        {
            try
            {
                WebServerRequestSet commandSet = SerializationUtils.Deserialize<WebServerRequestSet>(postData);

                foreach (IAuthRequest authCommand in commandSet.Requests)
                {
                    IAuthRequestHandler handler = _webServerService.GetAuthCommandHandler(authCommand.GetType());

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

