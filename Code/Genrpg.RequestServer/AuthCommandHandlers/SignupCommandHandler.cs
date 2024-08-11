using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Utils;
using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Website.Messages.Signup;

namespace Genrpg.RequestServer.AuthCommandHandlers
{
    public class SignupCommandHandler : BaseAuthCommandHandler<SignupCommand>
    {
        protected override async Task InnerHandleMessage(WebContext context, SignupCommand command, CancellationToken token)
        {
            if (command == null)
            {
                WebUtils.ShowError(context, "Login missing SignupCommand");
                return;
            }

            if (string.IsNullOrEmpty(command.Password))
            {
                WebUtils.ShowError(context, "Missing password");
                return;
            }

            if (!NewPasswordIsOk(context, command.Password))
            {
                return;
            }

            if (!NameIsOk(context, command.Name))
            {
                return;
            }

            if (!EmailIsOk(context, command.Email))
            {
                return;
            }

            if (!ShareIdIsOk(context, command.ShareId))
            {
                return;
            }

            Account account = (await _repoService.Search<Account>(x => x.LowerEmail == command.Email.ToLower())).FirstOrDefault();

            if (account != null)
            {
                if (!ExistingPasswordIsOk(account, command))
                {
                    WebUtils.ShowError(context, "That email is already in use");
                    return;
                }
            }
            else if (account == null)
            {
                Account refAccount = null;

                if (!string.IsNullOrEmpty(command.ReferrerId))
                {
                    refAccount = (await _repoService.Search<Account>(x => x.LowerShareId == command.ReferrerId.ToLower())).FirstOrDefault();

                    if (refAccount == null)
                    {
                        WebUtils.ShowError(context, "No account has that ShareId. Leave blank for random ReferralId.");
                        return;
                    }
                }
                else
                {
                    List<Account> existingAccounts = await _repoService.Search<Account>(x => true);

                    if (existingAccounts.Count > 0)
                    {
                        refAccount = existingAccounts[Random.Shared.Next(existingAccounts.Count)];
                        command.ReferrerId = refAccount.Id;
                    }
                }

                Account existingShareIdAccount = (await _repoService.Search<Account>(x => x.LowerShareId == command.ShareId.ToLower())).FirstOrDefault();

                if (existingShareIdAccount != null)
                {
                    WebUtils.ShowError(context, "That ShareId is already taken.");
                    return;
                }

                string passwordSalt = PasswordUtils.GetRandomBytes();
                string passwordHash = PasswordUtils.GetPasswordHash(passwordSalt, command.Password);

                string newId = await _accountService.GetNextAccountId();
                account = new Account()
                {
                    Id = newId,
                    Name = command.Name,
                    LowerName = command.Name.ToLower(),
                    ShareId = command.ShareId,
                    LowerShareId = command.ShareId.ToLower(),
                    ReferrerAccountId = refAccount != null ? refAccount.Id : null,
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    Email = command.Email,
                    LowerEmail = command.Email.ToLower(),
                    CreatedOn = DateTime.UtcNow,
                    OriginalAccountProductId = command.AccountProductId,
                    Flags = 0,
                };

                bool success = await _repoService.Save(account);

                if (!success)
                {
                    WebUtils.ShowError(context, "Email and ShareId must be unique.");
                    return;
                }

                Account account2 = await _repoService.Load<Account>(account.Id);

                if (account2 == null)
                {
                    WebUtils.ShowError(context, "Account failed to save.");
                    return;
                }
            }

            User user = await _repoService.Load<User>(account.Id);

            if (user == null)
            {
                user = new User()
                {
                    Id = account.Id,
                    CreationDate = DateTime.UtcNow,
                };

                await _repoService.Save(user);
                user = await _repoService.Load<User>(account.Id);
            }

            if (user == null)
            {
                WebUtils.ShowError(context, "User failed to save");
                return;
            }

            await AfterAuthSuccess(context, account, command);

            return;
        }

        protected bool EmailIsOk(WebContext context, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                WebUtils.ShowError(context, "Email cannot be blank");
                return false;
            }

            int atIndex = email.IndexOf("@");
            int lastDotIndex = email.LastIndexOf(".");
            if (atIndex < 1 || lastDotIndex < 2 ||
                atIndex >= lastDotIndex ||
                lastDotIndex < atIndex + 2 ||
                lastDotIndex >= email.Length - 2)
            {
                WebUtils.ShowError(context, "This doesn't look like a valid email.");
                return false;
            }

            return true;
        }

        protected bool NameIsOk(WebContext context, string screenName)
        {
            string nameError = $"Your Name must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} characters.";
            if (string.IsNullOrEmpty(screenName)
                || screenName.Length < AccountConstants.MinNameLength
                || screenName.Length > AccountConstants.MaxNameLength)
            {
                WebUtils.ShowError(context, nameError);
                return false;
            }

            return true;
        }

        protected bool ShareIdIsOk(WebContext context, string shareId)
        {
            string shareIdError = $"Your ShareId must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} alphanumeric characters.";
            if (string.IsNullOrEmpty(shareId) ||
                shareId.Length < AccountConstants.MinShareIdLength ||
                shareId.Length > AccountConstants.MaxShareIdLength)
            {
                WebUtils.ShowError(context, shareIdError);
                return false;
            }

            return true;
        }

        protected bool NewPasswordIsOk(WebContext context, string password)
        {
            string passwordError = $"Password must be at least {AccountConstants.MinPasswordLength} characters long";
            if (string.IsNullOrEmpty(password) ||
                password.Length < AccountConstants.MinPasswordLength)
            {
                WebUtils.ShowError(context, passwordError);
                return false;
            }
            return true;
        }
    }
}

