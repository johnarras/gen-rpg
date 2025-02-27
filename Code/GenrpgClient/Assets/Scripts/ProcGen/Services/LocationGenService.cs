﻿using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.Locations;
using System.Threading;
using System.Threading.Tasks;

public interface ILocationGenService : IInitializable
{
    Location Generate(IGameState gs,int locationId, int zoneId);
}



public class LocationGenService : ILocationGenService
{

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public virtual Location Generate(IGameState gs,int locationId, int zoneId)
    {
        return null;
    }
}
