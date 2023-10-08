using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Genrpg.Shared.Core.Entities;


using Genrpg.Shared.DataStores.Entities;

public class SetupBundles
{
	public const int BillboardSize = 128;
	public const int AtlasSize = 512;
	public const int CoreSize = 1024;

    [MenuItem("Build/Clear Bundles")]
    static void ClearAllAssetBundles()
    {
        ClearBundlesInPath("Assets");
    }

    private static void ClearBundlesInPath(string path)
    {
        path += "/";
        if (!Directory.Exists(path))
        {
            return;
        }

        string[] dirs = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            AssetImporter importer = AssetImporter.GetAtPath(file) as AssetImporter;

            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.assetBundleName = "";
                importer.SaveAndReimport();
            }
        }

        foreach (string dir in dirs)
        {
            ClearBundlesInPath(dir);
        }
                
    }


    [MenuItem("Build/Setup Bundles")]
    static void SetupBundlesDirect()
    {
        SetupAll(null);
    }

    public static void SetupAll(UnityGameState gs)
    { 
        gs = SetupEditorUnityGameState.Setup(gs);

        SetupAtlases(gs);
        SetupAudio(gs);

        SetupRocks(gs);

        SetupTerrainTextures(gs);
        SetupGrass(gs);

        SetupMonsters(gs);


        SetupMagicEffects(gs);


        SetupProps(gs);

        SetupBushes(gs);

        SetupTrees(gs);

        SetupUI(gs);
        SetupScreens(gs);
    }


    // These are set up by hand because it's a pain to do the process of setting up the whole atlas.
    // But the procedure is for a folder named FolderName inside of the Atlas folder
    // 1. Make a a prefab called FolderNamePrefab
    // 2. Add a SpriteAtlasContainer to it.
    // 3. Create an atlas called FolderNameAtlas in the same folder.
    // 4. Drag the folder named FolderName into the objects list of the new SpriteAtlas.
    // 5. Make sure the objects inside of FolderName are sprites.
    // 6. Add the new atlas as the Atlas variable in the SpriteAtlasContainer.
    public static void SetupAtlases(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Atlas, true, false, null);

}
    public static void SetupAudio(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Audio, true, false, null);
    }

    public static void SetupRocks(UnityGameState gs)
	{
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Rocks, true, false, null);
    }

    public static void SetupUI(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.UI, false, false, null);
    }

    public static void SetupScreens(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Screens, false, false, null);
    }

    public static void SetupTrees(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Trees, true, true, null);
    }
    public static void SetupBushes(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Bushes, true, true, null);
    }

    public static void SetupProps(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Props, false, false, null);
    }
    public static void SetupTerrainTextures(UnityGameState gs)
    {
        BundleSetupUtils.SetupPrefabsFromTexturesWithNormalmaps(gs, AssetCategory.TerrainTex);
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.TerrainTex, false, false, null);
    }
    public static void SetupGrass(UnityGameState gs)
    {
        BundleSetupUtils.SetupPrefabsFromDiffuseTextures(gs, AssetCategory.Grass);
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Grass, true, true, null);
    }

    public static void SetupMagicEffects(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        BundleSetupUtils.BundleRootDirectory(gs, AssetCategory.Magic, false, false);
    }
    public static void SetupMonsters(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);

        BundleSetupUtils.BundleFilesInDirectory(gs, AssetCategory.Monsters, true, true, null);
    }

    //[MenuItem("Build/Update Terrain Textures")]
    public static void UpdateTerrainTextures()
    {
        UnityGameState gs = SetupEditorUnityGameState.Setup(null);
        string assetCategory = AssetCategory.TerrainTex;
        if (string.IsNullOrEmpty(assetCategory))
        {
            return;
        }

        string endOfPath = UnityAssetService.GetAssetPath(assetCategory);
        if (string.IsNullOrEmpty(endOfPath))
        {
            return;
        }

        string pathWithoutSlash = endOfPath.Replace("/", "");

        List<string> paths = new List<string>();

        paths.Add("Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath + "Textures/");
        paths.Add("Assets/Resources/" + endOfPath + "Textures/");

        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                Debug.Log("Directory doesn't exist:" + path);
                continue;
            }
            string[] files = Directory.GetFiles(path);

            UnityAssetService _assetService = new UnityAssetService();
            foreach (string item in files)
            {
                if (EditorAssetUtils.IsIgnoreFilename(item))
                {
                    continue;
                }

                AssetDatabase.ImportAsset(item, ImportAssetOptions.ForceUpdate);
                Texture2D t2d = AssetDatabase.LoadAssetAtPath(item, typeof(UnityEngine.Object)) as Texture2D;

                if (t2d == null)
                {
                    break;
                }


                string fileName = _assetService.StripPathPrefix(item);

                TextureImporter importer = AssetImporter.GetAtPath(item) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.sRGBTexture = true;
                importer.anisoLevel = 2;
                importer.filterMode = FilterMode.Trilinear;
                importer.crunchedCompression = false;
                importer.compressionQuality = 100;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 512;

                if (item.IndexOf("_n") < 0)
                {
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.alphaIsTransparency = false;
                }
                importer.SaveAndReimport();
            }
        }
        AssetDatabase.SaveAssets();
    }
}
