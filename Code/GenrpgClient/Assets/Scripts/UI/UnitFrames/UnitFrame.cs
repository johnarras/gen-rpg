using System.Collections.Generic;
using Genrpg.Shared.Levels.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using GEntity = UnityEngine.GameObject;

public class UnitFrame : BaseBehaviour
{
    
    public CoreUnitUI _unitUI;

    
    public List<UnitStatBar> _statBar;

    
    public CastBar _castBar;

    
    public GEntity _contentRoot;

    
    public ExpBar _expBar;

    
    public GEntity _effectParent;

    private Unit _unit;
    protected UnitController _controller;

    private List<OnAddEffect> _addedEffects = new List<OnAddEffect>();

    public void Init(UnityGameState gs, Unit unitIn)
    {
        _gs.AddEvent<NewLevel>(this, OnLevelUpdate);
        _gs.AddEvent<OnAddEffect>(this, AddVisualEffect);
        _gs.AddEvent<OnRemoveEffect>(this, RemoveVisualEffect);
        _gs.AddEvent<OnUpdateEffect>(this, UpdateVisualEffect);
        _unit = unitIn;
        _controller = GEntityUtils.FindInParents<UnitController>(entity);
        if (_controller != null)
        {
            _controller.SetUnitFrame(this);
        }
        for (int b = 0; b < _statBar.Count; b++)
        {
            if (_statBar[b] != null)
            {
                _statBar[b].Init(gs, _unit);
            }
        }

        if (_unit != null)
        {
            _unitUI?.Init(_unit.Name, _unit.Level);

            if (_castBar != null)
            {
                _castBar.Init(gs, _unit);
            }

            if (_expBar != null)
            {
                _expBar.Init(gs, _unit);
            }
        }
        UpdateVisibility();
    }
    
    public void UpdateVisibility()
    {
        if (_unit == null || _controller == null)
        {
            return;
        }
        bool shouldShow = (_controller.UnitState == UnitController.CombatState || _controller.AlwaysShowHealthBar());

        if (PlayerObject.GetUnit()?.TargetId == _unit.Id)
        {
            shouldShow = true;
        }
        if (_unit.HasFlag(UnitFlags.IsDead))
        {
            shouldShow = false;
        }

        GEntityUtils.SetActive(_contentRoot, shouldShow);
    }

    private NewLevel OnLevelUpdate(UnityGameState gs, NewLevel newLevel)
    {
        if (_unit == null || _unit.Id != newLevel.UnitId)
        {
            return null;
        }

        _unit.Level = newLevel.Level;
        _unitUI?.Init(_unit.Name, _unit.Level);
        return null;
    }

    private OnAddEffect AddVisualEffect (UnityGameState gs, OnAddEffect eff)
    {
        if (_unit == null || eff.TargetId != _unit.Id)
        {
            return null;
        }

        return null;
    }

    private OnRemoveEffect RemoveVisualEffect (UnityGameState gs, OnRemoveEffect eff)
    {
        if (_unit == null || eff.TargetId != _unit.Id)
        {
            return null;
        }

        return null;

    }

    private OnUpdateEffect UpdateVisualEffect(UnityGameState gs, OnUpdateEffect eff)
    {

        _logService.Debug("Update effect: " + eff.Id);

        return null;
    }
}