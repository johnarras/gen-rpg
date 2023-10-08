using System;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using Assets.Scripts.Atlas.Constants;
using System.Threading;
using GPointerEventData = UnityEngine.EventSystems.PointerEventData;

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
        UIHelper.SetButton(SelfButton, spellIconData.Screen.GetAnalyticsName(), ClickButton);
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

        UIHelper.SetText(KeyBind, "");

        KeyCommData keyCommData = _gs.ch.Get<KeyCommData>();

        KeyComm keyCode = keyCommData.GetKeyComm(KeyComm.ActionPrefix + ActionIndex);

        if (keyCode != null)
        {
            UIHelper.SetText(KeyBind, keyCode.ShowName());
        }

        string iconName = ItemConstants.BlankIconName;
        if (_spell != null)
        {
            iconName = _spell.Icon;
        }

        _assetService.LoadSpriteInto(_gs, AtlasNames.SkillIcons, iconName, Icon, _token);

        if (Tint != null && _spell == null)
        {
            Tint.FillAmount = 0;
        }

        UIHelper.SetText(Charges, "");
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
        if (Tint == null || _spell == null)
        {
            return;
        }

        if (_spell.MaxCharges > 1)
        {
            if (_spell.CurrCharges != _lastCharges)
            {
                _lastCharges = _spell.CurrCharges;
                UIHelper.SetText(Charges, _lastCharges.ToString());

                if (_spell.CurrCharges < _spell.MaxCharges && _spell.CooldownEnds > DateTime.UtcNow)
                {
                    SetCooldown(_gs, _gs.ch);
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