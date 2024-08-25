using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserEnergy.Messages
{
    [MessagePackObject]
    public class UpdateUserEnergyResult : IWebResult
    {
        [Key(0)] public DateTime LastHourlyReset { get; set; }
        [Key(1)] public long EnergyAdded { get; set; }
    }
}
