using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Errors.Messages
{
    [MessagePackObject]
    public sealed class ErrorMessage : BaseMapApiMessage
    {
        [Key(0)] public string ErrorText { get; set; }

        public ErrorMessage(string txt)
        {
            ErrorText = txt;
        }
    }
}
