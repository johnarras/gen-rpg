using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Reflection.Services;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.SpellCrafting.Messages;

public class SpellbookScreen : SpellIconScreen
{
    protected IReflectionService _reflectionService;
    protected ISharedSpellCraftService _spellCraftService;

    public const string SpellProcEditPrefab = "SpellProcEdit";

    public const string ArrowListSelectorPrefab = "ArrowListSelector";

    [SerializeField]
    private GameObject _dropdownParent;

    [SerializeField]
    private SpellIconPanel _spellPanel;

    [SerializeField]
    private ArrowListSelector _elementSelector;
    [SerializeField]
    private ArrowListSelector _skillSelector;

    [SerializeField]
    private GameObject _procParent;

    [SerializeField]
    private Text _infoText;
    [SerializeField]
    private Text _extraInfo;

    [SerializeField]
    private Button _closeButton;
    [SerializeField]
    private Button _craftButton;
    [SerializeField]
    private Button _deleteButton;
    [SerializeField]
    private Button _clearButton;

    public List<ArrowListSelector> _modSelectors { get; set; }
    public List<SpellProcEdit> _procEdits { get; set; }

    private Spell _selectedSpell = null;
    private Sprite[] _sprites = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _gs.AddEvent<OnCraftSpell>(this, OnCraftSpellHandler);
        _gs.AddEvent<OnDeleteSpell>(this, OnDeleteSpellHandler);
        UIHelper.SetButton(_closeButton, GetAnalyticsName(), StartClose);
        UIHelper.SetButton(_craftButton, GetAnalyticsName(), ClickCraft);
        UIHelper.SetButton(_deleteButton, GetAnalyticsName(), ClickDelete);
        UIHelper.SetButton(_clearButton, GetAnalyticsName(), ClickClear);
        SetupDropdowns();
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
        if (_spellPanel != null)
        {
            _spellPanel.Init(_gs, this, null, token);
        }
    }

    protected void SetupDropdowns()
    {
        if (_elementSelector != null)
        {
            _elementSelector.Init(_gs, _gs.data.GetGameData<SpellSettings>().ElementTypes, this);
        }

        if (_skillSelector != null)
        {
            _skillSelector.Init(_gs, _gs.data.GetGameData<SpellSettings>().SkillTypes, this);
        }

        if (_dropdownParent == null)
        {
            return;
        }

        GameObjectUtils.DestroyAllChildren(_dropdownParent);
        _modSelectors = new List<ArrowListSelector>();

        if (_gs.data.GetGameData<SpellSettings>().SpellModifiers != null)
        {
            foreach (SpellModifier mod in _gs.data.GetGameData<SpellSettings>().SpellModifiers)
            {
                if (mod.IsProcMod)
                {
                    continue;
                }

                _assetService.LoadAssetInto(_gs, _dropdownParent, AssetCategory.UI, ArrowListSelectorPrefab, OnDownloadSpellModPrefab, mod, _token);
            }
        }

        _procEdits = new List<SpellProcEdit>();

        for (int i = 0; i < SpellConstants.MaxProcsPerSpell; i++)
        {
            _assetService.LoadAssetInto(_gs, _procParent, AssetCategory.UI, SpellProcEditPrefab, OnDownloadSpellProc,i, _token);
        }
    }

    private void OnDownloadSpellModPrefab(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        SpellModifier mod = data as SpellModifier;

        if (mod == null)
        {
            GameObject.Destroy(go);
            return;
        }

        ArrowListSelector modSelector = go.GetComponent<ArrowListSelector>();
        if (modSelector == null)
        {
            GameObject.Destroy(go);
            return;
        }
        SpellModifierValue defaultValue = null;
        if (mod.Values != null)
        {
            foreach (SpellModifierValue val in mod.Values)
            {
                val.InitTempMod(mod);
                if (val.CostScale == SpellModifier.DefaultCostScale)
                {
                    defaultValue = val;
                }
            }
        }

        modSelector.Init(gs, mod.Values, this, defaultValue);
        _modSelectors.Add(modSelector);
    }

    private void OnDownloadSpellProc(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        SpellProcEdit procEdit = go.GetComponent<SpellProcEdit>();

        if (procEdit == null)
        {
            GameObject.Destroy(go);
            return;
        }

        procEdit.Init(gs, this, null, token);

        _procEdits.Add(procEdit);


    }

    public void ClickClear()
    {
        SetSelectedSpell(null);
    }

    public void ClickCraft()
    {
        if (_editSpell == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(_editSpell.Icon) && _sprites != null && _sprites.Length > 0)
        {
            _editSpell.Icon = _sprites[_gs.rand.Next() % _sprites.Length].name.Replace("(Clone)", "");
        }

        _networkService.SendMapMessage(new CraftSpell() { CraftedSpell = _editSpell });

    }


    protected OnCraftSpell OnCraftSpellHandler(UnityGameState gs, OnCraftSpell data)
    {
        _spellPanel.Init(gs, this, null, _token);
        return null;
    }


    protected OnDeleteSpell OnDeleteSpellHandler(UnityGameState gs, OnDeleteSpell data)
    {
        _spellPanel.Init(gs, this, null, _token);
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
        UpdateDropdownsFromSelectedSpell();
    }

    protected T GetSelectedItem<T>(Dropdown dd) where T : class
    {
        if (dd == null)
        {
            return default(T);
        }

        if (dd.options == null || dd.value < 0 || dd.value >= dd.options.Count)
        {
            return default(T);
        }

        MyDropdownOption option = dd.options[dd.value] as MyDropdownOption;
        if (option == null)
        {
            return default(T);
        }

        return option.OptionItem as T;

    }


    private Spell _editSpell = null;
    /// <summary>
    /// On select or set, update the spell based on what the dropdowns are.
    /// </summary>
    public void UpdateSpellFromDropdowns()
    {
        if (_gs.data.GetGameData<SpellSettings>().SpellModifiers == null)
        {
            return;
        }

        if (_editSpell == null)
        {
            _editSpell = new Spell();
        }

        _editSpell.ElementTypeId = 1;
        _editSpell.SkillTypeId = 1;

        if (_elementSelector == null || _skillSelector == null)
        {
            return;
        }

        ElementType selectedElement = _elementSelector.GetSelectedItem<ElementType>();
        SkillType selectedSkill = _skillSelector.GetSelectedItem<SkillType>();

        if (selectedElement != null)
        {
            _editSpell.ElementTypeId = selectedElement.IdKey;
        }

        if (selectedSkill != null)
        {
            _editSpell.SkillTypeId = selectedSkill.IdKey;
        }

        if (_modSelectors != null)
        {
            foreach (ArrowListSelector md in _modSelectors)
            {
                SpellModifierValue selectedMod = md.GetSelectedItem<SpellModifierValue>();

                if (selectedMod == null)
                {
                    continue;
                }


                SpellModifier realMod = selectedMod.GetTempMod();

                if (realMod == null)
                {
                    continue;
                }

                _reflectionService.SetObjectValue(_editSpell, realMod.DataMemberName, selectedMod.Value);
            }
        }

        if (_spellCraftService.GenerateSpellData(_gs, _editSpell))
        {
            ShowSpellData(_editSpell);
        }

    }


    private bool _suppressInfoChanged = false;
    /// <summary>
    /// On select or set, update the spell based on what the dropdowns are.
    /// </summary>
    public void UpdateDropdownsFromSelectedSpell()
    {
        if (_gs.data.GetGameData<SpellSettings>().SpellModifiers == null)
        {
            return;
        }

        _suppressInfoChanged = true;
        if (_selectedSpell != null)
        {
            _editSpell = SerializationUtils.FastMakeCopy(_selectedSpell) as Spell;
        }
        else
        {
            _editSpell = new Spell();
        }

        if (_elementSelector == null || _skillSelector == null)
        {
            return;
        }

        _elementSelector.SetToId(_editSpell.ElementTypeId);

        _skillSelector.SetToId(_editSpell.SkillTypeId);

        if (_modSelectors != null)
        {
            foreach (ArrowListSelector md in _modSelectors)
            {
                SpellModifierValue selectedMod = md.GetSelectedItem<SpellModifierValue>();

                if (selectedMod == null)
                {
                    continue;
                }

                SpellModifier realMod = selectedMod.GetTempMod();
                object idObj = _reflectionService.GetObjectValue(_editSpell, realMod.DataMemberName);

                if (idObj == null)
                {
                    continue;
                }

                long modVal = -1;

                if (Int64.TryParse(idObj.ToString(), out modVal))
                {
                    md.SetToId(modVal);
                }
            }
        }

        _suppressInfoChanged = false;
        ShowSpellData(_editSpell);
    }




    private void ShowSpellData(Spell spell)
    {
        if (spell == null)
        {
            return;
        }

    }

    public override void OnInfoChanged()
    {
        if (_suppressInfoChanged)
        {
            return;
        }

        UpdateSpellFromDropdowns();
    }
    protected override void ShowDragTargetIconsGlow(bool visible)
    {

    }
}

