using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{
    public interface IPlayerFilter : IIdName
    {
        long TotalModSize { get; set; }
        long MaxAcceptableModValue { get; set; }
        long MinLevel { get; set; }
        long MaxLevel { get; set; }
        long Priority { get; set; } 
        double MinDaysSinceInstall { get; set; }
        double MaxDaysSinceInstall { get; set; }
        long SpendTimes { get; set; }
        double TotalSpend { get; set; }
        bool UseDateRange { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }

    }
}
