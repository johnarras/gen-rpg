using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;


using ClientEvents;

using UI.Screens.Constants;

public class PopupManager : BaseBehaviour
{

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        _gs.AddEvent<ShowLootEvent>(this, OnLootPopup);

    }

    private ShowLootEvent OnLootPopup (UnityGameState gs, ShowLootEvent ldata)
    {
        if (ldata == null || ldata.Rewards == null || ldata.Rewards.Count < 1)
        {
            return null;
        }

        _screenService.Open(gs, ScreenId.Loot, ldata.Rewards);

        return null;
    }


}