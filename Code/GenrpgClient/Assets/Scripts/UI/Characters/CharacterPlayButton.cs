using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;


using Services;

using UnityEngine;
using UnityEngine.UI;
using Services.ProcGen;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using UI.Screens.Constants;

public class CharacterPlayButton : BaseBehaviour
{
    protected IZoneGenService _zoneGenService;
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Text _text;

    private string _mapId;
    private string _charId;

    public void Init(string charId, string mapId, IScreen screen)
    {
        _mapId = mapId;
        _charId = charId;

        UIHelper.SetButton(_button, screen.GetAnalyticsName(), ClickPlay);
        UIHelper.SetText(_text, "Play " + _mapId);
    }

    public void ClickPlay()
    {
        LoadIntoMapCommand lwd = new LoadIntoMapCommand() { Env= _gs.Env, MapId = _mapId, CharId = _charId, GenerateMap = false };
        _zoneGenService.LoadMap(_gs, lwd);
    }
}
