using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;


using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.Entities;
using ClientEvents;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Threading;

public class UnitFrameContainer : BaseBehaviour
{

    public UnitFrame Frame;

    private CancellationToken _token;
    public void Init(CancellationToken token)
    {
        _token = token;
        _gs.AddEvent<SetMapPlayerEvent>(this, OnSetMapPlayer);
        _gs.AddEvent<ExitMapEvent>(this, OnExitMap);
        SetMapPlayer(PlayerObject.GetUnit());
    }

    private ExitMapEvent OnExitMap(UnityGameState gs, ExitMapEvent exitMap)
    {
        OnUpdatePlayer(exitMap.Ch);
        return null;
    }

    private SetMapPlayerEvent OnSetMapPlayer(UnityGameState gs, SetMapPlayerEvent sdata)
    {
        if (Frame == null)
        {
            return null;
        }
        OnUpdatePlayer(sdata.Ch);
        return null;
    }

    private void OnUpdatePlayer(Character ch)
    { 
        if (ch != null)
        {
            SetMapPlayer(ch);
        }
        else
        {
            SetMapPlayer(null);
        }
    }

    private void SetMapPlayer(Unit unit)
    {
        if (unit == null)
        {
            GEntityUtils.SetActive(Frame, false);
        }
        else
        {
            GEntityUtils.SetActive(Frame, true);
            Frame.Init(_gs,unit);
        }
    }
}