using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using Assets.Scripts.Atlas.Constants;
using System.Threading;
using UnityEngine;
using UI.Screens.Constants;

public class InitActionIconData : InitSpellIconData
{
    public int actionIndex = -1;
}

public class ActionButton : SpellIcon
{
    Spell _spell = null;
    private int _actionIndex = -1;
    public int ActionIndex => _actionIndex;
    DateTime cooldownStart;
    DateTime cooldownEnd;

    [SerializeField]
    private Image _tint;
    [SerializeField]
    private Text _keyBind;
    [SerializeField]
    private Text _charges;


    public override void Init(InitSpellIconData spellIconData, CancellationToken token)
    {
        InitActionIconData initData = spellIconData as InitActionIconData;
        if (initData == null)
        {
            return;
        }
        name = name + initData.actionIndex;
        UIHelper.SetButton(_selfButton, spellIconData.Screen.GetAnalyticsName(), ClickButton);
        base.Init(spellIconData, token); 
        if (_gs.ch == null)
        {
            return;
        }
        _actionIndex = initData.actionIndex;
        _initData = initData;
        _spell = null;

        if (!InputConstants.OkActionIndex(ActionIndex))
        {
            return;
        }

        ActionInputData actionInputs = _gs.ch.Get<ActionInputData>();
        ActionInput actionItem = actionInputs.GetInput(ActionIndex);

        if (actionItem != null)
        {
            _spell = _gs.ch.Get<SpellData>().Get(actionItem.SpellId);

        }
        initData.Data = _spell;

        UIHelper.SetText(_keyBind, "");

        KeyCommData keyCommData = _gs.ch.Get<KeyCommData>();

        KeyComm keyCode = keyCommData.GetKeyComm(KeyComm.ActionPrefix + ActionIndex);

        if (keyCode != null)
        {
            UIHelper.SetText(_keyBind, keyCode.ShowName());
        }

        string iconName = ItemConstants.BlankIconName;
        if (_spell != null)
        {
            iconName = _spell.Icon;
        }

        _assetService.LoadSpriteInto(_gs, AtlasNames.SkillIcons, iconName, _icon, _token);

        if (_tint != null && _spell == null)
        {
            _tint.fillAmount = 0;
        }

        UIHelper.SetText(_charges, "");
    }

    protected void ClickButton()
    {

    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!InputConstants.OkActionIndex(ActionIndex))
        {
            return;
        }

        if (InputService.Instance.ModifierIsActive(_gs, KeyComm.ShiftName))
        {
            base.OnPointerDown(eventData);
            return;
        }
    }

    public void SetCooldown(UnityGameState gs, Character ch)
    {
        if (ch != gs.ch || _spell == null)
        {
            return;
        }

        if (cooldownEnd > DateTime.UtcNow)
        {
            return;
        }

        cooldownStart = DateTime.UtcNow;
        if (gs.ch.GlobalCooldownEnds > _spell.CooldownEnds)
        {
            cooldownEnd = gs.ch.GlobalCooldownEnds;
        }
        else
        {
            cooldownEnd = _spell.CooldownEnds;
        }
    }

    int _lastCharges = -1;
    float _lastFillAmount = -1;
    public void UpdateCooldown()
    {
        if (_tint == null || _spell == null)
        {
            return;
        }

        if (_spell.MaxCharges > 1)
        {
            if (_spell.CurrCharges != _lastCharges)
            {
                _lastCharges = _spell.CurrCharges;
                UIHelper.SetText(_charges, _lastCharges.ToString());

                if (_spell.CurrCharges < _spell.MaxCharges && _spell.CooldownEnds > DateTime.UtcNow)
                {
                    SetCooldown(_gs, _gs.ch);
                }
            }
        }


        if (cooldownEnd <= DateTime.UtcNow)
        {
            _tint.fillAmount = 0;
            _lastFillAmount = 0;
            return;
        }
        if (cooldownStart >= DateTime.UtcNow)
        {
            _tint.fillAmount = 1;
            _lastFillAmount = 1;
            return;
        }
        double totalSeconds = (cooldownEnd - cooldownStart).TotalSeconds;
        if (totalSeconds <= 0)
        {
            _tint.fillAmount = 0;
            _lastFillAmount = 0;
            return;
        }

        float oldFillAmount = _tint.fillAmount;
        float pctComplete = MathUtils.Clamp(0, (float)((DateTime.UtcNow - cooldownStart).TotalSeconds / totalSeconds), 1);
        _tint.fillAmount = 1 - pctComplete;
        _lastFillAmount = 1 - pctComplete;

    }
}