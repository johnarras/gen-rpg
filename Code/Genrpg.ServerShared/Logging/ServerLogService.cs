using System;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;

namespace Genrpg.ServerShared.Logging
{
    public class ServerLogService : ILogService
    {

        public async Task Initialize(GameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }


        private IServerConfig _config;

        public ServerLogService(IServerConfig config)
        {
            _config = config;
        }

        public int SetupPriorityAscending() { return SetupPriorities.Logging; }

        public async Task PrioritySetup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

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
