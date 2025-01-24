using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.RefreshStores
{
    [MessagePackObject]
    public class RefreshStoresRequest : IClientUserRequest
    {
        [Key(0)] public string CharId { get; set; }
    }
}
