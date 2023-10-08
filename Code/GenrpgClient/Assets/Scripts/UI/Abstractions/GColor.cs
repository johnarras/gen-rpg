

using UnityEngine; // Needed

public static class GColor
{
    public static Color Create(float r, float g, float b, float a = 1)
    {
        return new Color(r, g, b, a);
    }

    public static Color red => Color.red; 
    public static Color green => Color.green; 
    public static Color blue => Color.blue;

    public static Color white => Color.white;
    public static Color black => Color.black;
    public static Color gray => Color.gray;

    public static Color yellow => Color.yellow;
    public static Color cyan => Color.cyan;
    public static Color magenta => Color.magenta;

}