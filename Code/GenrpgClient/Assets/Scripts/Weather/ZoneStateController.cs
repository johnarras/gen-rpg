using System;
using System.Collections.Generic;
using Genrpg.Shared.Core.Entities;
using GEntity = UnityEngine.GameObject;
using ClientEvents;
using Genrpg.Shared.Utils;

using UI.Screens.Constants;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.ProcGen.Settings.Weather;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Players.Messages;
using System.Net;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Maps.Constants;
using System.Linq;

public struct UpdateColor
{
    public Color Current;
    public Color Target;

    public void Set(Color val)
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
}

public class ZoneStateController : BaseBehaviour, IZoneStateController
{
    private ICameraController _cameraController = null;
    private IMapTerrainManager _terrainManager = null;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;
    protected IAudioService _audioService;
    protected ICrawlerMapService _crawlerMapService;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public override void Init()
    {
        base.Init();
        RenderSettings.sun = Sun;
        AddUpdate(ZoneUpdate, UpdateType.Regular);
        _dispatcher.AddEvent<OnFinishLoadPlayer>(this, OnFinishLoadingPlayer);
        ResetColors();
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
    public WindZone Wind;

    public Light Sun;
    public Material SkyboxMaterial;

  
    public float LinearFogEnd = 300;


    DateTime windBurstEnd = DateTime.UtcNow;
    DateTime nextWindBurst = DateTime.UtcNow;


    public const float WeatherTransitionTime = 20.0f;
    WeatherType Weather;
    public WeatherType DataWeather = new WeatherType();

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
        CloudColor.Set(GColor.gray);
        SkyColor.Set(GColor.cyan);
        SunlightColor.Set(GColor.white);
        AmbientColor.Set(GColor.white);
        FogColor.Set(GColor.gray);
        RenderSettings.fog = true;
        SetupSkybox();
        
    }

    public void SetupSkybox()
    {
        RenderSettings.skybox = SkyboxMaterial;
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", Color.white*2);
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

    private bool InCrawlerMode() { return CrawlerMapService.MapType != ECrawlerMapTypes.None; }

    private bool _didInitZoneState = false;
    private void ZoneUpdate()
    {

        if (!_didInitZoneState)
        {
            GEntity go = _playerManager.GetEntity();
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
            GEntity go = _playerManager.GetEntity();
            if (go != null)
            {
                int wx = (int)go.transform().localPosition.x;
                int wy = (int)go.transform().localPosition.z;

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
                        this.DataWeather = _gameData.Get<WeatherTypeSettings>(_gs.ch).Get(_currentZoneType.WeatherTypeId);

                    }
                }
            }
            else if (InCrawlerMode())
            {

                IReadOnlyList<WeatherType> weatherTypes = _gameData.Get<WeatherTypeSettings>(_gs.ch).GetData();
                ECrawlerMapTypes mapType = CrawlerMapService.MapType;

                if (mapType == ECrawlerMapTypes.Dungeon)
                {
                    DataWeather = weatherTypes.FirstOrDefault(x => x.Name == "CrawlerDungeon");
                }
                else if (mapType == ECrawlerMapTypes.City)
                {
                    DataWeather = weatherTypes.FirstOrDefault(x => x.Name == "CrawlerCity");
                }
                else if (mapType == ECrawlerMapTypes.Outdoors)
                {
                    DataWeather = weatherTypes.FirstOrDefault(x => x.Name == "CrawlerOutdoors");
                }

                if (DataWeather == null)
                {
                    DataWeather = weatherTypes.FirstOrDefault(x => x.IdKey > 0);
                }
            }

            if (DataWeather == null)
            {
                return;
            }

            SunlightColor.Target = TextureUtils.ConvertMyColorToColor(DataWeather.LightColor);
            FogColor.Target = TextureUtils.ConvertMyColorToColor(DataWeather.FogColor);
            CloudColor.Target = TextureUtils.ConvertMyColorToColor(DataWeather.CloudColor);
            AmbientColor.Target = TextureUtils.ConvertMyColorToColor(DataWeather.AmbientColor);
            SkyColor.Target = TextureUtils.ConvertMyColorToColor(DataWeather.SkyColor);

            FogDensity.Target = DataWeather.FogScale;
            CloudSpeed.Target = DataWeather.CloudSpeed;
            CloudDensity.Target = DataWeather.CloudScale;
            PrecipScale.Target = DataWeather.PrecipScale;
            WindScale.Target = DataWeather.WindScale;
            ParticleScale.Target = DataWeather.ParticleScale;

            SunlightIntensity.Target = DataWeather.LightScale;
            if (SunlightIntensityMultiplier > 0)
            {
                SunlightIntensity.Target *= SunlightIntensityMultiplier;
            }

            FogStart.Target = DataWeather.FogDistance;
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
        AmbientColor.Current = TextureUtils.MoveCurrToTargetColor(AmbientColor.Current, AmbientColor.Target * AmbientScale, delta);
        FogColor.Current = TextureUtils.MoveCurrToTargetColor(FogColor.Current, FogColor.Target, delta);
        SunlightColor.Current = TextureUtils.MoveCurrToTargetColor(SunlightColor.Current, SunlightColor.Target, delta);
        SkyColor.Current = TextureUtils.MoveCurrToTargetColor(SkyColor.Current, SkyColor.Target, delta);
        CloudColor.Current = TextureUtils.MoveCurrToTargetColor(CloudColor.Current, CloudColor.Target, delta);
        FogDensity.Current = TextureUtils.MoveCurrFloatToTarget(FogDensity.Current, FogDensity.Target*fogDensityMult, delta * 0.01f);

        if (_cameraController != null)
        {
            List<Camera> allCams = _cameraController.GetAllCameras();
            foreach (Camera cam in allCams)
            {
                cam.backgroundColor = SkyColor.Current;
            }
        }

        FogStart.Current = TextureUtils.MoveCurrFloatToTarget(FogStart.Current, FogStart.Target * FogDistScale, FogDistDelta * FogDistScale);
        FogEnd.Current = TextureUtils.MoveCurrFloatToTarget(FogEnd.Current, FogEnd.Target * FogDistScale, FogDistDelta * FogDistScale);

        SunlightIntensity.Current = TextureUtils.MoveCurrFloatToTarget(SunlightIntensity.Current, SunlightIntensity.Target * SunlightScale, delta);
        CloudSpeed.Current = TextureUtils.MoveCurrFloatToTarget(CloudSpeed.Current, CloudSpeed.Target, delta);
        WindScale.Current = TextureUtils.MoveCurrFloatToTarget(WindScale.Current, WindScale.Target, delta);
        PrecipScale.Current = TextureUtils.MoveCurrFloatToTarget(PrecipScale.Current, PrecipScale.Target, delta);
        ParticleScale.Current = TextureUtils.MoveCurrFloatToTarget(ParticleScale.Current, ParticleScale.Target, delta);
        CloudDensity.Current = TextureUtils.MoveCurrFloatToTarget(CloudDensity.Current, CloudDensity.Target, delta);

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
