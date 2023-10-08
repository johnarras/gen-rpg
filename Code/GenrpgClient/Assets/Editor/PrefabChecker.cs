using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;


public delegate void CheckPrefab (GameObject go, string path, System.Object returnData, StringBuilder logTxt);


public class PrefabChecker
{
	public static void CheckData (System.Object extraData, StringBuilder logTxt, CheckPrefab checker)
	{
		if (logTxt == null || checker == null)
		{
			return;
		}

		CheckPath("/", extraData, logTxt, checker);

		string txt = logTxt.ToString();
		Debug.Log(txt);
		return;
	}


	
	public static void CheckPath (string path, System.Object extraData, StringBuilder logTxt, CheckPrefab checker)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string fullPath = AppUtils.DataPath + path;
		string[] fileEntries = Directory.GetFiles(fullPath);
		string[] directories = Directory.GetDirectories(fullPath);
		foreach (string entry in fileEntries)
		{
			if (entry.IndexOf(".prefab") < 0 || entry.IndexOf(".meta") >= 0)
			{
				continue;
			}
            string newEntry = "Assets" + entry.Replace(AppUtils.DataPath, "");
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
			CheckPath(dir.Replace(AppUtils.DataPath, ""), extraData, logTxt, checker);
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

