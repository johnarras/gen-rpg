using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Messages.Error
{
    [MessagePackObject]
    public class ErrorResponse : IWebResponse
    {
        [Key(0)] public string Error { get; set; }
    }
}
