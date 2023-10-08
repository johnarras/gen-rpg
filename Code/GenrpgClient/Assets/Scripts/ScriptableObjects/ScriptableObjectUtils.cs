#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine; // Needed

public class ScriptableObjectUtils
{
    const string _baseConfigPath = "Assets/Config/";

#if UNITY_EDITOR
    public static void CreateBasicInstance<T>() where T : ScriptableObject
    {

        string classname = typeof(T).Name;
        string fullPath = _baseConfigPath + classname + ".asset";
        ScriptableObject so = ScriptableObject.CreateInstance(typeof(T));
        AssetDatabase.CreateAsset(so, fullPath);
        AssetDatabase.Refresh();
    }
#endif
}
