using ClientEvents;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.WorldData;
using System.Threading;

public class ZoneUI : BaseBehaviour
{
    public GText LocationName;

    private CancellationToken _token;
    public void Init(CancellationToken token)
    {
        _token = token;
        _dispatcher.AddEvent<SetZoneNameEvent>(this, OnSetZoneName);
        OnSetZoneName(_gs, null);
    }

    private SetZoneNameEvent OnSetZoneName(UnityGameState gs, SetZoneNameEvent data)
    {
        GetCurrentZoneEvent sdata = _dispatcher.Dispatch(gs,new GetCurrentZoneEvent());

        if (sdata == null)
        {
            return null;
        }

        Zone zone = sdata.Zone;
        if (zone == null)
        {
            return null;
        }

        string txt = "Map " + gs.map.Id + ": " + zone.Name + " [#" + zone.IdKey + "] {Lev " + zone.Level + "}";

        _uIInitializable.SetText(LocationName, txt);
        return null;
    }
}

