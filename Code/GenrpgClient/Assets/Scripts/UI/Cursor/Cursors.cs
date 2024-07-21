using System.Collections.Generic;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

public class Cursors
{

    public const string Alchemy = "Alchemy";
    public const string Blacksmith = "Blacksmith";
    public const string Build = "Build";
    public const string Chat = "Chat";
    public const string Default = "Default";
    public const string Fight = "Fight";
    public const string Herbalism = "Herbalism";
    public const string Interact = "Interact";
    public const string Lumberjack = "Lumberjack";
    public const string Mining = "Mining";
    public const string Shop = "Shop";



    private static Dictionary<string, Texture2D> _cursorCache = new Dictionary<string, Texture2D>();

    private static readonly Vector2 _cursorHotspot = new Vector2(-16, 16);
    public static void SetCursor(string cursorName)
    {
        if (string.IsNullOrEmpty(cursorName))
        {
            return;
        }

        if (!_cursorCache.ContainsKey(cursorName))
        {
            // This has to get done before the game is set up, so use resources for now.
            Texture2D newCursor = AssetUtils.LoadResource<Texture2D>("Cursors/Cursor_" + cursorName);
            if (newCursor == null)
            {
                return;
            }
            _cursorCache[cursorName] = newCursor;
        }

        Texture2D tex = _cursorCache[cursorName];
        Cursor.SetCursor(tex, _cursorHotspot, CursorMode.Auto);
    }

}
