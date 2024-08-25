using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.CharMail.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.MapServer.CharMail.LetterHelpers;

namespace Genrpg.MapServer.CharMail.Services
{
    public class CharMailService : ICharMailService
    {
        SetupDictionaryContainer<long, ICharLetterHelper> _mailHelpers = new SetupDictionaryContainer<long, ICharLetterHelper>();

        protected IRepositoryService _repoService;

        public async Task ProcessMail(Character ch, string charLetterID)
        {
            await Task.CompletedTask;
        }
    }
}
