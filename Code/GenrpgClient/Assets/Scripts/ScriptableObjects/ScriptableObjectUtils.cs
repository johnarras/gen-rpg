using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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
