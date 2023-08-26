using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Players.Constants
{
    [MessagePackObject]
    public class PlayerConstants
    {
        public const float SaveDelay = 3.0f;
    }
}
