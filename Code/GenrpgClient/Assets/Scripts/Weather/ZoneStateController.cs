using System;
using System.Collections.Generic;
using UnityEngine;
using ClientEvents;
using Genrpg.Shared.Utils;
using Assets.Scripts.MapTerrain;
using Genrpg.Shared.ProcGen.Settings.Weather;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using System.Linq;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.UI.Entities;

public struct UpdateColor
{
    public UnityEngine.Color Current;
    public UnityEngine.Color Target;

    public void Set(UnityEngine.Color val)
    {
        Current = val;
        Target = val;
    }
}

public struct UpdateFloat
{
    public float Current;
    public float Target;

    public void Set(float val)
    {
        Current = val;
        Target = val;
    }
}

[Serializable]
public class WeatherEffectContainer
{
    public string Name;
    public WeatherType Weather;
}

public interface IZoneStateController : IInjectable, IInjectOnLoad<IZoneStateController>
{
    Zone GetCurrentZone();
    Camera GetMainCamera();
    Light GetSun();
}

public class ZoneStateController : BaseBehaviour, IZoneStateController
{
    private ICameraController _cameraController = null;
    private IMapTerrainManager _terrainManager = null;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;
    protected IAudioService _audioService;
    protected ICrawlerMapService _crawlerMapService;
    private IModTextureService _modTextureService;

    public Camera MainCamera;
    public Light SunLight;
    public WindZone Wind;
    public Light Sun;
    public Material SkyboxMaterial;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public override void Init()
    {
        base.Init();
        RenderSettings.sun = Sun;
        AddUpdate(ZoneUpdate, UpdateType.Regular);
        AddListener<OnFinishLoadPlayer>(OnFinishLoadingPlayer);
        ResetColors();
    }

    public Camera GetMainCamera()
    {
        return MainCamera;
    }

    public Light GetSun()
    {
        return Sun;
    }
    

    public static float AmbientScale = 1.0f;
    public static float SunlightScale = 1.0f;
    public static float FogScale = 1.0f;

    public const float BaseFogStart = 150;
    public const float BaseFogEnd = 300;
    public static float FogDistScale = 1.0f;

    public static long CurrentZoneShown = -1;
    public bool PauseUpdates = false;
    public const int MaxTicksBetweenZoneUpdates = 3;

  
    public float LinearFogEnd = 300;


    DateTime windBurstEnd = DateTime.UtcNow;
    DateTime nextWindBurst = DateTime.UtcNow;


    public const float WeatherTransitionTime = 20.0f;
    WeatherType Weather;
    private WeatherType _dataWeather = null;

    public DateTime NextWeatherTransition = DateTime.UtcNow.AddSeconds(1000000);

    public float SunlightIntensityMultiplier = 1.1f;
    public float AmbientIntensityMultiplier = 0.5f;

    public const float ColorFrameDelta = 0.008f;

    public const float FogDensityDelta = 0.0005f;

    public const float FogDistDelta = 1.0f;

    int ticksToZoneUpdate = 0;

    public List<WeatherEffectContainer> Effects;

    public UpdateFloat SunlightIntensity;
    public UpdateFloat FogDensity;
    public UpdateFloat FogStart;
    public UpdateFloat FogEnd;
    public UpdateFloat PrecipScale;
    public UpdateFloat WindScale;
    public UpdateFloat ParticleScale;
    public UpdateFloat CloudDensity;
    public UpdateFloat CloudSpeed;

    public UpdateColor SkyColor;
    public UpdateColor FogColor;
    public UpdateColor SunlightColor;
    public UpdateColor CloudColor;
    public UpdateColor AmbientColor;

    Zone _currentZone;
    ZoneType _currentZoneType;


    protected WeatherType CurrentWeatherType;

    private void ResetColors()
    {
        SunlightIntensity.Set(1.0f);
        FogStart.Set(BaseFogStart);
        FogEnd.Set(BaseFogEnd);
        FogDensity.Set(0.001f);
        CloudSpeed.Set(1.0f);
        PrecipScale.Set(0.0f);
        WindScale.Set(0.0f);
        ParticleScale.Set(0.0f);
        CloudDensity.Set(0.0f);
        CloudColor.Set(Color.gray);
        SkyColor.Set(Color.cyan);
        SunlightColor.Set(Color.white);
        AmbientColor.Set(Color.white);
        FogColor.Set(Color.gray);
        RenderSettings.fog = true;
        SetupSkybox();
        
    }

    public void SetupSkybox()
    {
        RenderSettings.skybox = SkyboxMaterial;
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", UnityEngine.Color.white*2);
        }
    }
    
    public Zone GetCurrentZone()
    {
        return _currentZone;
    }

    private void OnFinishLoadingPlayer(OnFinishLoadPlayer edata)
    {
        ResetColors();
        _currentZone = null;
        _currentZoneType = null;

        return;
    }

    private bool InCrawlerMode() { return CrawlerMapService.MapType != CrawlerMapTypes.None; }

    private bool _didInitZoneState = false;
    private void ZoneUpdate()
    {

        if (!_didInitZoneState)
        {
            GameObject go = _playerManager.GetPlayerGameObject();
            if (go != null || InCrawlerMode())
            {
                ResetColors();
                _didInitZoneState = true;
            }
        }

        float delta = (InCrawlerMode() ? 1 : ColorFrameDelta);

        if (AmbientScale < 1.0f)
        {
            delta *= 2;
        }

        --ticksToZoneUpdate;
        if (ticksToZoneUpdate <= 0)
        {
            ticksToZoneUpdate = MaxTicksBetweenZoneUpdates;
            GameObject go = _playerManager.GetPlayerGameObject();
            if (go != null)
            {
                int wx = (int)go.transform.localPosition.x;
                int wy = (int)go.transform.localPosition.z;

                if (wx >= 0 && wy >= 0 && wx < _mapProvider.GetMap().GetHwid() && wy < _mapProvider.GetMap().GetHhgt())
                {

                    int gx = wx / (MapConstants.TerrainPatchSize - 1);
                    int gy = wy / (MapConstants.TerrainPatchSize - 1);


                    int zoneId = 0;
                    TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);
                    if (patch != null && patch.mainZoneIds != null)
                    {
                        wx %= (MapConstants.TerrainPatchSize - 1);
                        wy %= (MapConstants.TerrainPatchSize - 1);
                        zoneId = patch.mainZoneIds[wy, wx];
                    }

                    ActiveScreen hud = _screenService.GetScreen(ScreenId.HUD);

                    if (((_currentZone == null || _currentZone.IdKey != zoneId) && zoneId > 1) && hud != null)
                    {
                        Zone zone = _mapProvider.GetMap().Get<Zone>(zoneId);
                        if (zone == null)
                        {
                            return;
                        }
                        _currentZone = zone;
                        CurrentZoneShown = zone.IdKey;
                        _gs.ch.ZoneId = zone.IdKey;
                        _currentZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(_currentZone.ZoneTypeId);
                        _dataWeather = _gameData.Get<WeatherTypeSettings>(_gs.ch).Get(_currentZoneType.WeatherTypeId);

                    }
                }
            }
            else if (InCrawlerMode())
            {

                IReadOnlyList<WeatherType> weatherTypes = _gameData.Get<WeatherTypeSettings>(_gs.ch).GetData();
                long mapType = CrawlerMapService.MapType;

                if (_crawlerMapService.IsDungeon(mapType))
                {
                    _dataWeather = weatherTypes.FirstOrDefault(x => x.Name == "CrawlerDungeon");
                }
                else if (mapType == CrawlerMapTypes.City)
                {
                    _dataWeather = weatherTypes.FirstOrDefault(x => x.Name == "CrawlerCity");
                }
                else if (mapType == CrawlerMapTypes.Outdoors)
                {
                    _dataWeather = weatherTypes.FirstOrDefault(x => x.Name == "CrawlerOutdoors");
                }

                if (_dataWeather == null)
                {
                    _dataWeather = weatherTypes.FirstOrDefault(x => x.IdKey > 0);
                }
            }

            if (_dataWeather == null)
            {
                return;
            }

            SunlightColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.LightColor);
            FogColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.FogColor);
            CloudColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.CloudColor);
            AmbientColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.AmbientColor);
            SkyColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.SkyColor);

            FogDensity.Target = _dataWeather.FogScale;
            CloudSpeed.Target = _dataWeather.CloudSpeed;
            CloudDensity.Target = _dataWeather.CloudScale;
            PrecipScale.Target = _dataWeather.PrecipScale;
            WindScale.Target = _dataWeather.WindScale;
            ParticleScale.Target = _dataWeather.ParticleScale;

            SunlightIntensity.Target = _dataWeather.LightScale;
            if (SunlightIntensityMultiplier > 0)
            {
                SunlightIntensity.Target *= SunlightIntensityMultiplier;
            }

            FogStart.Target = _dataWeather.FogDistance;
            FogEnd.Target = LinearFogEnd;

            _audioService.PlayMusic(_currentZoneType);
            if (FogDistScale <= 1.0f)
            {
                _dispatcher.Dispatch(new SetZoneNameEvent());
            }
        }

        UpdateZoneState(delta);

        UpdateWind();

    }

    private void UpdateZoneState(float delta)
    {
        if (SunlightIntensity.Target < 0.1f)
        {
            SunlightIntensity.Target = 0.1f;
        }

        float fogDensityMult = (FogDistScale > 0 ? 1 / FogDistScale : 1.0f);
        fogDensityMult = 0;
        AmbientColor.Current = _modTextureService.MoveCurrToTargetColor(AmbientColor.Current, AmbientColor.Target * AmbientScale, delta);
        FogColor.Current = _modTextureService.MoveCurrToTargetColor(FogColor.Current, FogColor.Target, delta);
        SunlightColor.Current = _modTextureService.MoveCurrToTargetColor(SunlightColor.Current, SunlightColor.Target, delta);
        SkyColor.Current = _modTextureService.MoveCurrToTargetColor(SkyColor.Current, SkyColor.Target, delta);
        CloudColor.Current = _modTextureService.MoveCurrToTargetColor(CloudColor.Current, CloudColor.Target, delta);
        FogDensity.Current = _modTextureService.MoveCurrFloatToTarget(FogDensity.Current, FogDensity.Target*fogDensityMult, delta * 0.01f);

        if (_cameraController != null)
        {
            List<Camera> allCams = _cameraController.GetAllCameras();
            foreach (Camera cam in allCams)
            {
                cam.backgroundColor = SkyColor.Current;
            }
        }

        FogStart.Current = _modTextureService.MoveCurrFloatToTarget(FogStart.Current, FogStart.Target * FogDistScale, FogDistDelta * FogDistScale);
        FogEnd.Current = _modTextureService.MoveCurrFloatToTarget(FogEnd.Current, FogEnd.Target * FogDistScale, FogDistDelta * FogDistScale);

        SunlightIntensity.Current = _modTextureService.MoveCurrFloatToTarget(SunlightIntensity.Current, SunlightIntensity.Target * SunlightScale, delta);
        CloudSpeed.Current = _modTextureService.MoveCurrFloatToTarget(CloudSpeed.Current, CloudSpeed.Target, delta);
        WindScale.Current = _modTextureService.MoveCurrFloatToTarget(WindScale.Current, WindScale.Target, delta);
        PrecipScale.Current = _modTextureService.MoveCurrFloatToTarget(PrecipScale.Current, PrecipScale.Target, delta);
        ParticleScale.Current = _modTextureService.MoveCurrFloatToTarget(ParticleScale.Current, ParticleScale.Target, delta);
        CloudDensity.Current = _modTextureService.MoveCurrFloatToTarget(CloudDensity.Current, CloudDensity.Target, delta);

        UpdateSettings();
    }

    private void UpdateSettings()
    {
        RenderSettings.ambientSkyColor = AmbientColor.Current * AmbientIntensityMultiplier * 1.05f;
        RenderSettings.ambientEquatorColor = AmbientColor.Current * AmbientIntensityMultiplier * 0.9f;
        RenderSettings.ambientGroundColor = AmbientColor.Current * AmbientIntensityMultiplier * 0.5f;


        RenderSettings.fogColor = FogColor.Current;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = FogStart.Current;
        RenderSettings.fogEndDistance = FogEnd.Current;

        if (Sun != null)
        {
            Sun.intensity = SunlightIntensity.Current * SunlightScale * SunlightIntensityMultiplier;
            Sun.color = SunlightColor.Current;

        }

        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", FogColor.Current*0.5f);
        }
           

    }


    private void UpdateWind()
    {
        if (Wind == null)
        {
            return;
        }

        if (windBurstEnd < DateTime.UtcNow && Wind.windMain > 0)
        {
            Wind.windMain = 0.13f*Wind.windMain * WindScale.Current;
        }
        if (nextWindBurst < DateTime.UtcNow)
        {
            Wind.windMain = MathUtils.FloatRange(0.66f, 1.33f, _rand) * WindScale.Current;
            windBurstEnd = DateTime.UtcNow.AddSeconds(MathUtils.FloatRange(4.0f, 7.0f, _rand));
            nextWindBurst = DateTime.UtcNow.AddSeconds(MathUtils.FloatRange(12.0f, 22.0f, _rand));
        }
    }
}
