using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.CharMail.Services
{
    public interface ICharMailService : IInjectable
    {
        Task ProcessMail(Character ch, string charLetterID);
    }
}
