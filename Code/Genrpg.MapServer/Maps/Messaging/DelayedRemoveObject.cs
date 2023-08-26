using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Maps.Messaging
{
    public sealed class DelayedRemoveObject : BaseMapMessage
    {
        public string ObjectId;
    }
}