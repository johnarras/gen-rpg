using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using ClientEvents;
using UI.Screens.Constants;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public interface IPopupManager : IInitializable, IInjectOnLoad<IPopupManager>
{
}

public class PopupManager : BaseBehaviour, IPopupManager
{

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        _dispatcher.AddEvent<ShowLootEvent>(this, OnLootPopup);

    }
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
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