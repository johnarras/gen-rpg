using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.RefreshGameSettings
{
    [MessagePackObject]
    public class RefreshGameSettingsCommand : IClientCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
