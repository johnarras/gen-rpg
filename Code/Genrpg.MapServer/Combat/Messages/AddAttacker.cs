using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Combat.Messages
{
    public sealed class AddAttacker : BaseMapMessage
    {
        public string AttackerId { get; set; }
    }
}