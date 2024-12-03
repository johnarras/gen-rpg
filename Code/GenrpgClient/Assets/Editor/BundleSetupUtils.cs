using Genrpg.Shared.DataStores.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public delegate bool ExtraPrefabSetupStep(GameObject go);

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
    public static void BundleFilesInDirectory(string assetPathSuffix, bool allowAllFiles)
    {
        if (_assetService == null)
        {
            _assetService = new UnityAssetService();
        }

        string endOfPath = _assetService.GetAssetPath(assetPathSuffix);

        string pathWithoutSlash = endOfPath.Replace("/", "");

        List<string> paths = new List<string>();

        string fullPath = AssetConstants.DownloadAssetRootPath + endOfPath;

        string[] files = Directory.GetFiles(fullPath);

        int numAdded = 0;

        foreach (string fileName in files)
        {
            if (SetupFileAtPath(assetPathSuffix, fileName, false))
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
                BundleFilesInDirectory(assetPathSuffix + (!string.IsNullOrEmpty(assetPathSuffix) ? "/" : "") + subdirectory, false);
            }
            else
            {
                SetupFileAtPath(assetPathSuffix, directory, true);
                string bundleName = _assetService.GetBundleNameForCategoryAndAsset(assetPathSuffix, subdirectory);
                SetupChildFileOfBundle(directory, bundleName);

            }
        }
    }

    private static bool SetupFileAtPath(string assetPathSuffix, string item, bool allowDirectories, string assetBundleName = null)
    {
        if (!allowDirectories && EditorAssetUtils.IsNotPrefabName(item))
        {
            return false;
        }

        if (EditorAssetUtils.IsIgnoreFilename(item))
        {
            return false;
        }

        AssetDatabase.ImportAsset(item, ImportAssetOptions.Default);

        string fileName = _assetService.StripPathPrefix(item);

        AssetImporter importer = AssetImporter.GetAtPath(item) as AssetImporter;
        if (importer != null)
        {
            string shortFilename = fileName.Replace(AssetConstants.ArtFileSuffix, "");

            string bundleName = assetBundleName;

            if (string.IsNullOrEmpty(bundleName))
            {
                bundleName = _assetService.GetBundleNameForCategoryAndAsset(assetPathSuffix, shortFilename);
            }
            if (importer.assetBundleName != bundleName)
            {
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();
            }
        }
        return true;
    }




    private static void SetupChildFileOfBundle(string pathSuffix, string bundleName)
    {
        if (!Directory.Exists(pathSuffix))
        {
            return;
        }

        string[] directories = Directory.GetDirectories(pathSuffix);
        string[] files = Directory.GetFiles(pathSuffix);

        foreach (string directory in directories)
        {
            SetupChildFileOfBundle(directory, bundleName);
        }

        foreach (string file in files)
        {
            SetupFileAtPath(pathSuffix, file, true, bundleName);
        }

    }


}

