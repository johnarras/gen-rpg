using Cysharp.Threading.Tasks;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Texturse;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using UI.Screens.Constants;
using UnityEngine;

public class TestAssetDownloads : IInjectable
{
    private ILogService _logService;
    private IAssetService _assetService;
    private IScreenService _screenService;
    private IGameData _gameData;
    public async UniTask RunTests(IUnityGameState gs, CancellationToken token)
    {
        gs.loc.Resolve(this);

        _logService.Info("Start test");

        _logService.Info("Test screens");


        TestScreens(token);


        TestAssetCategory<UnitSettings,UnitType>(AssetCategoryNames.Monsters, token);

        TestAssetCategory<TextureTypeSettings,TextureType>(AssetCategoryNames.TerrainTex, token);

        TestAssetCategory<TreeTypeSettings, TreeType>(AssetCategoryNames.Trees, token,
            x => !x.HasFlag(TreeFlags.IsBush));

        TestAssetCategory<TreeTypeSettings, TreeType>(AssetCategoryNames.Bushes, token,
            x => x.HasFlag(TreeFlags.IsBush));

        TestMagic(token);


        await UniTask.CompletedTask;
    }

    private void OnDownloadAsset(System.Object obj, object data, CancellationToken token)
    {
        if (obj == null)
        {
           _logService.Info("Failed Download: " + data);
        }
        GEntityUtils.Destroy(obj as GameObject);

       
    }

    private void TestAssetCategory<Parent,Child> (string assetCategoryName, CancellationToken token, Func<Child, bool> filter = null) where Parent : ITopLevelSettings
    {
        Parent settings = _gameData.Get<Parent>(null);

        if (settings == null)
        {
            _logService.Info("Missing settings of type " + typeof(Parent).Name);
            return;
        }

        List<Child> childSettings = settings.GetChildren().Cast<Child>().ToList();  

        if (childSettings == null || childSettings.Count < 1)
        {
            return;
        }

        if (filter != null)
        {
            childSettings = childSettings.Where(x=> filter(x) == true).ToList();
        }

        foreach (Child setting in childSettings)
        {
            if (setting is IIndexedGameItem indexedItem)
            {
                if (indexedItem.IdKey == 0 || 
                    string.IsNullOrEmpty(indexedItem.Art) ||
                    indexedItem.Art.IndexOf("Unused") >= 0)
                {
                    continue;
                }

                if (filter != null && !filter(setting))
                {
                    continue;
                }

                if (indexedItem is IVariationIndexedGameItem variationItem)
                {
                    for (int i = 1; i <= variationItem.VariationCount; i++)
                    {

                        _assetService.LoadAsset(assetCategoryName, indexedItem.Art + i,
                            OnDownloadAsset, assetCategoryName + "-" + indexedItem.Art + i, null, token);
                    }
                }

                else
                {
                    _assetService.LoadAsset(assetCategoryName, indexedItem.Art,
                        OnDownloadAsset, assetCategoryName + "-" + indexedItem.Art, null, token);
                }
            }
        }
    }

    private void TestScreens(CancellationToken token)
    {
        foreach (ScreenId sid in Enum.GetValues(typeof(ScreenId)))
        {
            if (sid == ScreenId.None || sid.ToString().IndexOf("Unused") >= 0)
            {
                continue;
            }

            string subDir = _screenService.GetSubdirectory(sid);

            _assetService.LoadAsset(AssetCategoryNames.UI, sid.ToString() + "Screen", OnDownloadAsset, "Screen: " + sid, null, token, subDir);
        }
    }

    private void TestMagic(CancellationToken token)
    {
        IReadOnlyList<ElementType> elements = _gameData.Get<ElementTypeSettings>(null).GetData();


        List<string> fxNames = EntityUtils.GetStaticStrings(typeof(FXNames));

        foreach (ElementType element in elements)
        {
            if (string.IsNullOrEmpty(element.Art))
            {
                continue;
            }

            foreach (string fxName in fxNames)
            {
                string fullName = element.Art + fxName;
                _assetService.LoadAsset(AssetCategoryNames.Magic, fullName, OnDownloadAsset, fullName, null, token);
            }
        }


    }
}
