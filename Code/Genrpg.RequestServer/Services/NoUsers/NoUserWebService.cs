
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.RequestServer.Utils;
using Genrpg.RequestServer.NoUserCommandHandlers;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.Services.NoUsers
{
    public class NoUserWebService : INoUserWebService
    {
        private ILogService _logService = null;
        private IWebServerService _loginServerService = null;

        public async Task HandleNoUserCommand(WebContext context, string postData, CancellationToken token)
        {
            WebServerCommandSet commandSet = SerializationUtils.Deserialize<WebServerCommandSet>(postData);

            try
            {
                foreach (INoUserCommand comm in commandSet.Commands)
                {
                    INoUserCommandHandler handler = _loginServerService.GetNoUserCommandHandler(comm.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<IWebResult> errors = new List<IWebResult>();

                foreach (IWebResult result in context.Results)
                {
                    if (result is ErrorResult error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.Results.Clear();
                    context.Results.AddRange(errors);
                    return;
                }

            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Commands.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                WebUtils.ShowError(context, errorMessage);
            }

            return;
        }
    }
}
