using UnityEngine;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.Entities;
using ClientEvents;
using UI.Screens.Constants;
using Assets.Scripts.UI.Config;
using System.Linq;
using UI.Screens.Utils;
using System.Threading;
using Assets.Scripts.Tokens;
using Cysharp.Threading.Tasks;

public class ScreenService : BaseBehaviour, IScreenService, IGameTokenService
{
    public List<ScreenLayer> Layers;

    public List<ScreenId> AllowMultiQueueScreens;

    public GameObject DragParent;

    private ScreenConfig[] _screenConfigs = null;

    private CancellationToken _gameToken;
    public void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        _gs.loc.Set<IScreenService>(this);
        SetupLayers();
        StartUpdates();
        _screenConfigs = Resources.LoadAll<ScreenConfig>("ScreenConfigs");
    }

    public void StartUpdates()
    {
        AddUpdate(LateScreenUpdate, UpdateType.Late);
    }

    private bool _haveSetupLayers = false;
    private void SetupLayers()
    {
        if (_haveSetupLayers || Layers == null)
        {
            return;
        }
        _haveSetupLayers = true;
        GameObjectUtils.DestroyAllChildren(gameObject);
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].CurrentScreen = null;
            Layers[i].ScreenQueue = new List<ActiveScreen>();
        }

        GameObjectUtils.DestroyAllChildren(gameObject);
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].LayerParent = new GameObject();
            Layers[i].LayerParent.name = Layers[i].LayerId + "Layer";
            Layers[i].Index = i;
            GameObjectUtils.AddToParent(Layers[i].LayerParent, gameObject);
            if (Layers[i].LayerId == ScreenLayers.DragItems)
            {
                DragParent = Layers[i].LayerParent;
                Canvas canvas = DragParent.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10000;
            }
        }
    }

    public virtual object GetDragParent()
    {
        return DragParent;
    }

    private void LateScreenUpdate()
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen != null || layer.CurrentLoading != null)
            {
                continue;
            }
            if (layer.ScreenQueue == null || layer.ScreenQueue.Count < 1)
            {
                continue;
            }

            ActiveScreen nextItem = layer.ScreenQueue[0];
            layer.CurrentLoading = nextItem;
            layer.ScreenQueue.RemoveAt(0);

            _assetService.LoadAssetInto(_gs, layer.LayerParent, AssetCategory.Screens, ScreenUtils.GetPrefabName(nextItem.ScreenId), OnLoadScreen, nextItem, _gameToken);
        }

    }

    private void OnLoadScreen(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        OnLoadScreenAsync(gs, url, obj, data, token).Forget();
    }

    private async UniTask OnLoadScreenAsync (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    { 
        GameObject screen = obj as GameObject;
        ActiveScreen nextItem = data as ActiveScreen;
        
        
        if (screen == null)
        {
            _gs.logger.Debug("Couldn't load screen " + url);
            return;
        }

        if (nextItem ==null)
        {
            _gs.logger.Debug("Couldn't find active screen object for new screen");
            GameObject.Destroy(screen);
            return;
        }

        ScreenLayer layer = nextItem.LayerObject as ScreenLayer;

        if (layer == null)
        {
            _gs.logger.Debug("Couldn't find active screen layer for new screen");
            GameObject.Destroy(screen);
            return;
        }


        BaseScreen bs = screen.GetComponent<BaseScreen>();

        if (bs == null)
        {
            GameObject.Destroy(screen);
            _gs.logger.Debug("Screen had no BaseScreen on it");
            return;
        }
        bs.ScreenId = nextItem.ScreenId;

        Canvas canvas = screen.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.sortingOrder = layer.Index * 10;
        }

        Camera cam = GameObjectUtils.GetComponent<Camera>(screen);
        if (cam != null)
        {
            cam.depth = 100 * (layer.Index + 1);
        }     

        nextItem.Screen = bs;

        layer.CurrentScreen = nextItem;
        layer.CurrentLoading = null;
        Analytics.Send(AnalyticsEvents.OpenScreen, nextItem.Screen.GetAnalyticsName());
        await nextItem.Screen.StartOpen(nextItem.Data, token);
        ClearAllScreensList();

        InitClient.Instance?.RemoveSplashScreen();
    }

    public void StringOpen (UnityGameState gs, string screenName, object data = null)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => ScreenUtils.GetPrefabName(x.ScreenName) == screenName);

        if (config != null)
        {
            Open(gs, config.ScreenName, data);
        }
    }

    public void Open(UnityGameState gs, ScreenId screenName, object data = null)
    {
        ScreenLayer currLayer = GetLayer(screenName);
        if (currLayer == null)
        {
            _gs.logger.Debug("Couldn't find layer for the screen " + screenName);
            return;
        }

        bool allowMultiScreens = false;
        if (AllowMultiQueueScreens != null)
        {
            allowMultiScreens = AllowMultiQueueScreens.Contains(screenName);
        }
        if (!allowMultiScreens)
        {

            List<ActiveScreen> currScreen = GetScreensNamed(gs, screenName);

            if (currScreen != null && currScreen.Count > 0)
            {
                return;
            };
            foreach (ActiveScreen screen in currLayer.ScreenQueue)
            {
                if (screen.ScreenId == screenName)
                {
                    return;
                }
            }
            if (currLayer.CurrentLoading != null && currLayer.CurrentLoading.ScreenId == screenName)
            {
                return;
            }
        }

        ActiveScreen act = new ActiveScreen();
        act.Data = data;
        act.Screen = null;
        act.LayerId = currLayer.LayerId;
        act.ScreenId = screenName;
        act.LayerObject = currLayer;

        currLayer.ScreenQueue.Add(act);
    }

    public ScreenLayer GetLayer(ScreenId screenName)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => x.ScreenName == screenName);

        ScreenLayers layerId = config?.ScreenLayer ?? ScreenLayers.Screens;

        return Layers.FirstOrDefault(x => x.LayerId == layerId);
    }


    public void Close(UnityGameState gs, ScreenId screenName)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == screenName)
            {

                BaseScreen baseScreen = layer.CurrentScreen.Screen as BaseScreen;
                if (baseScreen != null)
                {
                    baseScreen.StartClose();
                }
                else
                {
                    layer.CurrentScreen = null;
                }
                break;
            }
        }
    }

    public void FinishClose(UnityGameState gs, ScreenId screenName)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == screenName)
            {

                BaseScreen baseScreen = layer.CurrentScreen.Screen as BaseScreen;
                if (baseScreen != null)
                {
                    GameObject.Destroy(baseScreen.gameObject);
                }
                Analytics.Send(AnalyticsEvents.CloseScreen, baseScreen.GetAnalyticsName());
                layer.CurrentScreen = null;
                ClearAllScreensList();
                break;
            }
        }

    }

    public ActiveScreen GetScreen(UnityGameState gs, ScreenId screenName)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null)
            {
                continue;
            }
            if (layer.CurrentScreen.ScreenId != screenName)
            {      
                continue;
            }
            return layer.CurrentScreen;
        }
        return null;
    }

    public List<ActiveScreen> GetScreensNamed (UnityGameState gs, ScreenId screenName)
    {
        List<ActiveScreen> retval = new List<ActiveScreen>();

        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null)
            {
                continue;
            }
            if (layer.CurrentScreen.ScreenId == screenName)
            {
                retval.Add(layer.CurrentScreen);
            }
        }
        return retval;
    }

    protected void ClearAllScreensList()
    {
        _allScreens = null;
    }

    private List<ActiveScreen> _allScreens = null;
    public List<ActiveScreen> GetAllScreens(UnityGameState gs)
    {

        if (_allScreens !=null)
        {
            return _allScreens;
        }

        _allScreens = new List<ActiveScreen>();

        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null || layer.SkipInAllScreensList)
            {
                continue;
            }
            _allScreens.Add(layer.CurrentScreen);
        }
        return _allScreens;
    }


    public void CloseAll(UnityGameState gs, List<ScreenId> ignoreScreens = null)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null || layer.SkipInAllScreensList)
            {
                continue;
            }

            if (ignoreScreens != null && ignoreScreens.Contains(layer.CurrentScreen.ScreenId))
            {
                continue;
            }

            Close(gs, layer.CurrentScreen.ScreenId);
        }
    }
}