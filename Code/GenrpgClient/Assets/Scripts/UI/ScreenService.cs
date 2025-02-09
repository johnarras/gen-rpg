using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.UI.Config;
using System.Linq;
using System.Threading;
using Genrpg.Shared.UI.Settings;
using Genrpg.Shared.Analytics.Services;
using System.Threading.Tasks;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Client.Assets.Constants;
using Assets.Scripts.Awaitables;
using UnityEngine.UI;
using System;

public class ScreenService : BaseBehaviour, IScreenService, IGameTokenService, IInjectOnLoad<IScreenService>
{
    private IAnalyticsService _analyticsService;
    protected IAwaitableService _awaitableService;
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public List<ScreenLayer> Layers;

    public List<ScreenId> AllowMultiQueueScreens;

    public GameObject DragParent;

    private List<ScreenConfig> _screenConfigs = null;

    private CancellationToken _gameToken;
    public void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    public override void Init()
    {
        base.Init();
        SetupLayers();
        StartUpdates();
        _screenConfigs = _assetService.LoadAllResources<ScreenConfig>("ScreenConfigs");
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
        _clientEntityService.DestroyAllChildren(entity);
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].CurrentScreen = null;
            Layers[i].ScreenQueue = new List<ActiveScreen>();
        }

        _clientEntityService.DestroyAllChildren(entity);
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].LayerParent = new GameObject();
            Layers[i].LayerParent.name = Layers[i].LayerId + "Layer";
            Layers[i].Index = i;
            _clientEntityService.AddToParent(Layers[i].LayerParent, entity);
            if (Layers[i].LayerId == ScreenLayers.DragItems)
            {
                DragParent = Layers[i].LayerParent;
                Canvas canvas = DragParent.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10000;
            }
        }
    }
    public string GetFullScreenNameFromEnum(ScreenId id)
    {
        return (id.ToString().Replace("_", "/") + "Screen");
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

            string prefabName = GetFullScreenNameFromEnum(nextItem.ScreenId);
            string subdirectory = GetSubdirectory(nextItem.ScreenId);

            
            ScreenOverrideSettings overrideSettings = _gameData.Get<ScreenOverrideSettings>(_gs.ch);

            if (overrideSettings != null) // This will not exist during the very earliest screens, so check it.
            {
                ScreenOverride screenOverride = overrideSettings.GetData().FirstOrDefault(x => x.Name == nextItem.ScreenId.ToString());

                if (screenOverride != null && !string.IsNullOrEmpty(screenOverride.Name) && !string.IsNullOrEmpty(screenOverride.NewName))
                {
                    prefabName = prefabName.Replace(screenOverride.Name, screenOverride.NewName);
                }
            }

            _assetService.LoadAssetInto(layer.LayerParent, AssetCategoryNames.UI, 
                prefabName, OnLoadScreen, nextItem, _gameToken, subdirectory);
            
        }
    }

    private void OnLoadScreen(object obj, object data, CancellationToken token)
    {
        _awaitableService.ForgetAwaitable(OnLoadScreenAsync(obj, data, token));
    }

    private async Awaitable OnLoadScreenAsync (object obj, object data, CancellationToken token)
    { 
        GameObject screen = obj as GameObject;
        ActiveScreen nextItem = data as ActiveScreen;
        
        
        if (screen == null)
        {
            _logService.Debug("Couldn't load screen ");
            return;
        }

        if (nextItem ==null)
        {
            _logService.Debug("Couldn't find active screen object for new screen");
            _clientEntityService.Destroy(screen);
            return;
        }

        ScreenLayer layer = nextItem.LayerObject as ScreenLayer;

        if (layer == null)
        {
            _logService.Debug("Couldn't find active screen layer for new screen");
            _clientEntityService.Destroy(screen);
            return;
        }


        BaseScreen bs = screen.GetComponent<BaseScreen>();

        if (bs == null)
        {
            _clientEntityService.Destroy(screen);
            _logService.Debug("Screen had no BaseScreen on it");
            return;
        }
        bs.ScreenID = nextItem.ScreenId;
        bs.Subdirectory = GetSubdirectory(bs.ScreenID);

        Canvas canvas = screen.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.sortingOrder = layer.Index * 10;
        }

        Camera cam = _clientEntityService.GetComponent<Camera>(screen);
        if (cam != null)
        {
            cam.depth = 100 * (layer.Index + 1);
        }     

        nextItem.Screen = bs;

        layer.CurrentScreen = nextItem;
        layer.CurrentLoading = null;
        _analyticsService.Send(AnalyticsEvents.OpenScreen, nextItem.Screen.GetName());
        List<Canvas> canvases = _clientEntityService.GetComponents<Canvas>(nextItem.Screen);


        _clientEntityService.SetActive(nextItem.Screen, false);

        foreach (Canvas canvas2 in canvases)
        {
            canvas2.enabled = false;
        }

        try
        {
            await nextItem.Screen.StartOpen(nextItem.Data, nextItem.Screen.GetToken());
        }
        catch (Exception ex)
        {
            _logService.Exception(ex, "ScreenStartOpen: " + nextItem.ScreenId);
        }
        ClearAllScreensList();

        await Awaitable.NextFrameAsync(token);

        _clientEntityService.SetActive(nextItem.Screen, true);
        foreach (Canvas canvas2 in canvases)
        {
            canvas2.enabled = true;
        }
    }

    public void StringOpen (string screenName, object data = null)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => GetFullScreenNameFromEnum(x.ScreenName) == screenName);

        if (config != null)
        {
            Open(config.ScreenName, data);
        }
    }

    public void Open(ScreenId screenName, object data = null)
    {
        ScreenLayer currLayer = GetLayer(screenName);
        if (currLayer == null)
        {
            _logService.Debug("Couldn't find layer for the screen " + screenName);
            return;
        }

        bool allowMultiScreens = false;
        if (AllowMultiQueueScreens != null)
        {
            allowMultiScreens = AllowMultiQueueScreens.Contains(screenName);
        }
        if (!allowMultiScreens)
        {

            List<ActiveScreen> currScreen = GetScreensNamed(screenName);

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

    public string GetSubdirectory(ScreenId screenName)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => x.ScreenName == screenName);
        return config?.Subdirectory ?? null;
    }

    public ScreenLayer GetLayer(ScreenId screenName)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => x.ScreenName == screenName);

        ScreenLayers layerId = config?.ScreenLayer ?? ScreenLayers.Screens;

        return Layers.FirstOrDefault(x => x.LayerId == layerId);
    }


    public void Close(ScreenId screenName)
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

    public void FinishClose(ScreenId screenName)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == screenName)
            {

                BaseScreen baseScreen = layer.CurrentScreen.Screen as BaseScreen;
                if (baseScreen != null)
                {
                    _clientEntityService.Destroy(baseScreen.gameObject);
                }
                _analyticsService.Send(AnalyticsEvents.CloseScreen, baseScreen.GetName());
                layer.CurrentScreen = null;
                ClearAllScreensList();
                break;
            }
        }

    }

    public ActiveScreen GetLayerScreen(ScreenLayers layerId)
    {
        ScreenLayer layer = Layers.FirstOrDefault(x => x.LayerId == layerId);

        return layer?.CurrentScreen ?? null;
    }

    public ActiveScreen GetScreen(ScreenId screenName)
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

    public List<ActiveScreen> GetScreensNamed (ScreenId screenName)
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
    public List<ActiveScreen> GetAllScreens()
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


    public void CloseAll(List<ScreenId> ignoreScreens = null)
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

            Close(layer.CurrentScreen.ScreenId);
        }
    }

    public ActiveScreen GetScreen(string screenName)
    {
        string shortScreenName = screenName.Replace("Screen", "");

        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null)
            {
                continue;
            }
            if (layer.CurrentScreen.ScreenId.ToString() != shortScreenName)
            {
                continue;
            }
            return layer.CurrentScreen;
        }
        return null;
    }
}