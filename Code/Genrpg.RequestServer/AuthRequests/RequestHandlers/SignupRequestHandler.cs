using Genrpg.RequestServer.AuthRequestHandlers.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.WebApi.Signup;
using Genrpg.Shared.Users.PlayerData;

namespace Genrpg.RequestServer.Auth.RequestHandlers
{
    public class SignupRequestHandler : BaseAuthRequestHandler<SignupRequest>
    {
        protected override async Task HandleRequestInternal(WebContext context, SignupRequest request, CancellationToken token)
        {
            if (request == null)
            {
                context.ShowError("Login missing SignupRequest");
                return;
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                context.ShowError("Missing password");
                return;
            }

            if (!NewPasswordIsOk(context, request.Password))
            {
                return;
            }

            if (!NameIsOk(context, request.Name))
            {
                return;
            }

            if (!EmailIsOk(context, request.Email))
            {
                return;
            }

            if (!ShareIdIsOk(context, request.ShareId))
            {
                return;
            }

            Account account = (await _repoService.Search<Account>(x => x.LowerEmail == request.Email.ToLower())).FirstOrDefault();

            EAuthResponse authResponse = EAuthResponse.Failure;
            if (account != null)
            {
                authResponse = ExistingPasswordIsOk(account, request);

                if (authResponse == EAuthResponse.Failure)
                {
                    context.ShowError("That email is already in use");
                    return;
                }
            }
            else if (account == null)
            {
                Account refAccount = null;

                if (!string.IsNullOrEmpty(request.ReferrerId))
                {
                    refAccount = (await _repoService.Search<Account>(x => x.LowerShareId == request.ReferrerId.ToLower())).FirstOrDefault();

                    if (refAccount == null)
                    {
                        context.ShowError("No account has that ShareId. Leave blank for random ReferralId.");
                        return;
                    }
                }
                else
                {
                    List<Account> existingAccounts = await _repoService.Search<Account>(x => true);

                    if (existingAccounts.Count > 0)
                    {
                        refAccount = existingAccounts[Random.Shared.Next(existingAccounts.Count)];
                        request.ReferrerId = refAccount.Id;
                    }
                }

                Account existingShareIdAccount = (await _repoService.Search<Account>(x => x.LowerShareId == request.ShareId.ToLower())).FirstOrDefault();

                if (existingShareIdAccount != null)
                {
                    context.ShowError("That ShareId is already taken.");
                    return;
                }

                string passwordSalt = _cryptoService.GetRandomBytes();
                string passwordHash = _cryptoService.GetPasswordHash(passwordSalt, request.Password);

                string newId = await _accountService.GetNextAccountId();
                account = new Account()
                {
                    Id = newId,
                    Name = request.Name,
                    LowerName = request.Name.ToLower(),
                    ShareId = request.ShareId,
                    LowerShareId = request.ShareId.ToLower(),
                    ReferrerAccountId = refAccount != null ? refAccount.Id : null,
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    Email = request.Email,
                    LowerEmail = request.Email.ToLower(),
                    CreatedOn = DateTime.UtcNow,
                    OriginalAccountProductId = request.AccountProductId,
                    Flags = 0,
                };

                bool success = await _repoService.Save(account);

                if (!success)
                {
                    context.ShowError("Email and ShareId must be unique.");
                    return;
                }

                Account account2 = await _repoService.Load<Account>(account.Id);

                if (account2 == null)
                {
                    context.ShowError("Account failed to save.");
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
                context.ShowError("User failed to save");
                return;
            }

            await AfterAuthSuccess(context, account, request, authResponse);

            return;
        }

        protected bool EmailIsOk(WebContext context, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                context.ShowError("Email cannot be blank");
                return false;
            }

            int atIndex = email.IndexOf("@");
            int lastDotIndex = email.LastIndexOf(".");
            if (atIndex < 1 || lastDotIndex < 2 ||
                atIndex >= lastDotIndex ||
                lastDotIndex < atIndex + 2 ||
                lastDotIndex >= email.Length - 2)
            {
                context.ShowError("This doesn't look like a valid email.");
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
                context.ShowError(nameError);
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
                context.ShowError(shareIdError);
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
                context.ShowError(passwordError);
                return false;
            }
            return true;
        }
    }
}

