using MessagePack;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.WhoList.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.WhoList.Messages
{
    [MessagePackObject]
    public sealed class OnGetWhoList : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public List<WhoListItem> Items { get; set; } = new List<WhoListItem>();

    }
}
