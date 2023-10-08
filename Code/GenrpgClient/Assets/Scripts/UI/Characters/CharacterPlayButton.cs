
using Genrpg.Shared.Login.Messages.LoadIntoMap;

public class CharacterPlayButton : BaseBehaviour
{
    public GButton PlayButton;
    public GText CharText;

    protected IZoneGenService _zoneGenService;

    private string _mapId;
    private string _charId;

    public void Init(string charId, string mapId, IScreen screen)
    {
        _mapId = mapId;
        _charId = charId;

        UIHelper.SetButton(PlayButton, screen.GetAnalyticsName(), ClickPlay);
        UIHelper.SetText(CharText, "Play " + _mapId);
    }

    public void ClickPlay()
    {
        LoadIntoMapCommand lwd = new LoadIntoMapCommand() { Env= _gs.Env, MapId = _mapId, CharId = _charId, GenerateMap = false };
        _zoneGenService.LoadMap(_gs, lwd);
    }
}
