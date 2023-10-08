
public class LayerUtils
{
    public static int GetMask(params string[] args)
    {
        return UnityEngine.LayerMask.GetMask(args);
    }

    public static int NameToLayer(string layerName)
    {
        return UnityEngine.LayerMask.NameToLayer(layerName);
    }

}