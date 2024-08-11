using Genrpg.Shared.DataStores.DataGroups;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Utils
{
    public class BlobUtils
    {
        public static string GetBlobContainerName(string dataCategory, string gamePrefix, string env)
        {
            if (dataCategory.ToLower() == EDataCategories.Accounts.ToString().ToLower())
            {
                return "accounts";
            }
            return (gamePrefix + env).ToLower();
        }
    }
}
