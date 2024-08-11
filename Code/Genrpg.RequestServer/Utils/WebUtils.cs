using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.Utils
{
    public static class WebUtils
    {
        public static string PackageResults(List<IWebResult> results)
        {
            return SerializationUtils.Serialize(new LoginServerResultSet() { Results = results });
        }

        public static void ShowError(WebContext context, string msg)
        {
            context.Results.Add(new ErrorResult() { Error = msg });
        }
    }
}
