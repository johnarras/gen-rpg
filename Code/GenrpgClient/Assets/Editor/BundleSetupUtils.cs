using Genrpg.Shared.DataStores.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public delegate bool ExtraPrefabSetupStep(UnityGameState gs, GameObject go);

public class BundleSetupUtils
{
    protected static UnityAssetService _assetService;
    public const bool MakeStaticObjects = false;

    public const int BundleFiles = 0;
    public const int BundleDirectories = 1;
    public const int BundleRoot = 2;
    /// <summary>
    /// Use this function to 
    /// </summary>
    /// <param name="path"></param>
    public static void BundleFilesInDirectory(UnityGameState gs, string assetPathSuffix, bool allowAllFiles)
    {
        if (_assetService == null)
        {
            _assetService = new UnityAssetService();
        }

        string endOfPath = AssetUtils.GetAssetPath(assetPathSuffix);

        string pathWithoutSlash = endOfPath.Replace("/", "");

        List<string> paths = new List<string>();

        string fullPath = "Assets/" + AssetConstants.DownloadAssetRootPath + endOfPath;

        string[] files = Directory.GetFiles(fullPath);

        int numAdded = 0;

        foreach (string fileName in files)
        {
            if (SetupFileAtPath(gs, assetPathSuffix, fileName, false))
            {
                numAdded++;
            }
        }
        if (numAdded > 0)
        {
            AssetDatabase.SaveAssets();
        }

        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                continue;
            }
        }

        string[] directories = Directory.GetDirectories(fullPath);

        foreach (string directory in directories)
        {
            string subdirectory = directory.Replace(fullPath, "");
            if (string.IsNullOrEmpty(assetPathSuffix))
            {
                BundleFilesInDirectory(gs, assetPathSuffix + (!string.IsNullOrEmpty(assetPathSuffix) ? "/" : "") + subdirectory, false);
            }
            else
            {
                SetupFileAtPath(gs, assetPathSuffix, directory, true);
            }
        }
    }

    private static bool SetupFileAtPath(UnityGameState gs, string assetPathSuffix, string item, bool allowAllFiles)
    {
        if (!allowAllFiles && EditorAssetUtils.IsIgnoreFilename(item))
        {
            return false;
        }

        AssetDatabase.ImportAsset(item, ImportAssetOptions.Default);

        string fileName = _assetService.StripPathPrefix(item);

        AssetImporter importer = AssetImporter.GetAtPath(item) as AssetImporter;
        if (importer != null)
        {
            string shortFilename = fileName.Replace(AssetConstants.ArtFileSuffix, "");
            string bundleName = _assetService.GetBundleNameForCategoryAndAsset(gs, assetPathSuffix, shortFilename);
            if (importer.assetBundleName != bundleName)
            {
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();
            }
        }
        return true;
    }
}

