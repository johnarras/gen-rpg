
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Logs.Entities;

namespace Genrpg.ServerShared.Logging
{
    public class ServerLogger : ILogSystem
    {


        public ServerLogger(ServerConfig config)
        {

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
