using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Editor
{
    public class SetupCrawlerItems
    {


        [MenuItem("Tools/CopyMonsterTextures")]
        static void CopyMonsterTextures()
        {
            return;
            //IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
            //IClientRandom clientRandom = new ClientRandom();
            //IGameData gameData = gs.loc.Get<IGameData>();
            //IReadOnlyList<UnitType> unitTypes = gameData.Get<UnitSettings>(null).GetData();

            //string directory = "Assets/FullAssets/Crawler/Images/Monsters/";


            //foreach (UnitType unitType in unitTypes)
            //{
            //    if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
            //    {
            //        continue;
            //    }

            //    for (int i = 1; i <= 4; i++)
            //    {
            //        File.Copy(directory + "Base" + i + ".png", directory + unitType.Icon + +i + ".png", true);
            //    }
            //}
        }

        [MenuItem("Tools/SetupMonsterImagePrefabs")]
        static void OldSetupMonsterImagePrefabs()
        {
            IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

            IGameData gameData = gs.loc.Get<IGameData>();
            IReadOnlyList<UnitType> unitTypes = gameData.Get<UnitSettings>(null).GetData();

            IClientRandom clientRandom = new ClientRandom();
            string imageDirectory = "Assets/FullAssets/Crawler/Images/Monsters/";
            string prefabDirectory = AssetConstants.DownloadAssetRootPath + "TextureLists/";

            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }

                string startTexPrefix = imageDirectory + unitType.Icon;
                string targetFile = prefabDirectory + unitType.Icon + ".prefab";

                if (File.Exists(targetFile))
                {
                    //continue;
                }

                List<Texture2D> textures = new List<Texture2D>();

                for (int i = 1; i <= 10; i++)
                {

                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(imageDirectory + unitType.Icon + i + ".png");

                   

                    if (tex == null)
                    {
                        if (i == 1)
                        {
                            Debug.LogError("MISSING first image for unit with image name: " + unitType.Icon);
                            return;
                        }
                        break;
                    }
                    textures.Add(tex);
                }

                GameObject go = new GameObject();
                go.name = unitType.Icon;
                TextureList tl = go.AddComponent<TextureList>();

                if (tl.Textures == null)
                {
                    tl.Textures = new List<Texture2D>();
                }

                tl.Textures = textures;
                PrefabUtility.SaveAsPrefabAsset(go, targetFile);
                GameObject.DestroyImmediate(go);
            }
        }


        [MenuItem("Tools/SetupMonster3DPrefabs")]
        static void SetupMonster3DPrefabs()
        {
            IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

            IGameData gameData = gs.loc.Get<IGameData>();
            IReadOnlyList<UnitType> unitTypes = gameData.Get<UnitSettings>(null).GetData();

            string startLoc = "Assets/FullAssets/Monsters/Bear/Monster1.prefab";
            GameObject startGo = AssetDatabase.LoadAssetAtPath<GameObject>(startLoc);

            if (startGo == null)
            {
                return;
            }

            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Art))
                {
                    continue;
                }

                string targetFile = AssetConstants.DownloadAssetRootPath + "Monsters/" + unitType.Art + ".prefab";

                if (File.Exists(targetFile))
                {
                    continue;
                }

                GameObject endGo = GameObject.Instantiate(startGo);
                endGo.name = unitType.Art;
                PrefabUtility.SaveAsPrefabAsset(endGo, targetFile);

                GameObject.DestroyImmediate(endGo);

            }

        }


        [MenuItem("Tools/SharpenMonsterImageEdges")]
        static void SharpenMonstarImageEdges()
        {
            IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

            string imageDirectory = "Assets/FullAssets/Crawler/Images/Monsters/";

            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);  
            }

            string[] files = Directory.GetFiles(imageDirectory);

            foreach (string filePath in files)
            {
                if (filePath.LastIndexOf(".png") != filePath.Length - 4)
                {
                    continue;
                }


                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);

                Color[] oldColors = tex.GetPixels(0, 0, tex.width, tex.height);

                Color[] newColors = new Color[oldColors.Length];

                Color transparent = new Color(0, 0, 0, 0);

                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        bool nextToTransparent = false;
                        for (int xx = Math.Max(x-1, 0); xx <= Math.Min(x+1,tex.width-1); xx++)
                        {
                            for (int yy = Math.Max(y-1,0); yy <= Math.Min(y+1,tex.height-1); yy++)
                            {
                                Color oldColor = oldColors[GetTextureIndex(xx, yy, tex.width)];

                                if (oldColor.a == 0)
                                {
                                    nextToTransparent = true;
                                    break;
                                }
                            }
                        }

                        int index = GetTextureIndex(x, y, tex.width);
                        newColors[index] = (nextToTransparent ? transparent : oldColors[index]);
                    }
                }
                tex.SetPixels(newColors);
                tex.Apply(); 
                byte[] bytes = tex.EncodeToPNG(); 
                //File.WriteAllBytes(filePath, bytes); 
                AssetDatabase.ImportAsset(filePath); 
                AssetDatabase.Refresh();
            }
            IGameData gameData = gs.loc.Get<IGameData>();
            IReadOnlyList<UnitType> unitTypes = gameData.Get<UnitSettings>(null).GetData();

            string startLoc = "Assets/FullAssets/Monsters/Bear/Monster1.prefab";
            GameObject startGo = AssetDatabase.LoadAssetAtPath<GameObject>(startLoc);

            if (startGo == null)
            {
                return;
            }

            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Art))
                {
                    continue;
                }

                string targetFile = AssetConstants.DownloadAssetRootPath + "Monsters/" + unitType.Art + ".prefab";

                if (File.Exists(targetFile))
                {
                    continue;
                }

                GameObject endGo = GameObject.Instantiate(startGo);
                endGo.name = unitType.Art;
                PrefabUtility.SaveAsPrefabAsset(endGo, targetFile);

                GameObject.DestroyImmediate(endGo);

            }


            static int GetTextureIndex(int x, int y, int textureWidth)
            {
                return x + y * textureWidth;
            }

        }
    }
}
