using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class ClientCachedGameSettings
    {
        [Key(0)] public string TypeName { get; set; }
        [Key(1)] public DateTime ClientSaveTime { get; set; }
    }
}
