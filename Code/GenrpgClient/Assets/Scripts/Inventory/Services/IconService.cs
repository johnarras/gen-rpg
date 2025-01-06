using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Client.Assets;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings;
using Genrpg.Shared.Inventory.Settings.Qualities;
using System.Threading;
using UnityEngine;

public interface IIconService : IInjectable
{
    string GetBackingNameFromQuality(IGameData gameData, long qualityTypeId);
    string GetFrameNameFromLevel(IGameData gameData, long level);
    void InitItemIcon(InitItemIconData data, GameObject parent, IAssetService assetService, CancellationToken token);
    void InitSpellIcon(InitSpellIconData data, GameObject parent, IAssetService assetService, CancellationToken token);
}

public class IconService : IIconService
{
    private IClientEntityService _clientEntityService;

    public const string DefaultItemIconName = "ItemIcon";
    public const string DefaultSpellIconName = "SpellIcon";

    public string GetBackingNameFromQuality(IGameData gameData, long qualityTypeId)
    {
        string txt = "BGCommon";
        QualityType quality = gameData.Get<QualityTypeSettings>(null).Get(qualityTypeId);
        if (quality == null || string.IsNullOrEmpty(quality.Icon))
        {
            return txt;
        }

        return quality.Icon;
    }

    public string GetFrameNameFromLevel(IGameData gameData, long level)
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


    public void InitItemIcon(InitItemIconData data, GameObject parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultItemIconName;

        if (data != null && !string.IsNullOrEmpty(data.IconPrefabName))
        {
            prefabName = data.IconPrefabName;
        }

        assetService.LoadAssetInto(parent, AssetCategoryNames.UI, 
            prefabName, OnLoadItemIcon, data, token, data.SubDirectory);

    }

    private void OnLoadItemIcon(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }
        ItemIcon iicon = go.GetComponent<ItemIcon>();
        if (iicon == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }


        InitItemIconData idata = data as InitItemIconData;
        if (idata == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

    public void InitSpellIcon(InitSpellIconData data, GameObject parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultSpellIconName;

        if (data != null && !string.IsNullOrEmpty(data.iconPrefabName))
        {
            prefabName = data.iconPrefabName;
        }

        assetService.LoadAssetInto(parent, AssetCategoryNames.UI, 
            prefabName, OnLoadSpellIcon, data, token, data.subdirectory);

    }

    private void OnLoadSpellIcon(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }
        SpellIcon iicon = go.GetComponent<SpellIcon>();
        if (iicon == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }


        InitSpellIconData idata = data as InitSpellIconData;
        if (idata == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

}
