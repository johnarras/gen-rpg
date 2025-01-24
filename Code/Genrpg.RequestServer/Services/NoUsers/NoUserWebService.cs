
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;

namespace Genrpg.RequestServer.Services.NoUsers
{
    public class NoUserWebService : INoUserWebService
    {
        private ILogService _logService = null;
        private IWebServerService _loginServerService = null;

        public async Task HandleNoUserRequest(WebContext context, string postData, CancellationToken token)
        {
            WebServerRequestSet commandSet = SerializationUtils.Deserialize<WebServerRequestSet>(postData);

            try
            {
                foreach (INoUserRequest comm in commandSet.Requests)
                {
                    INoUserRequestHandler handler = _loginServerService.GetNoUserCommandHandler(comm.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<IWebResponse> errors = new List<IWebResponse>();

                foreach (IWebResponse response in context.Responses)
                {
                    if (response is ErrorResponse error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.Responses.Clear();
                    context.Responses.AddRange(errors);
                    return;
                }

            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Requests.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                context.ShowError(errorMessage);
            }

            return;
        }
    }
}
