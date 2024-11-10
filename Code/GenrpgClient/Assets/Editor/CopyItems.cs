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
using UnityEngine;

namespace Assets.Editor
{
    public class CopyItems
    {


        [MenuItem("Tools/CopyMonsterTextures")]
        static void ExecuteLocal()
        {
            IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
            IClientRandom clientRandom = new ClientRandom();
            IGameData gameData = gs.loc.Get<IGameData>();
            IReadOnlyList<UnitType> unitTypes = gameData.Get<UnitSettings>(null).GetData();

            string directory = "Assets/FullAssets/Crawler/Images/Monsters/";


            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }

                for (int i = 1; i <= 4; i++)
                {
                    File.Copy(directory + "Base" + i + ".png", directory + unitType.Icon + +i + ".png", true);
                }
            }
        }

        [MenuItem("Tools/SetupMonsterImagePrefabs")]
        static void OldSetupMonsterImagePrefabs()
        {
            IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

            IGameData gameData = gs.loc.Get<IGameData>();
            IReadOnlyList<UnitType> unitTypes = gameData.Get<UnitSettings>(null).GetData();

            IClientRandom clientRandom = new ClientRandom();
            string imageDirectory = "Assets/FullAssets/Crawler/Images/Monsters/";
            string prefabDirectory = "Assets/BundledAssets/TextureLists/";

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

                string targetFile = "Assets/BundledAssets/Monsters/" + unitType.Art + ".prefab";

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
    }
}
