using Assets.Scripts.Assets;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public interface ICursorService : IInjectable
{
    void SetCursor(string cursorName);
}

public class CursorNames
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
}

public class CursorService : ICursorService
{

    private ILocalLoadService _localLoadService;
    
    private Dictionary<string, Texture2D> _cursorCache = new Dictionary<string,Texture2D>();

    private Vector2 _cursorHotspot = new Vector2(-16, 16);
    public void SetCursor(string cursorName)
    {
       
        if (!_cursorCache.TryGetValue(cursorName, out Texture2D tex))
        {
            tex = _localLoadService.LocalLoad<Texture2D>("Cursors/Cursor_" + cursorName);
            if (tex == null)
            {
                return;
            }
            _cursorCache[cursorName] = tex; 
        }

        Cursor.SetCursor(tex, _cursorHotspot, CursorMode.Auto);
    }

}
