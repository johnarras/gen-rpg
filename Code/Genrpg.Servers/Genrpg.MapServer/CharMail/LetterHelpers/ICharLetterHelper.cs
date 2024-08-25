using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.CharMail.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UserMail.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.CharMail.LetterHelpers
{
    public interface ICharLetterHelper : ISetupDictionaryItem<long>
    {
        Task ProcessLetter(Character ch, CharLetter letter);
    }
}
