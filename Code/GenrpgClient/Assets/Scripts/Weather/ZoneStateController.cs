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

public class ZoneStateController : BaseBehaviour
{
    private ICameraController _cameraController = null;
    private IMapTerrainManager _terrainManager = null;


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        RenderSettings.sun = Sun;
        AddUpdate(ZoneUpdate, UpdateType.Regular);
        _gs.AddEvent<GetCurrentZoneEvent>(this, OnGetCurrentZone);
        _gs.AddEvent<OnFinishLoadPlayer>(this, OnFinishLoadingPlayer);
        ResetColors();
    }

    

    public static float AmbientScale = 1.0f;
    public static float SunlightScale = 1.0f;
    public static float FogScale = 1.0f;

    public const float BaseFogStart = 200;
    public const float BaseFogEnd = 400;
    public static float FogDistScale = 1.0f;

    public static long CurrentZoneShown = -1;
    public bool PauseUpdates = false;
    public const int MaxTicksBetweenZoneUpdates = 3;
    public WindZone Wind;

    public Light Sun;
    public Material SkyboxMaterial;

  
    public float LinearFogEnd = 400;


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
        //if (Weather == null) Weather = ScriptableObject.CreateInstance<WeatherType>();
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


    private GetCurrentZoneEvent OnGetCurrentZone(GameState gs, GetCurrentZoneEvent edata)
    {
        edata.Zone = _currentZone;
        return edata;
    }
    
    private OnFinishLoadPlayer OnFinishLoadingPlayer(GameState gs, OnFinishLoadPlayer edata)
    {
        ResetColors();
        _currentZone = null;
        _currentZoneType = null;

        return null;
    }

    private bool _didInitZoneState = false;
    IScreenService ss = null;
    private void ZoneUpdate()
    {

        if (!_didInitZoneState)
        {
            GEntity go = PlayerObject.Get();
            if (go != null)
            {
                ResetColors();
                _didInitZoneState = true;
            }
        }


        float delta = ColorFrameDelta;

        if (AmbientScale < 1.0f)
        {
            delta *= 2;
        }
        UpdateZoneState(delta);
        --ticksToZoneUpdate;
        if (ticksToZoneUpdate <= 0)
        {
            ticksToZoneUpdate = MaxTicksBetweenZoneUpdates;
            GEntity go = PlayerObject.Get();
            if (go == null)
            {
                return;
            }
            if (go != null)
            {
                int wx = (int)go.transform().localPosition.x;
                int wy = (int)go.transform().localPosition.z;

                if (wx >= 0 && wy >= 0 && _gs.md != null && wx < _gs.map.GetHwid() && wy < _gs.map.GetHhgt())
                {

                    int gx = wx / (MapConstants.TerrainPatchSize-1);
                    int gy = wy / (MapConstants.TerrainPatchSize-1);


                    int zoneId = 0;
                    TerrainPatchData patch = _terrainManager.GetTerrainPatch(_gs, gx, gy);
                    if (patch != null && patch.mainZoneIds != null)
                    {
                        wx %= (MapConstants.TerrainPatchSize - 1);
                        wy %= (MapConstants.TerrainPatchSize - 1);
                        zoneId = patch.mainZoneIds[wy, wx];
                    }

                    ActiveScreen hud = ss.GetScreen(_gs, ScreenId.HUD);


                    if (((_currentZone == null || _currentZone.IdKey != zoneId) && zoneId > 1) && hud != null)
                    {
                        Zone zone = _gs.map.Get<Zone>(zoneId);
                        if (zone == null)
                        {
                            return;
                        }
                        _currentZone = zone;
                        CurrentZoneShown = zone.IdKey;
                        _gs.ch.ZoneId = zone.IdKey;                        
                        _currentZoneType = _gs.data.Get<ZoneTypeSettings>(_gs.ch).Get(_currentZone.ZoneTypeId);
                        if (_currentZoneType != null)
                        {
                            WeatherType weatherType = _gs.data.Get<WeatherTypeSettings>(_gs.ch).Get(_currentZoneType.WeatherTypeId);
                            if (weatherType == null)
                            {
                                return;
                            }
                            DataWeather = weatherType;
                            SunlightColor.Target = TextureUtils.ConvertMyColorToColor(weatherType.LightColor);
                            FogColor.Target = TextureUtils.ConvertMyColorToColor(weatherType.FogColor);
                            CloudColor.Target = TextureUtils.ConvertMyColorToColor(weatherType.CloudColor);
                            AmbientColor.Target = TextureUtils.ConvertMyColorToColor(weatherType.AmbientColor);
                            SkyColor.Target = TextureUtils.ConvertMyColorToColor(weatherType.SkyColor);

                            FogDensity.Target = weatherType.FogScale;
                            CloudSpeed.Target = weatherType.CloudSpeed;
                            CloudDensity.Target = weatherType.CloudScale;
                            PrecipScale.Target = weatherType.PrecipScale;
                            WindScale.Target = weatherType.WindScale;
                            ParticleScale.Target = weatherType.ParticleScale;

                            SunlightIntensity.Target = weatherType.LightScale;
                            if (SunlightIntensityMultiplier > 0)
                            {
                                SunlightIntensity.Target *= SunlightIntensityMultiplier;
                            }

                            FogStart.Target = weatherType.FogDistance;
                            FogEnd.Target = LinearFogEnd;

                            _audioService.PlayMusic(_gs, _currentZoneType);
                        }
                        if (FogDistScale <= 1.0f)
                        {
                            _gs.Dispatch(new SetZoneNameEvent());
                        }
                    }
                }
            }
        }

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

    public void UpdateSettings()
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
            Wind.windMain = MathUtils.FloatRange(0.66f, 1.33f, _gs.rand) * WindScale.Current;
            windBurstEnd = DateTime.UtcNow.AddSeconds(MathUtils.FloatRange(4.0f, 7.0f, _gs.rand));
            nextWindBurst = DateTime.UtcNow.AddSeconds(MathUtils.FloatRange(12.0f, 22.0f, _gs.rand));
        }
    }
}
