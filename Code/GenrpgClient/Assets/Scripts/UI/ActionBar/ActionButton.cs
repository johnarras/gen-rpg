using System;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Assets.Scripts.Atlas.Constants;
using System.Threading;
using GPointerEventData = UnityEngine.EventSystems.PointerEventData;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Inventory.Constants;

public class InitActionIconData : InitSpellIconData
{
    public int actionIndex = -1;
}

public class ActionButton : SpellIcon
{
    public GImage Tint;
    public GText KeyBind;
    public GText Charges;

    Spell _spell = null;
    private int _actionIndex = -1;
    public int ActionIndex => _actionIndex;
    DateTime cooldownStart;
    DateTime cooldownEnd;

    

    public override void Init(InitSpellIconData spellIconData, CancellationToken token)
    {
        InitActionIconData initData = spellIconData as InitActionIconData;
        if (initData == null)
        {
            return;
        }
        name = GetType().Name + initData.actionIndex;
        _uIInitializable.SetButton(SelfButton, spellIconData.Screen.GetName(), ClickButton);
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

        _uIInitializable.SetText(KeyBind, "");

        KeyCommData keyCommData = _gs.ch.Get<KeyCommData>();

        KeyComm keyCode = keyCommData.GetKeyComm(KeyComm.ActionPrefix + ActionIndex);

        if (keyCode != null)
        {
            _uIInitializable.SetText(KeyBind, keyCode.ShowName());
        }

        string iconName = ItemConstants.BlankIconName;
        if (_spell != null)
        {
            iconName = _spell.Icon;
        }

        _assetService.LoadAtlasSpriteInto(AtlasNames.SkillIcons, iconName, Icon, _token);

        if (Tint != null && _spell == null)
        {
            Tint.FillAmount = 0;
        }

        _uIInitializable.SetText(Charges, "");
    }

    protected void ClickButton()
    {

    }

    public override void OnPointerDown(GPointerEventData eventData)
    {
        if (!InputConstants.OkActionIndex(ActionIndex))
        {
            return;
        }

        if (_inputService.ModifierIsActive(KeyComm.ShiftName))
        {
            base.OnPointerDown(eventData);
            return;
        }
    }

    public void SetCooldown(Character ch)
    {
        if (ch != _gs.ch || _spell == null)
        {
            return;
        }

        if (cooldownEnd > DateTime.UtcNow)
        {
            return;
        }

        cooldownStart = DateTime.UtcNow;
        if (_gs.ch.GlobalCooldownEnds > _spell.CooldownEnds)
        {
            cooldownEnd = _gs.ch.GlobalCooldownEnds;
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
        if (Tint == null || _spell == null)
        {
            return;
        }

        if (_spell.MaxCharges > 1)
        {
            if (_spell.CurrCharges != _lastCharges)
            {
                _lastCharges = _spell.CurrCharges;
                _uIInitializable.SetText(Charges, _lastCharges.ToString());

                if (_spell.CurrCharges < _spell.MaxCharges && _spell.CooldownEnds > DateTime.UtcNow)
                {
                    SetCooldown(_gs.ch);
                }
            }
        }


        if (cooldownEnd <= DateTime.UtcNow)
        {
            Tint.FillAmount = 0;
            _lastFillAmount = 0;
            return;
        }
        if (cooldownStart >= DateTime.UtcNow)
        {
            Tint.FillAmount = 1;
            _lastFillAmount = 1;
            return;
        }
        double totalSeconds = (cooldownEnd - cooldownStart).TotalSeconds;
        if (totalSeconds <= 0)
        {
            Tint.FillAmount = 0;
            _lastFillAmount = 0;
            return;
        }

        float oldFillAmount = Tint.fillAmount;
        float pctComplete = MathUtils.Clamp(0, (float)((DateTime.UtcNow - cooldownStart).TotalSeconds / totalSeconds), 1);
        Tint.FillAmount = 1 - pctComplete;
        _lastFillAmount = 1 - pctComplete;

    }
}