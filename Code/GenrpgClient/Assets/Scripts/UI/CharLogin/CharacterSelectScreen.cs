using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Entities;

using UI.Screens.Constants;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using System.Linq;

public class CharacterSelectScreen : BaseScreen
{
    
#if UNITY_EDITOR
    public GButton GenWorldButton;
#endif
    public GEntity CharacterGridParent;
    public GButton CreateButton;
    public GButton LogoutButton;
    public GButton QuitButton;

    protected IZoneGenService _zoneGenService;
    protected IClientLoginService _loginService;

    public const string CharacterRowArt = "CharacterSelectRow";

    protected override async Task OnStartOpen(object data, CancellationToken token)
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

        UIHelper.SetButton(GenWorldButton, GetAnalyticsName(), ClickGenerate);
#endif
        GEntityUtils.DestroyAllChildren(CharacterGridParent);

        UIHelper.SetButton(LogoutButton, GetAnalyticsName(), ClickLogout);
        UIHelper.SetButton(CreateButton, GetAnalyticsName(), ClickCharacterCreate);
        UIHelper.SetButton(QuitButton, GetAnalyticsName(), ClickQuit);

        SetupCharacterGrid();

        GetSpellIcons(_gs);

        await Task.CompletedTask;
    }

    private void GetSpellIcons(UnityGameState gs)
    {
    }


#if UNITY_EDITOR
    public void ClickGenerate()
    {
        if (_gs.characterStubs.Count < 1)
        {
            FloatingTextScreen.Instance?.ShowError("You need at least one character to generate a map.");
        }
        LoadIntoMapCommand lwd = new LoadIntoMapCommand()
        {
            MapId = InitClient.Instance.CurrMapId,
            CharId = _gs.characterStubs.Select(x => x.Id).FirstOrDefault(),
            GenerateMap = true,
            Env = _gs.Env,
        };
        _zoneGenService.LoadMap(_gs, lwd);
    }
#endif 

    public void ClickCharacterCreate()
    {
        _screenService.Open(_gs, ScreenId.CharacterCreate);
        _screenService.Close(_gs, ScreenId.CharacterSelect);

    }

    public void OnSelectChar()
    {
        if (!CanClick("selectchar"))
        {
            return;
        }

        CharacterStub currStub = null;

        GEntity selected = UIHelper.GetSelected();

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

    public void ClickLogout()
    {
        _loginService.Logout(_gs);
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
            _assetService.LoadAssetInto(_gs, CharacterGridParent, AssetCategory.UI, CharacterRowArt, OnLoadCharacterRow, stub, _token);
        }
    }

    private void OnLoadCharacterRow(UnityGameState gs, string url, object row, object data, CancellationToken token)
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

    public void ClickQuit()
    {
        if (!CanClick("quit"))
        {
            return;
        }
        AppUtils.Quit();
    }

}

