using Genrpg.Shared.Login.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages.RefreshGameData
{
    public class RefreshGameSettingsCommand : ILoginCommand
    {
        public string CharId { get; set; }
    }
}
