using UnityEngine;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Entities;
using Services.ProcGen;
using UI.Screens.Constants;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using System.Linq;

public class CharacterSelectScreen : BaseScreen
{
    [SerializeField]
    private GameObject _characterGridParent;

#if UNITY_EDITOR
    [SerializeField]
    private Button _genWorldButton;
#endif
    [SerializeField]
    private Button _createButton;
    [SerializeField]
    private Button _logoutButton;
    [SerializeField]
    private Button _quitButton;

    protected IZoneGenService _zoneGenService;
    protected IClientLoginService _loginService;

    [HideInInspector]
    public const string CharacterRowArt = "CharacterSelectRow";

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
#if UNITY_EDITOR

        if (_genWorldButton == null)
        {
            GameObject genWorldObj = GameObjectUtils.FindChild(gameObject, "GenWorldButton");
            if (genWorldObj != null)
            {
                _genWorldButton = GameObjectUtils.GetComponent<Button>(genWorldObj);
            }
        }

        UIHelper.SetButton(_genWorldButton, GetAnalyticsName(), ClickGenerate);
#endif
        GameObjectUtils.DestroyAllChildren(_characterGridParent);

        UIHelper.SetButton(_logoutButton, GetAnalyticsName(), ClickLogout);
        UIHelper.SetButton(_createButton, GetAnalyticsName(), ClickCharacterCreate);
        UIHelper.SetButton(_quitButton, GetAnalyticsName(), ClickQuit);

        SetupCharacterGrid();

        GetSpellIcons(_gs);

        await UniTask.CompletedTask;
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

        GameObject selected = UIHelper.GetSelected();

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
        if (_characterGridParent == null)
        {
            return;
        }

        GameObjectUtils.DestroyAllChildren(_characterGridParent);

        foreach (CharacterStub stub in _gs.characterStubs)
        {
            _assetService.LoadAssetInto(_gs, _characterGridParent, AssetCategory.UI, CharacterRowArt, OnLoadCharacterRow, stub, _token);
        }
    }

    private void OnLoadCharacterRow(UnityGameState gs, string url, object row, object data, CancellationToken token)
    {
        GameObject go = row as GameObject;
        if (go == null)
        {
            return;
        }

        CharacterStub ch = data as CharacterStub;
        if (ch ==null)
        {
            GameObject.Destroy(go);
            return;
        }

        CharacterSelectRow charRow = go.GetComponent<CharacterSelectRow>();
        if (charRow == null)
        {
            GameObject.Destroy(go);
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

