using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.Locations;

public interface ILocationGenService : IService
{
    Location Generate(GameState gs,int locationId, int zoneId);
}



public class LocationGenService : ILocationGenService
{

    public virtual Location Generate(GameState gs,int locationId, int zoneId)
    {
        return null;
    }
}
