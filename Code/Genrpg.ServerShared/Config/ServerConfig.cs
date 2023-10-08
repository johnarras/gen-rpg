using Genrpg.ServerShared.CloudMessaging;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Config
{
    public class ServerConfig
    {
        public string Env { get; set; }

        public string ServerId { get; set; }

        public Dictionary<string, string> ConnectionStrings { get; set; }  = new Dictionary<string, string>();

        public string GetConnectionString(string key)
        {
            if (ConnectionStrings.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

    }
}
