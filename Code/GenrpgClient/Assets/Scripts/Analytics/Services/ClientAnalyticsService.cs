﻿using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ClientAnalyticsService : IAnalyticsService
{

    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private ClientConfig _config;
    public ClientAnalyticsService (ClientConfig config)
    {
        _config = config;
    }

    public void Send(GameState gs, string eventId, string eventType, string eventSubtype, Dictionary<string,string> extraData = null)
    {
    }

}