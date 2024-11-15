﻿using Genrpg.RequestServer.AuthCommandHandlers.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Website.Messages.Signup;

namespace Genrpg.RequestServer.AuthCommandHandlers
{
    public class SignupCommandHandler : BaseAuthCommandHandler<SignupCommand>
    {
        protected override async Task InnerHandleMessage(WebContext context, SignupCommand command, CancellationToken token)
        {
            if (command == null)
            {
                context.ShowError("Login missing SignupCommand");
                return;
            }

            if (string.IsNullOrEmpty(command.Password))
            {
                context.ShowError("Missing password");
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

            EAuthResult authResult = EAuthResult.Failure;
            if (account != null)
            { 
                authResult = ExistingPasswordIsOk(account, command);
                
                if (authResult == EAuthResult.Failure)
                {
                    context.ShowError("That email is already in use");
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
                        command.ReferrerId = refAccount.Id;
                    }
                }

                Account existingShareIdAccount = (await _repoService.Search<Account>(x => x.LowerShareId == command.ShareId.ToLower())).FirstOrDefault();

                if (existingShareIdAccount != null)
                {
                    context.ShowError("That ShareId is already taken.");
                    return;
                }

                string passwordSalt = _cryptoService.GetRandomBytes();
                string passwordHash = _cryptoService.GetPasswordHash(passwordSalt, command.Password);

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

            await AfterAuthSuccess(context, account, command, authResult);

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

