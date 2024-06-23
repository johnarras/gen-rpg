using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.PlayerData;

using UI.Screens.Constants;

using System.Threading;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using System.Linq;
using static UnityEngine.Networking.UnityWebRequest;
using UnityEngine;
using System.Threading.Tasks;

public class CharacterSelectScreen : BaseScreen
{
    
#if UNITY_EDITOR
    public GButton GenWorldButton;
    public GButton TestAssetsButton;
#endif
    public GEntity CharacterGridParent;
    public GButton CreateButton;
    public GButton LogoutButton;
    public GButton QuitButton;
    public GButton CrawlerButton;

    protected IZoneGenService _zoneGenService;
    protected IClientLoginService _loginService;
    protected INoiseService _noiseService;
    protected IInputService _inputService;

    public const string CharacterRowArt = "CharacterSelectRow";

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
#if UNITY_EDITOR

        if (GenWorldButton == null)
        {
            GEntity genWorldObj = GEntityUtils.FindChild(entity, "GenWorldButton");
            if (genWorldObj != null)
            {
                GenWorldButton = GEntityUtils.GetComponent<GButton>(genWorldObj);
            }
        }

        _uIInitializable.SetButton(GenWorldButton, GetName(), ClickGenerate);


        if (TestAssetsButton == null)
        {
            GEntity testsAssetsObj = GEntityUtils.FindChild(entity, "TestAssetsButton");
            if (testsAssetsObj != null)
            {
                TestAssetsButton = GEntityUtils.GetComponent<GButton>(testsAssetsObj);
            }
        }

        _uIInitializable.SetButton(TestAssetsButton, GetName(), ClickTestAssets);


#endif
        GEntityUtils.DestroyAllChildren(CharacterGridParent);

        _uIInitializable.SetButton(LogoutButton, GetName(), ClickLogout);
        _uIInitializable.SetButton(CreateButton, GetName(), ClickCharacterCreate);
        _uIInitializable.SetButton(QuitButton, GetName(), ClickQuit);
        _uIInitializable.SetButton(CrawlerButton, GetName(), ClickCrawler);

        SetupCharacterGrid();

        GetSpellIcons();


        await Task.CompletedTask;
    }

    private void GetSpellIcons()
    {
    }


#if UNITY_EDITOR

    private void ClickTestAssets()
    {
        TestAssetDownloads dl = new TestAssetDownloads();

        AwaitableUtils.ForgetAwaitable(dl.RunTests(_gs, _token));
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
            Env = _gs.Config.Env,
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
        _inputService.SetDisabled(true);
        _screenService.Open(ScreenId.Crawler);
    }

    private void ClickCharacterCreate()
    {
        _screenService.Open(ScreenId.CharacterCreate);
        _screenService.Close(ScreenId.CharacterSelect);

    }



    private void OnSelectChar()
    {
        CharacterStub currStub = null;

        GEntity selected = _uIInitializable.GetSelected();

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

        GEntityUtils.DestroyAllChildren(CharacterGridParent);

        foreach (CharacterStub stub in _gs.characterStubs)
        {
            _assetService.LoadAssetInto(CharacterGridParent, AssetCategoryNames.UI, 
                CharacterRowArt, OnLoadCharacterRow, stub, _token, Subdirectory);
        }
    }

    private void OnLoadCharacterRow(object row, object data, CancellationToken token)
    {
        GEntity go = row as GEntity;
        if (go == null)
        {
            return;
        }

        CharacterStub ch = data as CharacterStub;
        if (ch ==null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        CharacterSelectRow charRow = go.GetComponent<CharacterSelectRow>();
        if (charRow == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }
        charRow.Init(ch, this, token);
    }

    private void ClickQuit()
    {
        AppUtils.Quit();
    }

}

