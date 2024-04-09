using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Analytics.Services
{
    public interface IAnalyticsService : IService
    {
        void Send(GameState gs, string eventId, string eventType, string eventSubtype = null, Dictionary<string,string> extraData = null);
    }
}
