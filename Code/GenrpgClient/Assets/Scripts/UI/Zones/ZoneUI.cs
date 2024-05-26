using ClientEvents;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.WorldData;
using System.Threading;

public class ZoneUI : BaseBehaviour
{
    public GText LocationName;

    private IZoneStateController _zoneStateController = null;

    private CancellationToken _token;
    public void Init(CancellationToken token)
    {
        _token = token;
        _dispatcher.AddEvent<SetZoneNameEvent>(this, OnSetZoneName);
        OnSetZoneName(null);
    }

    private void OnSetZoneName(SetZoneNameEvent data)
    {
        Zone zone = _zoneStateController.GetCurrentZone();
        
        if (zone == null)
        {
            return;
        }

        string txt = "Map " + _gs.map.Id + ": " + zone.Name + " [#" + zone.IdKey + "] {Lev " + zone.Level + "}";

        _uIInitializable.SetText(LocationName, txt);
        return;
    }
}

