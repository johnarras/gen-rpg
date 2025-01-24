using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserEnergy.WebApi
{
    [MessagePackObject]
    public class UpdateUserEnergyResponse : IWebResponse
    {
        [Key(0)] public DateTime LastHourlyReset { get; set; }
        [Key(1)] public long EnergyAdded { get; set; }
    }
}
