﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Inventory.Entities;
using UnityEngine;

public class IconHelper
{
    public const string DefaultItemIconName = "ItemIcon";
    public const string DefaultSpellIconName = "SpellIcon";

    public static string GetBackingNameFromQuality(UnityGameState gs, long qualityTypeId)
    {
        string txt = "BGCommon";
        QualityType quality = gs.data.GetGameData<ItemSettings>().GetQualityType(qualityTypeId);
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


    public static void InitItemIcon(UnityGameState gs, InitItemIconData data, GameObject parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultItemIconName;

        if (data != null && !string.IsNullOrEmpty(data.iconPrefabName))
        {
            prefabName = data.iconPrefabName;
        }

        assetService.LoadAssetInto(gs, parent, AssetCategory.UI, prefabName, OnLoadItemIcon, data, token);

    }

    private static void OnLoadItemIcon(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }
        ItemIcon iicon = go.GetComponent<ItemIcon>();
        if (iicon == null)
        {
            GameObject.Destroy(go);
            return;
        }


        InitItemIconData idata = data as InitItemIconData;
        if (idata == null)
        {
            GameObject.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

    public static void InitSpellIcon(UnityGameState gs, InitSpellIconData data, GameObject parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultSpellIconName;

        if (data != null && !string.IsNullOrEmpty(data.iconPrefabName))
        {
            prefabName = data.iconPrefabName;
        }

        assetService.LoadAssetInto(gs, parent, AssetCategory.UI, prefabName, OnLoadSpellIcon, data, token);

    }

    private static void OnLoadSpellIcon(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }
        SpellIcon iicon = go.GetComponent<SpellIcon>();
        if (iicon == null)
        {
            GameObject.Destroy(go);
            return;
        }


        InitSpellIconData idata = data as InitSpellIconData;
        if (idata == null)
        {
            GameObject.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

}