using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;


using Services;
using UnityEngine;
using UnityEngine.UI;
using ClientEvents;
using Entities;
using Genrpg.Shared.Zones.Entities;
using System.Threading;

public class ZoneUI : BaseBehaviour
{
    [SerializeField]
    private Text _locationName;

    private CancellationToken _token;
    public void Init(CancellationToken token)
    {
        _token = token;
        _gs.AddEvent<SetZoneNameEvent>(this, OnSetZoneName);
        OnSetZoneName(_gs, null);
    }

    private SetZoneNameEvent OnSetZoneName(UnityGameState gs, SetZoneNameEvent data)
    {
        GetCurrentZoneEvent sdata = gs.Dispatch(new GetCurrentZoneEvent());

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

        UIHelper.SetText(_locationName, txt);
        return null;
    }
}

