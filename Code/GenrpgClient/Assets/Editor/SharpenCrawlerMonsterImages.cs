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
    public class SharpenMonsterCrawlerImages
    {


      
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
                if (filePath.LastIndexOf(".jpg") != filePath.Length - 4)
                {
                    continue;
                }

                Sprite tex = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);

                int width = tex.texture.width;
                int height = tex.texture.height;
                Color[] oldColors = tex.texture.GetPixels(0, 0, width, height);

                Color[] newColors = new Color[oldColors.Length];

                Color transparent = new Color(0, 0, 0, 0);

                Color cornerColor = tex.texture.GetPixel(0, 0);


                float maxDiff = 0.05f;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        bool nextToOldColor = false;
                        for (int xx = Math.Max(x-1, 0); xx <= Math.Min(x+1,width-1); xx++)
                        {
                            for (int yy = Math.Max(y-1,0); yy <= Math.Min(y+1,height -1); yy++)
                            {
                                Color oldColor = oldColors[GetTextureIndex(xx, yy, width)];

                                if (Math.Abs(oldColor.r-cornerColor.r) <= maxDiff &&
                                    Math.Abs(oldColor.g-cornerColor.g) <= maxDiff &&
                                    Math.Abs(oldColor.b-cornerColor.b) <= maxDiff)
                                {
                                    nextToOldColor = false;
                                    break;
                                }
                            }
                        }

                        int index = GetTextureIndex(x, y, width);
                        newColors[index] = (nextToOldColor ? transparent : oldColors[index]);
                    }
                }

                Texture2D newTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                newTex.SetPixels(newColors);
                newTex.Apply();
                byte[] bytes = newTex.EncodeToPNG();
              
                string newFilePath = filePath.Replace(".jpg", ".png");
                File.WriteAllBytes(newFilePath, bytes);
                AssetDatabase.ImportAsset(filePath);
                AssetDatabase.ImportAsset(newFilePath);
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
