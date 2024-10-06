
using ClientEvents;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.UI.Entities;

public interface IPopupManager : IInitializable, IInjectOnLoad<IPopupManager>
{
}

public class PopupManager : BaseBehaviour, IPopupManager
{

    public override void Init()
    {
        base.Init();
        AddListener<ShowLootEvent>(OnLootPopup);

    }
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private void OnLootPopup (ShowLootEvent ldata)
    {
        if (ldata == null || ldata.Rewards == null || ldata.Rewards.Count < 1)
        {
            return;
        }

        _screenService.Open(ScreenId.Loot, ldata.Rewards);

    }


}