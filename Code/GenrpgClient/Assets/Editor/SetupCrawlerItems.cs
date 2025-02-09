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

                List<Sprite> textures = new List<Sprite>();

                for (int i = 1; i <= 10; i++)
                {

                    string filename = imageDirectory + unitType.Icon + i + ".png";
                    Sprite spr = (Sprite)AssetDatabase.LoadAssetAtPath(filename, typeof(Sprite));

                    if (spr == null)
                    {
                        if (i == 1)
                        {
                            Debug.LogError("Missing first sprite for " + unitType.Icon);
                        }


                        break;
                    }
                    textures.Add(spr);
                }

                GameObject go = new GameObject();
                go.name = unitType.Icon;
                SpriteList tl = go.AddComponent<SpriteList>();

                if (tl.Sprites == null)
                {
                    tl.Sprites = new List<Sprite>();
                }

                tl.Sprites = textures;
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
    }
}