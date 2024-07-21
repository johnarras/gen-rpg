using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Messages.RefreshStores
{
    [MessagePackObject]
    public class RefreshStoresCommand : IClientCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
