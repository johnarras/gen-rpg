using MessagePack;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages.RefreshGameSettings
{
    [MessagePackObject]
    public class RefreshGameSettingsCommand : IClientCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
