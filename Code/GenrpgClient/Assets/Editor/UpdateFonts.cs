using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngineInternal;

namespace Assets.Editor
{
    public class UpdateFonts
    {
        [MenuItem("Tools/UpdateFonts")]
        public static void UpdateAllFonts()
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/DefaultFont");

            if (font == null)
            {
                Console.WriteLine("Couldn't find Fonts/Default font.");
                return;
            }

            string[] files = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string file2 = file.Replace(Application.dataPath, "");
                file2 = file2.Replace("\\", "/");
                file2 = "Assets" + file2;
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(file2);   

                if (obj == null)
                {
                    continue;
                }

                GText[] allText = obj.GetComponentsInChildren<GText>();

                if (allText.Length > 0)
                {
                    foreach (GText gtext in allText)
                    {
                        gtext.font = font;
                    }
                    EditorUtility.SetDirty(obj);
                    AssetDatabase.SaveAssetIfDirty(obj);
                    GameObject.DestroyImmediate(obj);
                }
            }

        }
    }
}
