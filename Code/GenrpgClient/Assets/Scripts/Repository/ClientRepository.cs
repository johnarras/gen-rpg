
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class ClientRepository<T> where T : class, IStringId
{

    private ILogSystem _logger;
    public ClientRepository(ILogSystem logger)
    {
        _logger = logger;
    }

    public virtual async Task<bool> SaveAll(List<T> list)
    {
        if (list == null)
        {
            return false;
        }

        for (int i = 0; i < list.Count; i++)
        {
            await Save(list[i]);
        }
        return true;
    }

    private string GetKeyFromId(string id)
    {
        return typeof(T).Name + id;
    }


    public async Task<T> Load(String id)
    {
        await Task.CompletedTask;
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return default(T);
            }
            string key = GetKeyFromId(id);
            string val = LoadString(key);
            if (string.IsNullOrEmpty(val))
            {
                return default(T);
            }
            return SerializationUtils.Deserialize<T>(val);
        }
        catch (Exception e)
        {
            _logger.Exception(e, "Local Load Error");
            return default(T);
        }
    }

    /// <summary>
    /// Special method for saving a string directly.
    /// </summary>
    /// <param name="id">Id to save (key)</param>
    /// <param name="data">Data to save (value)</param>
    /// <returns>Were the parameters ok? Not checking actual save success here.</returns>
    public async Task<bool> StringSave(string id, string data)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        SaveString(id, data);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> Save(T t)
    {
        if (t == null)
        {
            return false;
        }
        try
        {
            string id = "";
            if (t is IStringId tid)
            {
                id = tid.Id;
            }

            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            string key = GetKeyFromId(id);
            string val = SerializationUtils.Serialize(t);

            SaveString(key, val);
        }
        catch (Exception e)
        {
            _logger.Exception(e, "Local Save Error");
            return false;
        }
        await Task.CompletedTask;
        return true;
    }
    public async Task<bool> Delete(T t)
    {
        if (t == null)
        {
            return false;
        }
        await Task.CompletedTask;
        string id = EntityUtils.GetObjId(t);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        string key = GetKeyFromId(id);
        try
        {
            DeleteString(key);
        }
        catch (Exception e)
        {
            _logger.Exception(e, "LocalRepository.Delete");
            return false;
        }
        return true;
    }

    protected const string EDITOR_PREFIX = "Editor";



    protected static string GetPathPrefix()
    {
        string prefix = UnityAssetService.GetPerisistentDataPath() + "/Data";
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


    public static string GetPath(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "";
        }

        string basePath = GetPathPrefix();


        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }


        if (id.LastIndexOf("/") >= 0)
        {
            string beforeSlash = id.Substring(0, id.LastIndexOf("/"));
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

    public string LoadString(string id)
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


    public void SaveString(string id, string val)
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


    public void DeleteString(string id)
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

    public async Task<List<T>> LoadAll(List<string> ids)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<List<T>> Search(Expression<Func<T, bool>> func, int quantity=100, int skip = 0)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task CreateIndex(List<IndexConfig> configs)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}
