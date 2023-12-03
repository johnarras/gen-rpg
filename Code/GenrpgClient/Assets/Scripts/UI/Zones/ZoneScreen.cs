using System;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Currencies.Constants;

public class ZoneScreen : BaseScreen
{
    
    public GText ZoneName;
    public GRawImage MapImage;
    public GEntity ArrowParent;
    public GButton CloseButton;

    protected GEntity ArrowObject;


    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        Setup();
        UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);

        FloatingTextScreen.Instance.ShowMessage(_gs.data.GetGameData<CurrencySettings>(_gs.ch).GetCurrencyType(CurrencyTypes.Money).Art);
        await Task.CompletedTask;

    }

    private void Setup()
    {
        _assetService.LoadAssetInto(_gs, ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, _token);

        UIHelper.SetImageTexture(MapImage, UnityZoneGenService.mapTexture);
        ShowPlayer();

    }


    private void OnLoadArrow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        ArrowObject = obj as GEntity;
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

        GEntity arrow = ArrowObject;

        if (arrow == null)
        {
            return;
        }

        // Show player on map with arrow.
        GEntity player = PlayerObject.Get();
        if (player == null)
        {
            return;
        }

        GVector3 pos = GVector3.Create(player.transform().localPosition);

        if (MapImage == null || MapImage.texture == null)
        {
            return;
        }

        Texture mapTexture = MapImage.mainTexture;

        int mapSize = mapTexture.width;


        float imageSize = MapImage.rectTransform.sizeDelta.x;

        float minZonePixelSize = imageSize / 2;

        float minPercentSize = 1.0f * minZonePixelSize / mapSize;



        Zone currZone = _gs.map.Get<Zone>(ZoneStateController.CurrentZoneShown);

        float oldminx = 0;
        float oldminy = 0;
        float oldmaxx = 0;
        float oldmaxy = 0;

        if (currZone != null)
        { 
            float minx = currZone.ZMin; float miny = currZone.XMin; float maxx = currZone.ZMax; float maxy = currZone.XMax;

            xminpct = minx * 1.0f / _gs.map.GetHwid();
            xmaxpct = maxx * 1.0f / _gs.map.GetHhgt();
            yminpct = miny * 1.0f / _gs.map.GetHhgt();
            ymaxpct = maxy * 1.0f / _gs.map.GetHhgt();

            oldminx = xminpct;
            oldminy = yminpct;
            oldmaxx = xmaxpct;
            oldmaxy = ymaxpct;

            float numBlocks = _gs.map.GetHwid() / MapConstants.TerrainPatchSize;

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

            UIHelper.SetText(ZoneName, currZone.Name);
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
        float xpctstart = pos.x / _gs.map.GetHwid();
        float ypctstart = pos.z / _gs.map.GetHhgt();

        float newdx = xmaxpct - xminpct;
        float newdy = ymaxpct - yminpct;

        float xpct = MathUtils.Clamp(0, (xpctstart - xminpct) / (xmaxpct - xminpct), 1) - 0.5f;
        float ypct = MathUtils.Clamp(0, (ypctstart - yminpct) / (ymaxpct - yminpct), 1) - 0.5f;

        float rot = player.transform().eulerAngles.y;

        float sx = xpct * imageSize;
        float sy = ypct * imageSize;

        GVector3 cpos = GVector3.Create(arrow.transform().localPosition);
        arrow.transform().localPosition = GVector3.Create(sx, sy, cpos.z);

        arrow.transform().eulerAngles = GVector3.Create(0, 0, -rot);


    }

}

