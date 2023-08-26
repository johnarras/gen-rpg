using Genrpg.Shared.Login.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages.Error
{
    [MessagePackObject]
    public class ErrorResult : ILoginResult
    {
        [Key(0)] public string Error { get; set; }
    }
}
