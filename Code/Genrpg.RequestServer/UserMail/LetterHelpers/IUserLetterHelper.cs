using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UserMail.PlayerData;

namespace Genrpg.RequestServer.UserMail.LetterHelpers
{
    public interface IUserLetterHelper : ISetupDictionaryItem<long>
    {
        Task ProcessLetter(WebContext context, UserLetter letter);
    }
}
