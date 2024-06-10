
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading.Tasks;

public interface IClientRepositoryCollection
{
    UniTask<bool> Save(object t);
    UniTask<bool> SavePrettyPrint(object t);
    UniTask<object> LoadWithType(Type t, string id);
}

public class ClientRepositoryCollection<T> : IClientRepositoryCollection where T : class, IStringId
{

    private ILogService _logger;
    public ClientRepositoryCollection(ILogService logger)
    {
        _logger = logger;
    }

    public virtual async UniTask<bool> SaveAll(List<T> list)
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


    public async UniTask<T> Load(String id)
    {
        await UniTask.CompletedTask;
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
    public async UniTask<bool> StringSave(string id, string data, bool verboseSave = false)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        SaveString(id, data);
        await UniTask.CompletedTask;
        return true;
    }

    public async UniTask<bool> Save(object t)
    {
        return await SaveInternal(t, false);
    }

    public async UniTask<bool> SavePrettyPrint(object t)
    {
        return await SaveInternal(t, true);
    }

    private async UniTask<bool> SaveInternal(object t, bool savePrettyPrint)
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
            string val = (savePrettyPrint ? SerializationUtils.PrettyPrint(t) : SerializationUtils.Serialize(t));

            SaveString(key, val);
        }
        catch (Exception e)
        {
            _logger.Exception(e, "Local Save Error");
            return false;
        }
        await UniTask.CompletedTask;
        return true;
    }
    public async UniTask<bool> Delete(T t)
    {
        if (t == null)
        {
            return false;
        }
        await UniTask.CompletedTask;
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


    private string GetPath(string id)
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

    public async UniTask<List<T>> LoadAll(List<string> ids)
    {
        await UniTask.CompletedTask;
        throw new NotImplementedException();
    }

    public async UniTask<List<T>> Search(Expression<Func<T, bool>> func, int quantity=100, int skip = 0)
    {
        await UniTask.CompletedTask;
        throw new NotImplementedException();
    }

    public async UniTask<object> LoadWithType(Type t, string id)
    {
        await UniTask.CompletedTask;
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            string key = GetKeyFromId(id);
            string val = LoadString(key);
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            return SerializationUtils.DeserializeWithType(val, t);
        }
        catch (Exception e)
        {
            _logger.Exception(e, "Local LoadWithType Error");
            return default(T);
        }

    }
}
