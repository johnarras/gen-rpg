using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Services;
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
    public static void BundleFilesInDirectory(UnityGameState gs, string assetCategory, bool enableGPUInstancing,
        bool renameMaterials, ExtraPrefabSetupStep extraSetup)
    {
        if (_assetService == null)
        {
            _assetService = new UnityAssetService();
        }
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

        paths.Add("Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath);
        //paths.Add("Assets/Resources/" + endOfPath);

        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                //Debug.Log("Directory doesn't exist:" + path);
                continue;
            }
            string[] files = Directory.GetFiles(path);

            int numAdded = 0;

            foreach (string item in files)
            {

                if (SetupItemAtPath(gs, true, assetCategory, path, item, enableGPUInstancing, renameMaterials, extraSetup))
                {
                    numAdded++;
                }
            }
            AssetDatabase.SaveAssets();
        }
    }

    static float [] smallLODs = new float[] { 0.04f, 0.03f, 0.02f, 0.01f, 0.005f, 0.002f };
    static float [] largeLODs = new float[] { 0.10f, 0.05f, 0.02f, 0.01f, 0.005f, 0.002f };


    private static bool SetupItemAtPath(UnityGameState gs, bool setBundleName, string assetCategory, string path, string item, bool enableGPUInstancing, bool renameMaterials, ExtraPrefabSetupStep extraSetup)
    {
        if (AssetUtils.IsIgnoreFilename(item))
        {
            return false;
        }

        AssetDatabase.ImportAsset(item, ImportAssetOptions.ForceUpdate);


        string artName = item.Replace(path, "");
        string artPath = item;
        GameObject go = GetOrCreatePrefabAtPath(artName, artPath);

        if (go == null)
        {
            return false;
        }

        string fileName = _assetService.StripPathPrefix(item);

        bool changedSomething = false;
        string bundleName = "";

        AssetImporter importer = AssetImporter.GetAtPath(item) as AssetImporter;
        if (importer != null)
        {
            if (setBundleName)
            {
                if (path.IndexOf(AssetCategory.DownloadAssetRootPath) < 0)
                {
                    importer.assetBundleName = "";
                }
                else
                {
                    string shortFilename = fileName.Replace(UnityAssetService.ArtFileSuffix, "");
                    bundleName = _assetService.GetBundleForCategoryAndAsset(gs, assetCategory, shortFilename);
                    importer.assetBundleName = bundleName;
                }
                importer.SaveAndReimport();
            }
        }

        if (enableGPUInstancing)
        {

            string matBundleName = (renameMaterials ? bundleName : "");

            matBundleName = bundleName;

            if (SetupRenderers<Renderer>(gs, go, path, matBundleName, renameMaterials))
            {
                changedSomething = true;
            }

            if (GameObjectUtils.SetStatic(go, MakeStaticObjects))
            {
                changedSomething = true;
            }
        }

        if (extraSetup != null)
        {
            if (extraSetup(gs, go))
            {
                changedSomething = true;
            }
        }

        LODGroup lodgroup = GameObjectUtils.GetComponent<LODGroup>(go);

        if (lodgroup != null)
        {
            float[] lodPercents = (assetCategory == AssetCategory.Bushes ? smallLODs : largeLODs);
            LOD[] lods = lodgroup.GetLODs();
            int len = lods.Length;

            for (int k = 0; k < len; k++)
            {
                float pct = 0.001f;
                if (k < lodPercents.Length)
                {
                    pct = lodPercents[k];
                }

                if (lods[k].screenRelativeTransitionHeight != pct)
                {
                    lods[k].screenRelativeTransitionHeight = pct;
                    changedSomething = true;
                }

            }
            if (changedSomething)
            {

                LOD[] newLods = new LOD[len];
                for (int l = 0; l < len; l++)
                {
                    newLods[l] = lods[l];
                }
                lodgroup.SetLODs(newLods);
            }
        }


        if (changedSomething)
        {
            PrefabUtility.SaveAsPrefabAsset(go, artPath);
        }

        if (changedSomething)
        {
            importer.SaveAndReimport();
        }


        GameObject.DestroyImmediate(go);
        return true;
    }

    private static bool SetupRenderers<T>(UnityGameState gs, GameObject go, string path, string matBundleName, bool renameMaterials) where T : Renderer
    {

        bool changedSomething = false;
        if (go == null)
        {
            return false;
        }

        //shaderBundleName = matBundleName;
        List<T> renderers = GameObjectUtils.GetComponents<T>(go);

        string prefixName = go.name.Replace("(Clone)", "").ToLower();

        
        if (renderers == null || renderers.Count < 1)
        {
            return false;
        }


        string[] lodList = new string[] { "LOD0", "LOD1", "LOD2" };

        string objName = go.name.Replace("(Clone)", "");

      

        foreach (T rend in renderers)
        {
            if (rend.lightProbeUsage != LightProbeUsage.Off)
            {
                rend.lightProbeUsage = LightProbeUsage.Off;
                changedSomething = true;
            }

            if (rend.lightProbeUsage != 0)
            {
                rend.lightProbeUsage = 0;
                changedSomething = true;
            }

            if (rend.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
            {
                rend.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                changedSomething = true;
            }

            if (rend.reflectionProbeUsage != ReflectionProbeUsage.Simple)
            {
                rend.reflectionProbeUsage = ReflectionProbeUsage.Simple;
                changedSomething = true;
            }

            if (rend.shadowCastingMode != ShadowCastingMode.On)
            {
                rend.shadowCastingMode = ShadowCastingMode.On;
                changedSomething = true;
            }

            if (!rend.receiveShadows)
            {
                rend.receiveShadows = true;
                changedSomething = true;
            }
            

            if (rend.sharedMaterials == null)
            {
                continue;
            }

            Material[] mats = rend.sharedMaterials;

            BillboardRenderer billRend = rend as BillboardRenderer;
            if (billRend != null)
            {
                BillboardAsset billAsset = billRend.billboard;
                if (billAsset != null && billAsset.material != null)
                {
                    mats = new Material[1];
                    mats[0] = billAsset.material;
                }
            }


            foreach (Material mat in mats)
            {
                if (mat == null)
                {
                    continue;
                }
                if (!mat.enableInstancing)
                {
                    mat.enableInstancing = true;
                    changedSomething = true;
                }
                string origFullPath = AssetDatabase.GetAssetPath(mat);

                int lastSlashIndex = origFullPath.LastIndexOf("/");
                string matName = origFullPath.Substring(lastSlashIndex + 1);
                string origPathPrefix = origFullPath.Substring(0, lastSlashIndex + 1);

                string newMatName = matName;
                string newFullPath = origFullPath;

                if (renameMaterials)
                {

                    if (matName.IndexOf(objName) >= 0 &&
                        (matName.IndexOf("Leaves") > 0 || matName.IndexOf("Fronds") > 0 ||
                        matName.IndexOf("Branches") > 0))
                    {
                        //matName = matName.Replace(objName, "");
                    }
                    foreach (string lodname  in lodList)
                    {
                        if (origFullPath.IndexOf(lodname) >= 0)
                        {
                            if (matName.IndexOf(lodname) >= 0)
                            {
                                if ((matName.IndexOf("Leaves") > 0 || matName.IndexOf("Fronds") > 0 ||
                                    matName.IndexOf("Branches") > 0))
                                {
                                    // matName = matName.Replace(lodname, "");
                                }
                            }
                            //newMatName = newMatName = newMatName.Replace(objName, objName + lodname);
                        }
                    }

                    newMatName = matName;
                    


                    newFullPath = origPathPrefix + newMatName;

                   // var retval = AssetDatabase.RenameAsset(origFullPath, newMatName);
                }




                List<Texture> textures = new List<Texture>();
                string[] texNames = mat.GetTexturePropertyNames();
                foreach (string texName in texNames)
                {
                    if (mat.HasProperty(texName))
                    {
                        Texture newTex = mat.GetTexture(texName);
                        if (newTex != null)
                        {
                            textures.Add(newTex);
                        }
                    }
                }



                foreach (Texture tex in textures)
                {
                    TextureImporter texImporter = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) as TextureImporter;
                    if (texImporter == null)
                    {
                        continue;
                    }

                    bool shouldSave = false;
                
                    if (!texImporter.isReadable)
                    {
                        texImporter.isReadable = true;
                        shouldSave = true;
                    }


                    if (string.IsNullOrEmpty(texImporter.assetBundleName))
                    {
                        texImporter.assetBundleName = matBundleName;
                        shouldSave = true;
                    }
                    else if (texImporter.assetBundleName != matBundleName)
                    {
                        texImporter.assetBundleName = "";
                        shouldSave = true;
                    }

                    if (shouldSave)
                    {
                        texImporter.SaveAndReimport();
                        changedSomething = true;
                    }

                }
            }
        }

        return changedSomething;
    }

   

    /// <summary>
    /// Use this function to 
    /// </summary>
    /// <param name="path"></param>
    public static void BundleDirectoriesInDirectory(UnityGameState gs, string assetCategory, string pathSuffix,
        string removeFromBundleName, bool renameMaterials, bool enableGPUInstancing)
    {

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
        
        if (!string.IsNullOrEmpty(pathSuffix))
        {
            endOfPath += pathSuffix;
        }

        paths.Add("Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath);
        paths.Add("Assets/Resources/" + endOfPath);

        foreach (string path in paths)
        {

            try
            {

                if (!Directory.Exists(path))
                {
                    Debug.Log("Directory doesn't exist:" + path);
                    continue;
                }
                string[] dirs = Directory.GetDirectories(path);

                int numAdded = 0;

                UnityAssetService _assetService = new UnityAssetService();
                foreach (string item in dirs)
                {
                    string filename = _assetService.StripPathPrefix(item);
                    filename += "/";
                    string bundleName = "";
                    AssetImporter importer = AssetImporter.GetAtPath(item) as AssetImporter;
                    if (importer != null)
                    {
                        if (path.IndexOf(AssetCategory.DownloadAssetRootPath) < 0)
                        {
                            importer.assetBundleName = "";
                        }
                        else
                        {
                            bundleName = _assetService.GetBundleForCategoryAndAsset(gs, assetCategory, filename);
                            if (!string.IsNullOrEmpty(removeFromBundleName))
                            {
                                bundleName = bundleName.Replace(removeFromBundleName, "");
                            }
                            importer.assetBundleName = bundleName;
                            Debug.Log("Set bundle name to: " + importer.assetBundleName + " for " + filename);
                        }
                        importer.SaveAndReimport();
                        numAdded++;
                    }

                    string[] allFiles = Directory.GetFiles(item);
                    if (allFiles != null)
                    {
                        foreach (string file in allFiles)
                        {
                            SetupItemAtPath(gs, false, assetCategory, path, file, enableGPUInstancing, renameMaterials, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception on setup bundle directories: " + e.Message + " " + e.StackTrace + " " + path);
            }
        }
    }

    [MenuItem("Build/Create Terrain Texture Prefabs")]
    public static void SetupTerrainTexturePrefabs()
    {
        UnityGameState gs = SetupEditorUnityGameState.Setup(null);
        SetupPrefabsFromTexturesWithNormalmaps(gs, AssetCategory.TerrainTex);
    }

    [MenuItem("Build/Create Grass Texture Prefabs")]
    public static void SetupGrassTexturePrefabs()
    {
        UnityGameState gs = SetupEditorUnityGameState.Setup(null);
        SetupPrefabsFromDiffuseTextures(gs, AssetCategory.Grass);
    }

    public static GameObject GetOrCreatePrefabAtPath(string artName, string artPath)
    {

        if (string.IsNullOrEmpty(artPath) || string.IsNullOrEmpty(artName))
        {
            return null;
        }

        GameObject art = (GameObject)AssetDatabase.LoadAssetAtPath(artPath, typeof(GameObject));

        if (art == null)
        {
            UnityEngine.Object otherAsset = AssetDatabase.LoadAssetAtPath(artPath, typeof(object));
            if (otherAsset != null)
            {
                return null;
            }
            art = new GameObject();
            art.name = artName;
            PrefabUtility.SaveAsPrefabAsset(art, artPath);
            GameObject.DestroyImmediate(art);
            art = (GameObject)AssetDatabase.LoadAssetAtPath(artPath, typeof(GameObject));

        }

        art = GameObject.Instantiate<GameObject>(art);

        return art;
    }


    /// <summary>
    /// Use this function to 
    /// </summary>
    /// <param name="path"></param>

    public static void SetupPrefabsFromTexturesWithNormalmaps (UnityGameState gs, string assetCategory)
    {

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

        string texturePathSuffix = "Textures";

        paths.Add("Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath + texturePathSuffix);


        string artFolder = "Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath;

        StringBuilder sb = new StringBuilder();
        foreach (string path in paths)
        {
            
            try
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

                    if (AssetUtils.IsIgnoreFilename(item))
                    {
                        continue;
                    }

                    if (item.IndexOf("_d") < 0)
                    {
                        continue;
                    }
                    AssetDatabase.ImportAsset(item, ImportAssetOptions.ForceUpdate);
                    Texture2D dtex = (Texture2D) AssetDatabase.LoadAssetAtPath(item, typeof(Texture2D));

                    if (dtex == null)
                    {
                        continue;
                    }

                    string npath = item.Replace("_d.png", "_n.png");
                    npath = npath.Replace("_d.jpg", "_n.jpg");

                    AssetDatabase.ImportAsset(npath, ImportAssetOptions.ForceUpdate);

                    Texture2D ntex = (Texture2D)AssetDatabase.LoadAssetAtPath(npath, typeof(Texture2D));

                    if (ntex == null)
                    {
                        continue;
                    }

                    string artPath = item;
                    artPath = artPath.Replace("_d.png", UnityAssetService.ArtFileSuffix);
                    artPath = artPath.Replace("_d.jpg", UnityAssetService.ArtFileSuffix);
                    artPath = artPath.Replace(texturePathSuffix + "\\", "");

                    string artName = artPath.Replace(artFolder, "");

                    GameObject prefab = GetOrCreatePrefabAtPath(artName, artPath);

                    TextureList tlist = GameObjectUtils.GetOrAddComponent<TextureList>(gs, prefab);
                    if (tlist.Textures == null)
                    {
                        tlist.Textures = new List<Texture2D>();
                    }

                    tlist.Textures.Clear();
                    tlist.Textures.Add(dtex);
                    tlist.Textures.Add(ntex);

                    PrefabUtility.SaveAsPrefabAsset(prefab, artPath);
                    GameObject.DestroyImmediate(prefab);
                    sb.Append(artName.Replace(UnityAssetService.ArtFileSuffix, "") + "\n");

                }
                AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                Debug.LogError("Exception on setup bundle files: " + e.Message + " " + e.StackTrace + " " + path);
            }
        }
    }

    /// <summary>
    /// Use this function to 
    /// </summary>
    /// <param name="path"></param>

    public static void SetupPrefabsFromDiffuseTextures(UnityGameState gs, string assetCategory)
    {

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

        string texturePathSuffix = "Textures";

        paths.Add("Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath + texturePathSuffix);


        string prefabFolder = "Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath;

        StringBuilder sb = new StringBuilder();
        foreach (string path in paths)
        {

            try
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
                    if (AssetUtils.IsIgnoreFilename(item))
                    {
                        continue;
                    }

                    if (item.IndexOf("_n") >= 0)
                    {
                        continue;
                    }
                    AssetDatabase.ImportAsset(item, ImportAssetOptions.ForceUpdate);
                    Texture2D dtex = (Texture2D)AssetDatabase.LoadAssetAtPath(item, typeof(Texture2D));

                    if (dtex == null)
                    {
                        continue;
                    }


                    string artPath = item;
                    artPath = artPath.Replace(".png", UnityAssetService.ArtFileSuffix);
                    artPath = artPath.Replace(".jpg", UnityAssetService.ArtFileSuffix);
                    artPath = artPath.Replace(texturePathSuffix + "\\", "");

                    string artName = artPath.Replace(prefabFolder, "");


                    GameObject prefab = GetOrCreatePrefabAtPath(artName, artPath);

                    TextureList tlist = GameObjectUtils.GetOrAddComponent<TextureList>(gs,prefab);
                    if (tlist.Textures == null)
                    {
                        tlist.Textures = new List<Texture2D>();
                    }

                    tlist.Textures.Clear();
                    tlist.Textures.Add(dtex);

                    PrefabUtility.SaveAsPrefabAsset(prefab, artPath);
                    GameObject.DestroyImmediate(prefab);
                    sb.Append(artName.Replace(UnityAssetService.ArtFileSuffix, "") + "\n");
                }
                AssetDatabase.SaveAssets();
                //Debug.Log(sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError("Exception on setup bundle files: " + e.Message + " " + e.StackTrace + " " + path);
            }
        }
    }

    public static void BundleRootDirectory(UnityGameState gs, string assetCategory, bool renameMaterials, bool enableGPUInstancing)
    {
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


        paths.Add("Assets/" + AssetCategory.DownloadAssetRootPath + endOfPath);
        paths.Add("Assets/Resources/" + endOfPath);

        foreach (string path in paths)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    //Debug.Log("Directory doesn't exist:" + path);
                    continue;
                }

                string[] names = new string[1];
                names[0] = path;
                // Important to remove last / from this path or it makes a new empty folder and removes the real
                // folder if you try to delete it.
                if (names[0].LastIndexOf("/") == names[0].Length - 1)
                {
                    names[0] = names[0].Substring(0, names[0].Length - 1);
                }
                int numAdded = 0;

                foreach (string item in names)
                {
                    if (AssetUtils.IsIgnoreFilename(item))
                    {
                        continue;
                    }

                    AssetDatabase.ImportAsset(item, ImportAssetOptions.ForceUpdate);

                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(item, typeof(UnityEngine.Object));

                    AssetImporter importer = AssetImporter.GetAtPath(item) as AssetImporter;
                    if (importer != null)
                    {
                        // Check it it's in an asset bundle path
                        if (item.IndexOf(AssetCategory.DownloadAssetRootPath) < 0)
                        {
                            // No. Set the asset bundle name to empty.
                            importer.assetBundleName = "";
                        }
                        else
                        {
                            // Yes. Set the asset bundle name to (... the file path?)
                            string fileName = item.Replace("Assets/" + AssetCategory.DownloadAssetRootPath, "");
                            string bid = importer.assetBundleName = _assetService.GetBundleForCategoryAndAsset(gs, assetCategory, fileName);
                            importer.assetBundleName = bid;
                            //Debug.Log("Setting BID, '" + bid + "' for '" + fileName + "'");
                        }
                        importer.SaveAndReimport();
                        numAdded++;
                    }

                    string[] allFiles = Directory.GetFiles(item);
                    if (allFiles != null)
                    {
                        foreach (string file in allFiles)
                        {
                            SetupItemAtPath(gs, false, assetCategory, path, file, enableGPUInstancing, renameMaterials, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception on setup bundle files: " + e.Message + " " + e.StackTrace + " " + path);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
