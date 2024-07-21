using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.Utils;
using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Website.Messages.Login;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.AuthCommandHandlers
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
                    WebUtils.ShowError(context, "That account doesn't exist.");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(command.Email))
            {
                account = (await _repoService.Search<Account>(x => x.LowerEmail == command.Email.ToLower())).FirstOrDefault();

                if (account == null)
                {
                    WebUtils.ShowError(context, "That email isn't linked to an account.");
                    return;
                }
            }
            else
            {
                WebUtils.ShowError(context, "You must specify a UserId or an email to log in.");
                return;
            }

            string passwordHash = PasswordUtils.GetPasswordHash(account.PasswordSalt, command.Password);

            if (passwordHash != account.PasswordHash)
            {
                WebUtils.ShowError(context, "Email or password is incorrect.");
                return;
            }

            await AfterAuthSuccess(context, account, command.AccountProductId, command.ReferrerId);

            await Task.CompletedTask;
        }
    }
}
