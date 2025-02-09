using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spawns.Entities;
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
    public class DesaturateMonsterCrawlerImages
    {


      
        [MenuItem("Tools/DesaturateCrawlerMonsterImages")]
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

                Sprite tex = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);

                int width = tex.texture.width;
                int height = tex.texture.height;
                Color[] oldColors = tex.texture.GetPixels(0, 0, width, height);

                Color[] newColors = new Color[oldColors.Length];

                Color transparent = new Color(0, 0, 0, 0);

                Color cornerColor = tex.texture.GetPixel(0, 0);

                float desaturatePercent = 0.25f;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {

                        int index = GetTextureIndex(x, y, width);
                        Color oldColor = oldColors[index];

                        float averageColor = (oldColor.r + oldColor.g + oldColor.b) / 3;

                        float rdiff = oldColor.r - averageColor;
                        float gdiff = oldColor.g - averageColor;    
                        float bdiff = oldColor.b - averageColor;

                        float rnew = averageColor + rdiff * (1 - desaturatePercent);
                        float gnew = averageColor + gdiff * (1 - desaturatePercent);
                        float bnew = averageColor + bdiff * (1 - desaturatePercent);

                        newColors[index] = new Color(rnew, gnew, bnew, oldColor.a);
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


            static int GetTextureIndex(int x, int y, int textureWidth)
            {
                return x + y * textureWidth;
            }

        }
    }
}
