using Genrpg.Shared.Logs.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.IO;
using UnityEngine.SearchService;

public class LocalFileRepository
{

    protected const string EDITOR_PREFIX = "Editor";

    private ILogSystem _logger;
    public LocalFileRepository (ILogSystem logger)
    {
        _logger = logger;
    }

    protected static string GetPathPrefix()
    {
        string prefix = AssetUtils.GetPerisistentDataPath() + "/Data";
#if DEMO_BUILD
        if (InitProject.Env != EnvNames.Prod && !string.IsNullOrEmpty(Application.version))
        {
            var version = Application.version.Trim();
            prefix += "V" + version;
        }
#endif
        if (!Directory.Exists(prefix))
        {
            Directory.CreateDirectory(prefix);
        }

        return prefix;
    }


    public string GetPath(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "";
        }

        if (id.IndexOf(":") > 0)
        {
            return id;
        }

        string basePath = GetPathPrefix();


        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }


        if (id.LastIndexOf("/") >= 0)
        {
            string beforeSlash = id.Substring(0,id.LastIndexOf("/"));
            if (!string.IsNullOrEmpty(beforeSlash))
            {
                string fullDir = basePath + "/" + beforeSlash;
                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }
            }
        }
        else
        {
#if UNITY_EDITOR
                id = EDITOR_PREFIX + id;
#endif
        }
        return basePath + "/" + id;
    }


    public void DeleteAllData()
    {
        string pathPrefix = GetPathPrefix();
        if (Directory.Exists(pathPrefix))
        {
            Directory.Delete(pathPrefix, true);
        }
    }

    public string Load (string id)
    {
        string path = GetPath(id);
        try
		{
            if (!File.Exists(path))
            {
                return "";
            }
            return File.ReadAllText(path, System.Text.Encoding.UTF8);
		}
		catch (Exception e) 
		{ 
            _logger.Info("Failed to read file: " + path + " " + e.Message); 
		}
        return "";
	}

    public byte[] LoadBytes(string id)
    {
        string path = GetPath(id);
        
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }
            return File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            _logger.Info("Failed to read bytes: " + " " + path + " " + e.Message);
        }
        return null;
    }


    public void Save(string id, string val)
    {
        string path = GetPath(id);
        try
		{
            File.WriteAllText(path, val, System.Text.Encoding.UTF8);
		}
		catch (Exception e)
		{
		    _logger.Info("Failed to save text file: " + path + " " + e.Message);
		}
	}

    public void SaveBytes(string id, byte[] val)
    {
        if (val == null)
        {
            return;
        }
        string path = GetPath(id);
        try
        {
            File.WriteAllBytes(path, val);
        }
        catch (Exception e)
        {
            _logger.Info("Failed to save bytes: " + path + " " + e.Message);
        }
    }


    public void Delete (string id)
    {
        string path = GetPath(id);
        try
		{
            File.Delete(path);
		}
		catch (Exception e)
		{
		    _logger.Info("Failed to delete file: " + path + " " + e.Message);
		}
	}

    public void SaveObject(string fileName, object obj)
    {
        if (obj == null)
        {
            return;
        }
        Save(fileName, SerializationUtils.Serialize(obj));
    }

    public T LoadObject<T>(string fileName) where T : class
    {
        return SerializationUtils.TryDeserialize<T>(Load(fileName));
    }
}
