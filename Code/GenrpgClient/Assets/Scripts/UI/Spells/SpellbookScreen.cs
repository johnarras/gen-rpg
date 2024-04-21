using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Assets.Scripts.Atlas.Constants;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.SpellCrafting.Messages;
using UnityEngine;
using Genrpg.Shared.SpellCrafting.Services;
using Assets.Scripts.UI.Spells;
using Genrpg.Shared.SpellCrafting.Constants;
using System.Collections.Generic;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;

public class SpellbookScreen : SpellIconScreen
{
    protected ISharedSpellCraftService _spellCraftService;
    protected IRepositoryService _repoService;

    protected string SpellEffectEditPrefabName = "SpellEffectEdit";

    public SpellIconPanel SpellPanel;
    public GButton DeleteButton;
    public GButton AddEffectButton;
    public GButton ValidateButton;
    public GButton ClearButton;
    public GButton CraftButton;

    public GInputField NameInput;
    public GText PowerCostText;
    public GDropdown ElementDropdown;
    public GDropdown PowerTypeDropdown;


    public SpellModInputField CastTimeInput;
    public SpellModInputField RangeInput;
    public SpellModInputField CooldownInput;
    public SpellModInputField ShotsInput;
    public SpellModInputField MaxChargesInput;

    public GEntity EffectListParent;

    private Spell _selectedSpell = null;
    private Sprite[] _sprites = null;

    private List<SpellEffectEdit> _effectEdits = new List<SpellEffectEdit>();

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _dispatcher.AddEvent<OnCraftSpell>(this, OnCraftSpellHandler);
        _dispatcher.AddEvent<OnDeleteSpell>(this, OnDeleteSpellHandler);
        _uIInitializable.SetButton(CraftButton, GetName(), ClickCraft);
        _uIInitializable.SetButton(DeleteButton, GetName(), ClickDelete);
        _uIInitializable.SetButton(ClearButton, GetName(), ClickClear);
        _uIInitializable.SetButton(AddEffectButton, GetName(), ClickAddEffect);
        _uIInitializable.SetButton(ValidateButton, GetName(), ClickValidate);
        InitScreenInputs();
        SetSelectedSpell(null);
        ShowSpells(token);

        if (LoadSpellIconsOnLoad())
        {
            _assetService.GetSpriteList(_gs, AtlasNames.SkillIcons, OnLoadSprites, token);
        }
        await UniTask.CompletedTask;
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
        ElementDropdown?.Init(_gameData.Get<ElementTypeSettings>(_gs.ch).GetData(), OnDropdownValueChanged);
        PowerTypeDropdown?.Init(_gameData.Get<StatSettings>(_gs.ch).GetPowerStats(), OnDropdownValueChanged);
        
        ShotsInput?.Init(SpellModifiers.Shots, OnDropdownValueChanged);
        RangeInput?.Init(SpellModifiers.Range, OnDropdownValueChanged);
        CooldownInput?.Init(SpellModifiers.Cooldown, OnDropdownValueChanged);
        MaxChargesInput?.Init(SpellModifiers.ExtraTargets, OnDropdownValueChanged);
        CastTimeInput?.Init(SpellModifiers.CastTime, OnDropdownValueChanged);
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

        _assetService.LoadAssetInto(_gs, EffectListParent, AssetCategoryNames.UI, 
            SpellEffectEditPrefabName, OnLoadEffect, effect, _token, Subdirectory);
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

        IReadOnlyList<ElementType> elements = _gameData.Get<ElementTypeSettings>(_gs.ch).GetData();
        IReadOnlyList<StatType> statTypes = _gameData.Get<StatSettings>(_gs.ch).GetData();

        _editSpell.ElementTypeId = _uIInitializable.GetSelectedIdFromName(typeof(ElementType), ElementDropdown);
        _editSpell.PowerStatTypeId = _uIInitializable.GetSelectedIdFromName(typeof(StatType), PowerTypeDropdown);

        _editSpell.Cooldown = (int)CooldownInput?.GetSelectedValue();
        _editSpell.MaxRange = (int)RangeInput?.GetSelectedValue();
        _editSpell.Shots = (int)ShotsInput?.GetSelectedValue();
        _editSpell.MaxCharges = (int)ShotsInput?.GetSelectedValue();
        _editSpell.CastTime = (float)CastTimeInput?.GetSelectedValue();

        foreach (SpellEffectEdit edit in _effectEdits)
        {
            edit.CopyFromUIToEffect();
        }

        if (_spellCraftService.ValidateSpellData(_gs, _gs.ch, _editSpell))
        {
            CopyFromSpellToUI(_editSpell);
            return true;
        }
        _logService.Error("Spell could not be validated!");
        return false;
    }

    private void OnDropdownValueChanged()
    {
        CopyFromUIToSpell();
    }


    private void CopyFromSpellToUI(Spell spell)
    {
        if (spell == null)
        {
            return;
        }

        ElementDropdown?.SetFromId(spell.ElementTypeId);
        PowerTypeDropdown?.SetFromId(spell.PowerStatTypeId);

        CooldownInput?.SetSelectedValue(spell.Cooldown);
        ShotsInput?.SetSelectedValue(spell.Shots);
        CastTimeInput?.SetSelectedValue(spell.CastTime);
        RangeInput?.SetSelectedValue(spell.MaxRange);
        MaxChargesInput?.SetSelectedValue(spell.MaxCharges);

        _uIInitializable.SetText(PowerCostText, spell.PowerCost.ToString());

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
                _effectEdits[e].Init(spell.Effects[e], spell, this, OnDropdownValueChanged);
            }
        }

        // Add new effect edit blocks for things as needed
        for (int e = _effectEdits.Count; e < spell.Effects.Count; e++)
        {
            _assetService.LoadAssetInto(_gs, EffectListParent, AssetCategoryNames.UI,
                SpellEffectEditPrefabName, OnLoadEffect, spell.Effects[e], _token, Subdirectory);

        }
    }


    private void OnLoadEffect(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;

        if (obj == null)
        {
            _logService.Error("Failed to load SpellEffectEdit");
            return;
        }

        SpellEffectEdit edit = go.GetComponent<SpellEffectEdit>();

        if (edit == null)
        {
            _logService.Error("SpellEffectEdit Component missing");
            return;
        }

        edit.Init(data as SpellEffect, _editSpell, this, OnDropdownValueChanged);

        _effectEdits.Add(edit);


    }

    protected override void ShowDragTargetIconsGlow(bool visible)
    {

    }
}

