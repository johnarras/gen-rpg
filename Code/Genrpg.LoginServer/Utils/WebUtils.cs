using Genrpg.LoginServer.Core;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.Shared.Utils;
using Microsoft.Identity.Client;
using System.Collections.Generic;

namespace Genrpg.LoginServer.Utils
{
    public static class WebUtils
    {
        public static string PackageResults(List<ILoginResult> results)
        {
            return SerializationUtils.Serialize(new LoginServerResultSet() { Results = results });
        }

        public static void ShowError(LoginGameState gs, string msg)
        {
            gs.Results.Add(new ErrorResult() { Error = msg });
        }
    }
}
