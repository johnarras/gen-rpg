
using Assets.Scripts.UI.MainMenu;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.LoadSave.Constants;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.UI.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


public class LoadSaveScreen : BaseScreen
{
    public GButton LoadButton;
    public GButton SaveButton;
    public GButton DeleteButton;
    public List<LoadSaveButton> LoadButtons = new List<LoadSaveButton>();

    private IClientAppService _clientAppService;
    private ICrawlerService _crawlerService;

    private ILoadSaveService _loadSaveService;

    private int _currSlot = 0;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _screenService.Close(ScreenId.Loading);

        _uiService.SetButton(LoadButton, GetName(), OnClickLoad);
        _uiService.SetButton(SaveButton, GetName(), OnClickSave);
        _uiService.SetButton(DeleteButton, GetName(), OnClickDelete);
        RefreshButtons();
        SetSlot(1);
        await Task.CompletedTask;
    }

    private void RefreshButtons()
    {
        for (int i = LoadSaveConstants.MinSlot; i <= LoadSaveConstants.MaxSlot; i++)
        {
            LoadSaveButton button = LoadButtons[i - 1];

            PartyData playerData = _loadSaveService.LoadSlot<PartyData>(i);

            button.Init(this, i, playerData);

        }
    }

    public int GetCurrentSlot()
    {
        return _currSlot;
    }

    public void SetSlot(int slot)
    {
        _currSlot = slot;

        for (int i = LoadSaveConstants.MinSlot; i <= LoadSaveConstants.MaxSlot; i++)
        {
            LoadButtons[i - 1].SetHighlight(i == slot);
        }

        LoadButton.enabled = true;
    }

    private void OnClickLoad()
    {
        if (_crawlerService.LoadParty(_currSlot) != null)
        {
            StartClose();
        }
        return;
    }


    private void OnClickContinue()
    {
        
    }
    private void OnClickSave()
    {
        _crawlerService.SaveGame();
    }

    private void OnClickDelete()
    {
        _loadSaveService.Delete<PartyData>(_currSlot);
    }

    protected override void OnStartClose()
    {
        if (_screenService.GetScreen(ScreenId.Crawler) == null)
        {
            _screenService.Open(ScreenId.CrawlerMainMenu);
        }
        base.OnStartClose();
    }


}

