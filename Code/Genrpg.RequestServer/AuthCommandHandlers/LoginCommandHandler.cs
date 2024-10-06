using Genrpg.RequestServer.AuthCommandHandlers.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Website.Messages.Login;

namespace Genrpg.RequestServer.AuthCommandHandlers
{
    public class LoginCommandHandler : BaseAuthCommandHandler<LoginCommand>
    {
        protected override async Task InnerHandleMessage(WebContext context, LoginCommand command, CancellationToken token)
        {
            Account account = null;
            if (!string.IsNullOrEmpty(command.UserId))
            {
                account = await _repoService.Load<Account>(command.UserId);
                if (account == null)
                {
                    context.ShowError("That account doesn't exist.");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(command.Email))
            {
                account = (await _repoService.Search<Account>(x => x.LowerEmail == command.Email.ToLower())).FirstOrDefault();

                if (account == null)
                {
                    context.ShowError("That email isn't linked to an account.");
                    return;
                }
            }
            else
            {
                context.ShowError("You must specify a UserId or an email to log in.");
                return;
            }

            EAuthResult result = ExistingPasswordIsOk(account, command);

            if (result == EAuthResult.Failure)
            {
                context.ShowError("Login information is incorrect.");
                return;
            }

            await AfterAuthSuccess(context, account, command, result);

            await Task.CompletedTask;
        }
    }
}
