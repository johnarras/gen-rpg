﻿using ClientEvents;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.WorldData;
using System.Threading;

public class ZoneUI : BaseBehaviour
{
    public GText LocationName;

    private IZoneStateController _zoneStateController = null;
    protected IMapProvider _mapProvider;

    private CancellationToken _token;
    public void Init(CancellationToken token)
    {
        _token = token;
        AddListener<SetZoneNameEvent>(OnSetZoneName);
        OnSetZoneName(null);
    }

    private void OnSetZoneName(SetZoneNameEvent data)
    {
        Zone zone = _zoneStateController.GetCurrentZone();
        
        if (zone == null)
        {
            return;
        }

        Map map = _mapProvider.GetMap();

        if (map == null)
        {
            return;
        }

        string txt = "Map " + map.Id + ": " + zone.Name + " [#" + zone.IdKey + "] {Lev " + zone.Level + "}";

        _uiService.SetText(LocationName, txt);
        return;
    }
}

