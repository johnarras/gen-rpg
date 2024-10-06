using UnityEngine;
using Genrpg.Shared.Characters.PlayerData;

using System.Threading;
using Genrpg.Shared.Website.Messages.LoadIntoMap;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.PlayerSearch;
using Assets.Scripts.BoardGame.Controllers;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Client.Assets.Constants;

public class CharacterSelectScreen : ErrorMessageScreen
{
    
#if UNITY_EDITOR
    public GButton GenWorldButton;
    public GButton TestAssetsButton;
#endif
    public GameObject CharacterGridParent;
    public GButton CreateButton;
    public GButton LogoutButton;
    public GButton QuitButton;
    public GButton CrawlerButton;
    public GButton BoardGameButton;
    public GText ErrorText;

    protected IZoneGenService _zoneGenService;
    protected IClientAuthService _loginService;
    protected INoiseService _noiseService;
    protected IInputService _inputService;
    protected IPlayerSearchService _playerSearchService;
    private IBoardGameController _boardGameController;
    private IClientConfigContainer _configContainer;
    private IClientAppService _clientAppService;

    public const string CharacterRowArt = "CharacterSelectRow";

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
#if UNITY_EDITOR

        if (GenWorldButton == null)
        {
            GameObject genWorldObj = (GameObject)_gameObjectService.FindChild(entity, "GenWorldButton");
            if (genWorldObj != null)
            {
                GenWorldButton = _gameObjectService.GetComponent<GButton>(genWorldObj);
            }
        }

        _uiService.SetButton(GenWorldButton, GetName(), ClickGenerate);


        if (TestAssetsButton == null)
        {
            GameObject testsAssetsObj = (GameObject)_gameObjectService.FindChild(entity, "TestAssetsButton");
            if (testsAssetsObj != null)
            {
                TestAssetsButton = _gameObjectService.GetComponent<GButton>(testsAssetsObj);
            }
        }

        _uiService.SetButton(TestAssetsButton, GetName(), ClickTestAssets);


#endif
        _gameObjectService.DestroyAllChildren(CharacterGridParent);

        _uiService.SetButton(LogoutButton, GetName(), ClickLogout);
        _uiService.SetButton(CreateButton, GetName(), ClickCharacterCreate);
        _uiService.SetButton(QuitButton, GetName(), ClickQuit);
        _uiService.SetButton(CrawlerButton, GetName(), ClickCrawler);
        _uiService.SetButton(BoardGameButton, GetName(), ClickBoardGame);

        SetupCharacterGrid();

        await Task.CompletedTask;
    }

    public override void ShowError(string errorMessage)
    {
        _uiService.SetText(ErrorText, errorMessage);
    }

#if UNITY_EDITOR

    private void ClickTestAssets()
    {
        TestAssetDownloads dl = new TestAssetDownloads();

        TaskUtils.ForgetAwaitable(dl.RunTests(_gs, _token));
    }

    private void ClickGenerate()
    {
        if (_gs.characterStubs.Count < 1)
        {
            _dispatcher.Dispatch(new ShowFloatingText("You need at least one character to generate a map.", EFloatingTextArt.Error));
        }
        LoadIntoMapCommand lwd = new LoadIntoMapCommand()
        {
            MapId = InitClient.EditorInstance.CurrMapId,
            CharId = _gs.characterStubs.Select(x => x.Id).FirstOrDefault(),
            GenerateMap = true,
            Env = _configContainer.Config.Env,
            WorldDataEnv = _assetService.GetWorldDataEnv(),
        };
        _zoneGenService.LoadMap(lwd);
    }


    private int GetIndex(int x, int y, int noiseSize)
    {
        return x + y * noiseSize;
    }

#endif


    private void ClickCrawler()
    {
        _screenService.CloseAll();
        _screenService.Open(ScreenId.Crawler);
    }

    private void ClickBoardGame()
    {
        _screenService.CloseAll();
        _screenService.Open(ScreenId.MobileHUD);
        _boardGameController.LoadCurrentBoard();
    }

    private void ClickCharacterCreate()
    {
        _screenService.Open(ScreenId.CharacterCreate);
        _screenService.Close(ScreenId.CharacterSelect);

    }



    private void OnSelectChar()
    {
        CharacterStub currStub = null;

        GameObject selected = (GameObject)_uiService.GetSelected();

        CharacterSelectRow currRow = null;

        if (selected != null)
        {
            currRow = selected.GetComponent<CharacterSelectRow>();
            if (currRow != null)
            {
                currStub = currRow.GetStub();
            }
        }

    }

    private void ClickLogout()
    {
        _loginService.Logout();
    }


    public virtual void SetupCharacterGrid()
    {
        if (CharacterGridParent == null)
        {
            return;
        }

        _gameObjectService.DestroyAllChildren(CharacterGridParent);

        foreach (CharacterStub stub in _gs.characterStubs)
        {
            _assetService.LoadAssetInto(CharacterGridParent, AssetCategoryNames.UI, 
                CharacterRowArt, OnLoadCharacterRow, stub, _token, Subdirectory);
        }
    }

    private void OnLoadCharacterRow(object row, object data, CancellationToken token)
    {
        GameObject go = row as GameObject;
        if (go == null)
        {
            return;
        }

        CharacterStub ch = data as CharacterStub;
        if (ch ==null)
        {
            _gameObjectService.Destroy(go);
            return;
        }

        CharacterSelectRow charRow = go.GetComponent<CharacterSelectRow>();
        if (charRow == null)
        {
            _gameObjectService.Destroy(go);
            return;
        }
        charRow.Init(ch, this, token);
    }

    private void ClickQuit()
    {
        _clientAppService.Quit();
    }

}

