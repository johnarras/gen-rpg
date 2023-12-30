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
        double MinUserDaysSinceInstall { get; set; }
        double MaxUserDaysSinceInstall { get; set; }
        long MinPurchaseCount { get; set; }
        double MinPurchaseTotal { get; set; }
        bool UseDateRange { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        void OrderSelf();

    }
}
