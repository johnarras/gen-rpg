using MessagePack;
using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages.Interfaces;

namespace Genrpg.Shared.GameSettings.Messages
{
    [MessagePackObject]
    public sealed class OnRefreshGameSettings : BaseMapApiMessage
    {
    }
}
