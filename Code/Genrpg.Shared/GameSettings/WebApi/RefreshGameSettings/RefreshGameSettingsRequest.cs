using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameSettings.WebApi.RefreshGameSettings
{
    [MessagePackObject]
    public class RefreshGameSettingsRequest : IClientUserRequest
    {
        [Key(0)] public string CharId { get; set; }
    }
}
