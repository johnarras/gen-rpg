using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.UserMail.Services
{
    public interface IUserMailService : IInjectable
    {
        Task ProcessMail(WebContext context);
    }
}
