using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;

public class BundleItemData : MonoBehaviour
{ 
    public string BundleName;
    public IAssetService AssetService;
    public UnityGameState GS;

    public bool AlwaysShowDecrement = false;


    private void OnEnable()
    {
        UnityAssetService.ObjectsLoaded++;
    }

    private void OnDisable()
    {
        UnityAssetService.ObjectsUnloaded++;
    }

    protected void OnDestroy()
    {
        AssetService?.RemoveLoadedInstance(GS, BundleName, 1, AlwaysShowDecrement);
    }
}