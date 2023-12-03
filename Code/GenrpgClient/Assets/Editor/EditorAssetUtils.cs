using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class EditorAssetUtils
{
	public static void DeleteAllFiles (string path)
	{
		try
		{
            string[] files = Directory.GetFiles(path);
			foreach (string file in files)
			{
				try
				{
					File.Delete(file);
				}
				catch (Exception ee)
				{
					Debug.Log("Inner DeleteAllFiles:" + ee.Message);
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Outer DeleteAllFiles: " + e.Message);

		}

	}

	public static void DeleteAllDirectories(string path)
	{
		try
		{
            string[] dirs = Directory.GetDirectories(path);
			foreach (string dir in dirs)
			{
				try
				{
					Directory.Delete(dir);
				}
				catch (Exception ee)
				{
					Debug.Log("Inner DeleteAllDirs: " + ee.Message);
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Outer DeleteAllDirs: " + e.Message);
		}
	
	}

    public static bool IsIgnoreFilename(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return true;
        }

        if (name.LastIndexOf(".prefab") < 0 || name.LastIndexOf(".prefab") != name.Length-7)
        {
            return true;
        }

        if (name.LastIndexOf(".meta") == name.Length-5 || name.IndexOf("Thumbs.db") >= 0 ||
            name.IndexOf(".spriteatlas") >= 0)
        {
            return true;
        }

        return false;
    }

}
