using Assets.Scripts.Assets.Textures;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.ProcGen.Settings.Textures;
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
    public class SetupBundleItems
    {


        [MenuItem("Tools/SetupTerrainImages")]
        static void SetupTerrainImagePrefabs()
        {
            IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

            IGameData gameData = gs.loc.Get<IGameData>();

            IReadOnlyList<TextureType> textureTypes = gameData.Get<TextureTypeSettings>(null).GetData();

            IClientRandom clientRandom = new ClientRandom();
            string imageDirectory = "Assets/FullAssets/TerrainTex/Textures/";
            string prefabDirectory = AssetConstants.DownloadAssetRootPath + "TerrainTex/";

            foreach (TextureType textureType in textureTypes)
            {
                if (textureType.IdKey < 1 || string.IsNullOrEmpty(textureType.Art))
                {
                    continue;
                }

                string startTexPrefix = imageDirectory + textureType.Art;
                string targetFile = prefabDirectory + textureType.Art + ".prefab";

                if (File.Exists(targetFile))
                {
                    //continue;
                }

                List<Sprite> textures = new List<Sprite>();

                string baseFilename = imageDirectory + textureType.Art;

                string diffuseName = baseFilename + "_d.png";
                string normalName = baseFilename + "_n.png";

                Texture2D diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(diffuseName);   
                Texture2D normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (normalName);

                if (diffuseTexture == null || normalTexture == null)
                {
                    Debug.LogError("Missing textures for " + textureType.Art);
                    continue;
                }

                GameObject go = new GameObject();
                go.name = textureType.Art;
                TextureList tl = go.AddComponent<TextureList>();

                tl.Textures = new List<Texture2D>();
                tl.Textures.Add(diffuseTexture);
                tl.Textures.Add(normalTexture);
                PrefabUtility.SaveAsPrefabAsset(go, targetFile);
                GameObject.DestroyImmediate(go);
            }
        }
    }
}
