﻿using Genrpg.Shared.Constants;
using Genrpg.Shared.Units.Entities;
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


        [MenuItem("Build/CopyMonsterTextures")]
        static void ExecuteLocal()
        {
            UnityGameState gs = SetupEditorUnityGameState.Setup(null);

            IReadOnlyList<UnitType> unitTypes = gs.data.Get<UnitSettings>(null).GetData();

            string directory = "Assets/FullAssets/Monsters/Images/";

            
            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }

                int val = gs.rand.Next(1, 5);

                string targetFile = directory + unitType.Icon + ".png";

                if (File.Exists(targetFile))
                {
                    continue;
                }

                File.Copy(directory + "Full" + val + ".png", targetFile);

            }
        }

        [MenuItem("Build/SetupPrefabs")]
        static void ExecutePrefabs()
        {
            UnityGameState gs = SetupEditorUnityGameState.Setup(null);

            IReadOnlyList<UnitType> unitTypes = gs.data.Get<UnitSettings>(null).GetData();

            string directory = "Assets/FullAssets/Monsters/Images/";


            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }

                int val = gs.rand.Next(1, 5);

                string startTex = directory + unitType.Icon + ".png";
                string targetFile = directory + unitType.Icon + ".prefab";

                if (File.Exists(targetFile))
                {
                    continue;
                }

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(startTex);

                if (tex == null)
                {
                    continue;
                }

                GameObject go = new GameObject();
                go.name = unitType.Icon;
                TextureList tl = go.AddComponent<TextureList>();

                if (tl.Textures == null)
                {
                    tl.Textures = new List<Texture2D>();
                }
                tl.Textures.Add(tex);

                PrefabUtility.SaveAsPrefabAsset(go, targetFile);
                GameObject.DestroyImmediate(go);

            }
        }
    }
}