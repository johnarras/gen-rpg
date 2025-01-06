using System;
using UnityEngine;
using Genrpg.Shared.Utils;

using System.Threading;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.MapServer.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Assets.Constants;

public class ZoneScreen : BaseScreen
{
    
    public GText ZoneName;
    public GRawImage MapImage;
    public GameObject ArrowParent;

    protected GameObject ArrowObject;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;


    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        Setup();

        await Task.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, _token, "Maps");

        _uiService.SetImageTexture(MapImage, MinimapUI.GetTexture());
        ShowPlayer();

    }


    private void OnLoadArrow(object obj, object data, CancellationToken token)
    {
        ArrowObject = obj as GameObject;
    }

    protected override void ScreenUpdate()
    {
        ShowPlayer();
    }


    private long _lastZoneShown = -1;
    float xminpct = 0;
    float xmaxpct = 0;
    float yminpct = 0;
    float ymaxpct = 0;


    const float ZonePadPercent = 0.05f;

    void ShowPlayer()
    {

        GameObject arrow = ArrowObject;

        if (arrow == null)
        {
            return;
        }

        // Show player on map with arrow.
        GameObject player = _playerManager.GetPlayerGameObject();
        if (player == null)
        {
            return;
        }

        Vector3 pos = player.transform.localPosition;

        if (MapImage == null || MapImage.texture == null)
        {
            return;
        }

        Texture mapTexture = MapImage.mainTexture;

        int mapSize = mapTexture.width;

        float imageSize = MapImage.rectTransform.sizeDelta.x;

        float minZonePixelSize = imageSize / 2;

        float minPercentSize = 1.0f * minZonePixelSize / mapSize;

        Zone currZone = _mapProvider.GetMap().Get<Zone>(ZoneStateController.CurrentZoneShown);

        float oldminx = 0;
        float oldminy = 0;
        float oldmaxx = 0;
        float oldmaxy = 0;

        if (currZone != null)
        { 
            float minx = currZone.ZMin; float miny = currZone.XMin; float maxx = currZone.ZMax; float maxy = currZone.XMax;

            xminpct = minx * 1.0f / _mapProvider.GetMap().GetHwid();
            xmaxpct = maxx * 1.0f / _mapProvider.GetMap().GetHhgt();
            yminpct = miny * 1.0f / _mapProvider.GetMap().GetHhgt();
            ymaxpct = maxy * 1.0f / _mapProvider.GetMap().GetHhgt();

            oldminx = xminpct;
            oldminy = yminpct;
            oldmaxx = xmaxpct;
            oldmaxy = ymaxpct;

            float numBlocks = _mapProvider.GetMap().GetHwid() / MapConstants.TerrainPatchSize;

            float edgeSize = 0.01f;

            if (numBlocks > 0)
            {
                edgeSize = Math.Min(0.05f, 1.0f / numBlocks);
            }
            edgeSize = 0;

            float xdiff = xmaxpct - xminpct;
            float ydiff = ymaxpct - yminpct;

            float xmid = (xminpct + xmaxpct) / 2;
            float ymid = (yminpct + ymaxpct) / 2;

            float maxDiff = Math.Max(xdiff, ydiff);

            maxDiff *= (1 + ZonePadPercent);

            if (maxDiff < minPercentSize)
            {
                //dmaxDiff = minPercentSize;
            }



            xminpct = xmid - maxDiff / 2;
            xmaxpct = xmid + maxDiff / 2;
            if (xminpct < 0)
            {
                xmaxpct += -xminpct;
                xminpct = 0;
            }

            if (xmaxpct > 1)
            {
                xminpct -= (xmaxpct - 1);
                xmaxpct = 1;
            }
            yminpct = ymid - maxDiff / 2;
            ymaxpct = ymid + maxDiff / 2;
            if (yminpct < 0)
            {
                ymaxpct += -yminpct;
                yminpct = 0;
            }
            if (ymaxpct > 1)
            {
                yminpct -= (ymaxpct - 1);
                ymaxpct = 1;
            }
            xdiff = maxDiff;
            ydiff = maxDiff;

            
            if (mapTexture != null)
            {
                MapImage.uvRect = new Rect(new Vector2(xminpct, yminpct), new Vector2(xdiff, ydiff));
            }
            
            _lastZoneShown = currZone.IdKey;

            _uiService.SetText(ZoneName, currZone.Name);
        }
        else if (currZone == null)
        {
            return;
        }




        if (xminpct >= xmaxpct || yminpct >= ymaxpct)
        {
            return;
        }

        // Player pct goes from -0.5 to 0.5.
        float xpctstart = pos.x / _mapProvider.GetMap().GetHwid();
        float ypctstart = pos.z / _mapProvider.GetMap().GetHhgt();

        float newdx = xmaxpct - xminpct;
        float newdy = ymaxpct - yminpct;

        float xpct = MathUtils.Clamp(0, (xpctstart - xminpct) / (xmaxpct - xminpct), 1) - 0.5f;
        float ypct = MathUtils.Clamp(0, (ypctstart - yminpct) / (ymaxpct - yminpct), 1) - 0.5f;

        float rot = player.transform.eulerAngles.y;

        float sx = xpct * imageSize;
        float sy = ypct * imageSize;

        Vector3 cpos = arrow.transform.localPosition;
        arrow.transform.localPosition = new Vector3(sx, sy, cpos.z);

        arrow.transform.eulerAngles = new Vector3(0, 0, -rot);


    }

}

