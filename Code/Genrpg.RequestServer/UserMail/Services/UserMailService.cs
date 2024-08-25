using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.UserMail.LetterHelpers;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.UserMail.PlayerData;

namespace Genrpg.RequestServer.UserMail.Services
{
    public class UserMailService : IUserMailService
    {
        SetupDictionaryContainer<long,IUserLetterHelper> _mailHelpers = new SetupDictionaryContainer<long, IUserLetterHelper> ();

        protected IRepositoryService _repoService;

        public async Task ProcessMail(WebContext context)
        {
            List<UserLetter> letters = await _repoService.Search<UserLetter>(x => x.OwnerId == context.user.Id);

            List<Task> deleteTasks = new List<Task>();
            foreach (UserLetter letter in letters)
            {
                if (_mailHelpers.TryGetValue(letter.UserMailTypeId, out IUserLetterHelper userMailHelper))
                {
                    await userMailHelper.ProcessLetter(context, letter);
                }

                deleteTasks.Add(_repoService.Delete(letter));
            }

            await Task.WhenAll(deleteTasks);

        }
    }
}
