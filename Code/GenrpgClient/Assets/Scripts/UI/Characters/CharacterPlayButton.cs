
using Assets.Scripts.PlayerSearch;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Website.Messages.LoadIntoMap;
using Newtonsoft.Json.Linq;

public class CharacterPlayButton : BaseBehaviour
{
    public GButton PlayButton;
    public GText CharText;

    protected IZoneGenService _zoneGenService;
    protected IPlayerSearchService _playerSearchService;

    private string _mapId;
    private string _charId;

    public void Init(string charId, string mapId, IScreen screen)
    {
        _mapId = mapId;
        _charId = charId;

        _uIInitializable.SetButton(PlayButton, screen.GetName(), ClickPlay);
        _uIInitializable.SetText(CharText, "Play " + _mapId);
    }

    public void ClickPlay()
    {
        _playerSearchService.AccountSearch(_gs.user.Id,
           (PublicAccount acct) =>
           {
               if (acct != null)
               {
                   _logService.Info("Acct: " + acct.Id + " -- " + acct.Name);
               }
               else
               {
                   _logService.Info("Missing account");
               }
           },
           GetToken());

        _playerSearchService.UserSearch(_gs.user.Id,
              (PublicUser user) =>
              {
                  if (user != null)
                  {
                      _logService.Info("PUser: " + user.Id + " -- " + user.Name);
                  }
                  else
                  {
                      _logService.Info("Missing PUser");
                  }
              },
            GetToken());


        _playerSearchService.CharacterSearch(_charId,
              (PublicCharacter pchar) =>
              {
                  if (pchar != null)
                  {
                      _logService.Info("PChar: " + pchar.Id + " -- " + pchar.Name);
                  }
                  else
                  {
                      _logService.Info("Missing PChar");
                  }
              },
            GetToken());


        LoadIntoMapCommand lwd = new LoadIntoMapCommand() { Env= _gs.Config.Env, MapId = _mapId, CharId = _charId, GenerateMap = false };
        _zoneGenService.LoadMap(lwd);
    }
}
