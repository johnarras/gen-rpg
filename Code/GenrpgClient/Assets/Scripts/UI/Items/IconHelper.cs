using Genrpg.Shared.Inventory.Settings;
using Genrpg.Shared.Inventory.Settings.Qualities;
using System.Threading;
using GEntity = UnityEngine.GameObject;

public class IconHelper
{
    public const string DefaultItemIconName = "ItemIcon";
    public const string DefaultSpellIconName = "SpellIcon";

    public static string GetBackingNameFromQuality(UnityGameState gs, long qualityTypeId)
    {
        string txt = "BGCommon";
        QualityType quality = gs.data.GetGameData<QualityTypeSettings>(gs.ch).GetQualityType(qualityTypeId);
        if (quality == null || string.IsNullOrEmpty(quality.Icon))
        {
            return txt;
        }

        return quality.Icon;
    }

    public static string GetFrameNameFromLevel(UnityGameState gs, long level)
    {
        if (level < 0)
        {
            level = 0;
        }

        if (level > 100)
        {
            level = 100;
        }

        level -= level % 5;
        return "Frame_" + level.ToString().PadLeft(3, '0');
    }


    public static void InitItemIcon(UnityGameState gs, InitItemIconData data, GEntity parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultItemIconName;

        if (data != null && !string.IsNullOrEmpty(data.iconPrefabName))
        {
            prefabName = data.iconPrefabName;
        }

        assetService.LoadAssetInto(gs, parent, AssetCategoryNames.UI, 
            prefabName, OnLoadItemIcon, data, token, "Items");

    }

    private static void OnLoadItemIcon(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }
        ItemIcon iicon = go.GetComponent<ItemIcon>();
        if (iicon == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }


        InitItemIconData idata = data as InitItemIconData;
        if (idata == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

    public static void InitSpellIcon(UnityGameState gs, InitSpellIconData data, GEntity parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultSpellIconName;

        if (data != null && !string.IsNullOrEmpty(data.iconPrefabName))
        {
            prefabName = data.iconPrefabName;
        }

        assetService.LoadAssetInto(gs, parent, AssetCategoryNames.UI, 
            prefabName, OnLoadSpellIcon, data, token, "Spells");

    }

    private static void OnLoadSpellIcon(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }
        SpellIcon iicon = go.GetComponent<SpellIcon>();
        if (iicon == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }


        InitSpellIconData idata = data as InitSpellIconData;
        if (idata == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

}
