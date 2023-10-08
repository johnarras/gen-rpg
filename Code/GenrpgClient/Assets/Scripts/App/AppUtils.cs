
using UnityEngine; // Needed
public class AppUtils
{
    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public static int TargetFrameRate
    {
        get { return Application.targetFrameRate; }
        set { Application.targetFrameRate = value; }
    }

    public static string DataPath { get { return Application.dataPath; } }

    public static string PersistentDataPath {  get { return Application.persistentDataPath; } }

    public static bool IsPlaying { get { return Application.isPlaying; } }

    public static void OpenUrl(string url) { Application.OpenURL(url); }

    public static string Version { get { return Application.version; } }

    public static string Platform { get { return Application.platform.ToString(); } }

    public static string DeviceUniqueIdentifier { get { return SystemInfo.deviceUniqueIdentifier; } }
}
 


