using System.Collections.Generic;
using UnityEngine;
using ClientEvents;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Threading;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.SpellCrafting.Messages;
using System.Linq;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Client.Assets.Constants;

internal class ActionButtonDownload
{
    public int Index;
    public GameObject Parent;
}

public class ActionBars : SpellIconScreen
{
    private IRepositoryService _repoService = null!;

    public const string ActionButtonPrefab = "ActionButton";

    
    public List<ActionButtonSet> _buttonSetParents;

    protected override bool LoadSpellIconsOnLoad() { return false; }
      
    protected Dictionary<int,ActionButton> _buttons { get; set; }

    public override string ToString()
    {
        return "ActionBars";
    }

    private void OnSetMapPlayer(SetMapPlayerEvent data)
    {
        Reset();
        return;
    }

    public void Init(CancellationToken token)
    {
        _token = token;
        AddListener<OnDeleteSpell>(OnDeleteSpellHandler);
        AddListener<OnStartCast>(OnStartCastHandler);
        AddListener<OnCraftSpell>(OnCraftSpellHandler);
        AddListener<SetMapPlayerEvent>(OnSetMapPlayer);
        AddListener<OnRemoveActionBarItem>(OnRemoveActionItem);
        AddListener<OnSetActionBarItem>(OnSetActionBarItemHandler);
        Reset();
    }

    protected void Reset()
    {
        if (_buttonSetParents == null)
        {
            return;
        }

        ScreenID = ScreenId.ActionBars;

        _buttons = new Dictionary<int, ActionButton>();

        for (int b = 0; b < _buttonSetParents.Count; b++)
        {
            GameObject parent = _buttonSetParents[b].gameObject;

            _clientEntityService.SetActive(parent, true); // Config user bars
            _clientEntityService.DestroyAllChildren(parent);

            for (int i = _buttonSetParents[b].MinIndex; i <= _buttonSetParents[b].MaxIndex; i++)
            {
                if (!InputConstants.OkActionIndex(i))
                {
                    continue;
                }

                ActionButtonDownload abDownload = new ActionButtonDownload()
                {
                    Index = i,
                    Parent = parent,
                };

                _assetService.LoadAssetInto(parent, AssetCategoryNames.UI, ActionButtonPrefab, OnDownloadButton, abDownload, _token, "ActionBars");
            }
        }
    }

    private void OnDownloadButton(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;

        if (go == null)
        {
            return;
        }

        ActionButtonDownload abDownload = data as ActionButtonDownload;

        if (abDownload == null)
        {
            return;
        }

        if (!InputConstants.OkActionIndex(abDownload.Index))
        {
            _clientEntityService.Destroy(go);
            return;
        }

        ActionButton button = go.GetComponent<ActionButton>();
        if (button == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        InitActionIconData initData = new InitActionIconData()
        {
            actionIndex = abDownload.Index,
            Screen = this,      
        };

        button.Init(initData, token);
        if (!_buttons.ContainsKey(abDownload.Index))
        {
            _buttons[abDownload.Index] = button;
        }

        List<ActionButton> buttons = _buttons.Values.OrderBy(x => x.ActionIndex).ToList();

        foreach (ActionButton ab in buttons)
        {
            ab.transform.SetParent(null);
            _clientEntityService.AddToParent(ab.gameObject, abDownload.Parent);
        }

    }

    private void OnStartCastHandler(OnStartCast castEvent)
    {

        if (_gs.ch == null ||
            castEvent.CasterId != _gs.ch.Id)
        {
            return;
        }


        foreach (ActionButton button in _buttons.Values)
        {
            button.SetCooldown(_gs.ch);
        }

        return;
    }

    private void OnDeleteSpellHandler (OnDeleteSpell data)
    {
        if (data == null)
        {
            return;
        }

        if (_buttons == null)
        {
            return;
        }

        foreach (ActionButton button in _buttons.Values)
        {
            if (button == null)
            {
                continue;
            }

            Spell buttonSpell = button.GetSpell();

            if (buttonSpell != null && buttonSpell.IdKey == data.SpellId)
            {
                InitActionIconData initData = new InitActionIconData()
                {
                  actionIndex = button.ActionIndex,
                  Screen=this,
                };
                button.Init(initData, _token);
            }
        }

        return;
    }

    private void OnCraftSpellHandler(OnCraftSpell data)
    {
        if (data == null)
        {
            return;
        }

        Spell spell = data.CraftedSpell;

        if (spell == null)
        {
            return;
        }

        if (_buttons == null)
        {
            return;
        }

        foreach (ActionButton button in _buttons.Values)
        {
            if (button == null)
            {
                continue;
            }

            Spell buttonSpell = button.GetSpell();

            if (buttonSpell != null && buttonSpell.IdKey == spell.IdKey)
            {
                InitActionIconData initData = new InitActionIconData()
                {
                    actionIndex = button.ActionIndex,
                    Screen = this,
                    Data = spell,
                };
                button.Init(initData, _token);
            }
        }

        return;
    }

    protected override void  OnUpdate()
    {
        if (_buttons != null)
        {
            foreach (int index in _buttons.Keys)
            {
                _buttons[index].UpdateCooldown();
            }
        }
        base.OnUpdate();
    }



    public override void OnRightClickIcon(SpellIcon icon)
    {
    }

    public override void OnLeftClickIcon(SpellIcon icon)
    {
        ActionButton actionButton = icon as ActionButton;
        if (actionButton == null)
        {
            return;
        }

        _inputService.PerformAction(actionButton.ActionIndex);
    }

    protected override void HandleDragDrop(SpellIconScreen screen, SpellIcon dragItem, SpellIcon otherIconHit, GameObject finalObjectHit)
    {
        if (dragItem == null || screen == null)
        {
            return;
        }

        ActionButton hitIcon = otherIconHit as ActionButton;

        ActionButton actionDrag = dragItem as ActionButton;
        ActionButton actionOrig = _origItem as ActionButton;
        SpellIcon spellDrag = dragItem as SpellIcon;

        if (hitIcon != null && InputConstants.OkActionIndex(hitIcon.ActionIndex))
        {

            long newSpellId = dragItem.GetSpellId();
            long currSpellId = hitIcon.GetSpellId();

            UpdateActionInput(hitIcon.ActionIndex, newSpellId);

            if (actionOrig != null && actionOrig.ActionIndex != hitIcon.ActionIndex)
            {
                if (actionDrag != null)
                {
                    UpdateActionInput(actionDrag.ActionIndex, currSpellId);
                }
            }
        }
        else // Noicon hit, unset this action input.
        {
            if (actionDrag != null)
            {
                UpdateActionInput(actionDrag.ActionIndex, 0);
            }
        }
            

        ResetCurrentDragItem();
    }

    protected void OnRemoveActionItem(OnRemoveActionBarItem msg)
    {
        UpdateActionInput(msg.Index, 0, false);
        return;
    }


    protected void OnSetActionBarItemHandler(OnSetActionBarItem msg)
    {
        UpdateActionInput(msg.Index, msg.SpellId, false);
        return;
    }

    protected void UpdateActionInput(int actionIndex, long spellTypeId, bool sendCommand = true)
    {
        if (_buttons == null || !_buttons.ContainsKey(actionIndex))
        {
            return;
        }

        ActionButton button = _buttons[actionIndex];

        ActionInputData actionInputs = _gs.ch.Get<ActionInputData>();
        actionInputs.SetInput(actionIndex, spellTypeId, _repoService);
        if (button == null || button.GetInitData() == null)
        {
            return;
        }


        Spell spell = _gs.ch.Get<SpellData>().Get(spellTypeId);

        button.SetDataItem(spell);
        button.Init(button.GetInitData(), _token);

        if (sendCommand)
        {
            _networkService.SendMapMessage(new SetActionBarItem() { Index = actionIndex, SpellId = spellTypeId });
        }
    }
}