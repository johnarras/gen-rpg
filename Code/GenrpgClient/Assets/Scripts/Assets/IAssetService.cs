﻿using Entities;
using Entities.Assets;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public interface IAssetService : IService
{

    void Init(UnityGameState gs, CancellationToken token);
    bool IsInitialized(UnityGameState gs);
    void LoadAssetInto(UnityGameState gs, object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token);
    void LoadAsset(UnityGameState gs, string assetCategory, string assetPath, OnDownloadHandler handler, object data, object parent, CancellationToken token);
        
    void LoadSprite(UnityGameState gs, string atlasName, string spriteName, OnDownloadHandler handler, object data, CancellationToken token);
    void LoadSpriteInto(UnityGameState gs, string atlasName, string spriteName, object parentSprite, CancellationToken token);
    void GetSpriteList(UnityGameState gs, string atlasName, SpriteListDelegate onLoad, CancellationToken token);

    void DownloadFile(UnityGameState gs, string url, DownloadData data, CancellationToken token);
    string GetArtURLPrefix(UnityGameState gs);
    void ClearMemoryCache(UnityGameState gs, CancellationToken token);
    string GetBundleForCategoryAndAsset(UnityGameState gs, string assetCategory, string assetPath);

    string StripPathPrefix(string path);
    void RemoveLoadedInstance(UnityGameState gs, string bundleName, int quantity = 1, bool showData = false);

    List<string> GetInitialDownloadBundleList(UnityGameState gs);
    bool IsNeverUnloadBundle(UnityGameState gs, string bundleName);
    bool IncrementalUnloadOnly(UnityGameState gs, string bundleName);
    bool IsDownloading(UnityGameState gs);

}