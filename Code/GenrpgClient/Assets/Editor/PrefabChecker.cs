using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using Genrpg.Shared.Client.Core;


public delegate void CheckPrefab (GameObject go, string path, System.Object returnData, StringBuilder logTxt);


public class PrefabChecker
{
	public static void CheckData (System.Object extraData, StringBuilder logTxt, CheckPrefab checker)
	{
		if (logTxt == null || checker == null)
		{
			return;
        }
        IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

        IClientAppService _appService = gs.loc.Get<IClientAppService>();

        string dataPath = _appService.DataPath;

        CheckPath("/", extraData, logTxt, checker, dataPath);

		string txt = logTxt.ToString();
		Debug.Log(txt);
		return;
	}


	
	public static void CheckPath (string path, System.Object extraData, StringBuilder logTxt, CheckPrefab checker, string dataPath)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string fullPath = dataPath + path;
		string[] fileEntries = Directory.GetFiles(fullPath);
		string[] directories = Directory.GetDirectories(fullPath);
		foreach (string entry in fileEntries)
		{
			if (entry.IndexOf(".prefab") < 0 || entry.IndexOf(".meta") >= 0)
			{
				continue;
			}
            string newEntry = "Assets" + entry.Replace(dataPath, "");
			GameObject go = null;
			try
			{
				go = AssetDatabase.LoadAssetAtPath<GameObject>(newEntry);
			
			}
			catch (Exception e)
			{
				Debug.Log("failed to load object at path: " + newEntry + " " + e.Message + " " + e.StackTrace);
			}
			if (go != null)
			{
				CheckGameObject(go, newEntry, extraData, logTxt, checker);
			}
		}

		foreach (string dir in directories)
		{
			CheckPath(dir.Replace(dataPath, ""), extraData, logTxt, checker, dataPath);
		}
	}

	public static void CheckGameObject (GameObject go, string path, System.Object extraData, StringBuilder sb, CheckPrefab checker)
	{
		if (go == null || sb == null || checker == null)
		{
			return;
		}

		checker(go, path, extraData, sb);

		for (int c = 0; c < go.transform.childCount; c++)
		{
			CheckGameObject(go.transform.GetChild(c).gameObject, path, extraData, sb, checker);
		}
	}
}

