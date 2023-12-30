using Genrpg.Shared.Login.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages.RefreshStores
{
    [MessagePackObject]
    public class RefreshStoresCommand : ILoginCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
