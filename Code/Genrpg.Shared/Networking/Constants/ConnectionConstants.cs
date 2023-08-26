using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.Constants
{
    [MessagePackObject]
    public class ConnectionConstants
    {
        public const int StartBufSize = 65536;
        public const int HeaderSize = 4;
        public const int TimeoutMS = 5000;

        public const int TimeoutCheckMS = 8000;
    }
}
