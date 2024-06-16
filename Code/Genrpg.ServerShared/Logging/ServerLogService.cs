using System;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;

namespace Genrpg.ServerShared.Logging
{
    public class ServerLogService : ILogService
    {

        private IServerConfig _config;

        public async Task PrioritySetup(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task Initialize( CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public ServerLogService(IServerConfig config)
        {
            _config = config;
        }

        public int SetupPriorityAscending() { return SetupPriorities.Logging; }

        public void Message(string txt)
        {
            Console.WriteLine(txt);
        }
        public void Info(string txt)
        {
            Console.WriteLine(txt);
        }
        public void Warning(string txt)
        {
            Console.WriteLine(txt);
        }
        public void Debug(string txt)
        {
            Console.WriteLine(txt);
        }

        public void Error(string txt)
        {
            Console.WriteLine("Error: " + txt);
        }

        public void Exception(Exception e, string txt)
        {
            Console.WriteLine("Exception: " + txt + " " + e.Message + " " + e.StackTrace);
        }
    }
}
