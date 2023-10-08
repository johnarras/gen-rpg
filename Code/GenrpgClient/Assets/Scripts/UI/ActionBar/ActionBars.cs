using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using ClientEvents;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Spells.Entities;
using System.Threading;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.SpellCrafting.Messages;
using System.Linq;

internal class ActionButtonDownload
{
    public int Index;
    public GEntity Parent;
}

public class ActionBars : SpellIconScreen
{
    public const string ActionButtonPrefab = "ActionButton";

    
    public List<ActionButtonSet> _buttonSetParents;

    protected override bool LoadSpellIconsOnLoad() { return false; }
      
    protected Dictionary<int,ActionButton> _buttons { get; set; }

    public override string ToString()
    {
        return "ActionBars";
    }

    private SetMapPlayerEvent OnSetMapPlayer(UnityGameState gs, SetMapPlayerEvent data)
    {
        Reset();
        return null;
    }

    public void Init(CancellationToken token)
    {
        _token = token;
        _gs.AddEvent<OnDeleteSpell>(this, OnDeleteSpellHandler);
        _gs.AddEvent<OnStartCast>(this, OnStartCastHandler);
        _gs.AddEvent<OnCraftSpell>(this, OnCraftSpellHandler);
        _gs.AddEvent<SetMapPlayerEvent>(this, OnSetMapPlayer);
        _gs.AddEvent<OnRemoveActionBarItem>(this, OnRemoveActionItem);
        _gs.AddEvent<OnSetActionBarItem>(this, OnSetActionBarItemHandler);
        Reset();
    }

    protected void Reset()
    {
        if (_buttonSetParents == null)
        {
            return;
        }

        ScreenId = UI.Screens.Constants.ScreenId.ActionBars;

        _buttons = new Dictionary<int, ActionButton>();

        for (int b = 0; b < _buttonSetParents.Count; b++)
        {
            GEntity parent = _buttonSetParents[b].entity();

            GEntityUtils.SetActive(parent, true); // Config user bars
            GEntityUtils.DestroyAllChildren(parent);

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

                _assetService.LoadAssetInto(_gs, parent, AssetCategory.UI, ActionButtonPrefab, OnDownloadButton, abDownload, _token);
            }
        }
    }

    private void OnDownloadButton(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;

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
            GEntityUtils.Destroy(go);
            return;
        }

        ActionButton button = go.GetComponent<ActionButton>();
        if (button == null)
        {
            GEntityUtils.Destroy(go);
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
            ab.transform().SetParent(null);
            GEntityUtils.AddToParent(ab.entity(), abDownload.Parent);
        }

    }

    private OnStartCast OnStartCastHandler(UnityGameState gs, OnStartCast castEvent)
    {

        if (gs.ch == null ||
            castEvent.CasterId != gs.ch.Id)
        {
            return null;
        }


        foreach (ActionButton button in _buttons.Values)
        {
            button.SetCooldown(gs, gs.ch);
        }

        return null;
    }

    private OnDeleteSpell OnDeleteSpellHandler (UnityGameState gs, OnDeleteSpell data)
    {
        if (data == null)
        {
            return null;
        }

        if (_buttons == null)
        {
            return null;
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

        return null;
    }

    private OnCraftSpell OnCraftSpellHandler(UnityGameState gs, OnCraftSpell data)
    {
        if (data == null)
        {
            return null;
        }

        Spell spell = data.CraftedSpell;

        if (spell == null)
        {
            return null;
        }

        if (_buttons == null)
        {
            return null;
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

        return null;
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



    public override void OnRightClickIcon(UnityGameState gs, SpellIcon icon)
    {
    }

    public override void OnLeftClickIcon(UnityGameState gs, SpellIcon icon)
    {
        ActionButton actionButton = icon as ActionButton;
        if (actionButton == null)
        {
            return;
        }

        InputService.Instance.PerformAction(gs, actionButton.ActionIndex);
    }

    protected override void HandleDragDrop(SpellIconScreen screen, SpellIcon dragItem, SpellIcon otherIconHit, GEntity finalObjectHit)
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

    protected OnRemoveActionBarItem OnRemoveActionItem(UnityGameState gs, OnRemoveActionBarItem msg)
    {
        UpdateActionInput(msg.Index, 0, false);
        return null;
    }


    protected OnSetActionBarItem OnSetActionBarItemHandler(UnityGameState gs, OnSetActionBarItem msg)
    {
        UpdateActionInput(msg.Index, msg.SpellId, false);
        return null;
    }

    protected void UpdateActionInput(int actionIndex, long spellTypeId, bool sendCommand = true)
    {
        if (_buttons == null || !_buttons.ContainsKey(actionIndex))
        {
            return;
        }

        ActionButton button = _buttons[actionIndex];

        ActionInputData actionInputs = _gs.ch.Get<ActionInputData>();
        actionInputs.SetInput(actionIndex, spellTypeId);
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