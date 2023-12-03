using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Reflection.Services;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.SpellCrafting.Messages;
using UnityEngine;
using Genrpg.Shared.SpellCrafting.Services;
using System.Security.Policy;
using Assets.Scripts.UI.Spells;
using Genrpg.Shared.SpellCrafting.Constants;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.Entities;

public class SpellbookScreen : SpellIconScreen
{
    protected IReflectionService _reflectionService;
    protected ISharedSpellCraftService _spellCraftService;

    protected string SpellEffectEditPrefabName = "SpellEffectEdit";

    public SpellIconPanel SpellPanel;
    public GButton CloseButton;
    public GButton DeleteButton;
    public GButton AddEffectButton;
    public GButton ValidateButton;
    public GButton ClearButton;
    public GButton CraftButton;

    public GInputField NameInput;
    public GInputField ElementInput;
    public GInputField PowerTypeInput;
    public GText PowerCostText;


    public SpellModInputField CastTimeInput;
    public SpellModInputField RangeInput;
    public SpellModInputField CooldownInput;
    public SpellModInputField ShotsInput;
    public SpellModInputField MaxChargesInput;

    public GEntity EffectListParent;

    private Spell _selectedSpell = null;
    private Sprite[] _sprites = null;

    private List<SpellEffectEdit> _effectEdits = new List<SpellEffectEdit>();

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _gs.AddEvent<OnCraftSpell>(this, OnCraftSpellHandler);
        _gs.AddEvent<OnDeleteSpell>(this, OnDeleteSpellHandler);
        UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);
        UIHelper.SetButton(CraftButton, GetAnalyticsName(), ClickCraft);
        UIHelper.SetButton(DeleteButton, GetAnalyticsName(), ClickDelete);
        UIHelper.SetButton(ClearButton, GetAnalyticsName(), ClickClear);
        UIHelper.SetButton(AddEffectButton, GetAnalyticsName(), ClickAddEffect);
        UIHelper.SetButton(ValidateButton, GetAnalyticsName(), ClickValidate);
        InitScreenInputs();
        SetSelectedSpell(null);
        ShowSpells(token);

        if (LoadSpellIconsOnLoad())
        {
            _assetService.GetSpriteList(_gs, AtlasNames.SkillIcons, OnLoadSprites, token);
        }
        await Task.CompletedTask;
    }

    private void OnLoadSprites(UnityGameState gs, Sprite[] sprites)
    {
        _sprites = sprites;
    }

    public override void OnRightClickIcon(UnityGameState gs, SpellIcon icon)
    {
        SetSelectedSpell(icon.GetSpell());
    }

    public override void OnLeftClickIcon(UnityGameState gs, SpellIcon icon)
    {
        SetSelectedSpell(icon.GetSpell());
    }

    protected void ShowSpells(CancellationToken token)
    {
        if (SpellPanel != null)
        {
            SpellPanel.Init(_gs, this, null, token);
        }
    }

    public void InitScreenInputs()
    {
        ShotsInput?.Init(SpellModifiers.Shots);
        RangeInput?.Init(SpellModifiers.Range);
        CooldownInput?.Init(SpellModifiers.Cooldown);
        MaxChargesInput?.Init(SpellModifiers.ExtraTargets);
        CastTimeInput?.Init(SpellModifiers.CastTime);
    }

    public void ClickClear()
    {
        SetSelectedSpell(null);
    }

    public void ClickValidate()
    {
        CopyFromUIToSpell();
    }

    public void ClickCraft()
    {
        if (_editSpell == null)
        {
            return;
        }

        if (!CopyFromUIToSpell())
        {
            return;
        }

        if (string.IsNullOrEmpty(_editSpell.Icon) && _sprites != null && _sprites.Length > 0)
        {
            _editSpell.Icon = _sprites[_gs.rand.Next() % _sprites.Length].name.Replace("(Clone)", "");
        }

        _networkService.SendMapMessage(new CraftSpell() { CraftedSpell = _editSpell });

    }

    public void ClickAddEffect()
    {
        if (_editSpell == null)
        {
            return;
        }

        SpellEffect effect = new SpellEffect();
        _editSpell.Effects.Add(effect);

        _assetService.LoadAssetInto(_gs, EffectListParent, AssetCategoryNames.UI, SpellEffectEditPrefabName, OnLoadEffect, effect, _token);
    }


    protected OnCraftSpell OnCraftSpellHandler(UnityGameState gs, OnCraftSpell data)
    {
        SpellPanel.Init(gs, this, null, _token);
        return null;
    }


    protected OnDeleteSpell OnDeleteSpellHandler(UnityGameState gs, OnDeleteSpell data)
    {
        SpellPanel.Init(gs, this, null, _token);
        return null;
    }

    public void ClickDelete()
    {
        if (_selectedSpell == null)
        {
            return;
        }
        _networkService.SendMapMessage(new DeleteSpell() { SpellId = _selectedSpell.IdKey });
    }

    protected void SetSelectedSpell(Spell spell)
    {
        _selectedSpell = spell;
        _editSpell = spell;
        CopyFromSpellToUI(spell);
    }

    private Spell _editSpell = null;
    /// <summary>
    /// On select or set, update the spell based on what the dropdowns are.
    /// </summary>
    public bool CopyFromUIToSpell()
    {
        if (_editSpell == null)
        {
            _editSpell = new Spell();
        }

        _editSpell.Name = NameInput?.Text;
        _editSpell.ElementTypeId = UIHelper.GetIntInput(ElementInput);
        _editSpell.PowerStatTypeId = UIHelper.GetIntInput(PowerTypeInput);

        _editSpell.Cooldown = UIHelper.GetIntInput(CooldownInput?.InputField);
        _editSpell.Range = UIHelper.GetIntInput(RangeInput?.InputField);
        _editSpell.Shots = UIHelper.GetIntInput(ShotsInput?.InputField);
        _editSpell.MaxCharges = UIHelper.GetIntInput(ShotsInput?.InputField);
        _editSpell.CastTime = UIHelper.GetFloatInput(CastTimeInput?.InputField);

        foreach (SpellEffectEdit edit in _effectEdits)
        {
            edit.CopyFromUIToEffect();
        }

        if (_spellCraftService.ValidateSpellData(_gs, _gs.ch, _editSpell))
        {
            CopyFromSpellToUI(_editSpell);
            return true;
        }
        _gs.logger.Error("Spell could not be validated!");
        return false;
    }


    private void CopyFromSpellToUI(Spell spell)
    {
        if (spell == null)
        {
            return;
        }

        UIHelper.SetInputText(NameInput, spell.Name);
        UIHelper.SetInputText(ElementInput, spell.ElementTypeId);
        UIHelper.SetInputText(PowerTypeInput, spell.PowerStatTypeId);
        UIHelper.SetInputText(CooldownInput.InputField, spell.Cooldown);
        UIHelper.SetInputText(ShotsInput?.InputField, spell.Shots);
        UIHelper.SetInputText(CastTimeInput?.InputField, spell.CastTime);
        UIHelper.SetInputText(RangeInput?.InputField, spell.Range);
        UIHelper.SetInputText(MaxChargesInput?.InputField, spell.MaxCharges);

        UIHelper.SetText(PowerCostText, spell.PowerCost.ToString());

        // Get rid of extra effect blocks
        while (_effectEdits.Count > spell.Effects.Count)
        {
            GEntityUtils.Destroy(_effectEdits[_effectEdits.Count - 1].gameObject);
            _effectEdits.RemoveAt(_effectEdits.Count - 1);
        }

        // Re-initialize any existing effect blocks
        for (int e = 0; e < spell.Effects.Count; e++)
        {
            if (e < _effectEdits.Count)
            {
                _effectEdits[e].Init(spell.Effects[e], spell, this);
            }
        }

        // Add new effect edit blocks for things as needed
        for (int e = _effectEdits.Count; e < spell.Effects.Count; e++)
        {
            _assetService.LoadAssetInto(_gs, EffectListParent, AssetCategoryNames.UI, SpellEffectEditPrefabName, OnLoadEffect, spell.Effects[e], _token);

        }
    }


    private void OnLoadEffect(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;

        if (obj == null)
        {
            _gs.logger.Error("Failed to load SpellEffectEdit");
            return;
        }

        SpellEffectEdit edit = go.GetComponent<SpellEffectEdit>();

        if (edit == null)
        {
            _gs.logger.Error("SpellEffectEdit Component missing");
            return;
        }

        edit.Init(data as SpellEffect, _editSpell, this);

        _effectEdits.Add(edit);


    }

    protected override void ShowDragTargetIconsGlow(bool visible)
    {

    }
}

