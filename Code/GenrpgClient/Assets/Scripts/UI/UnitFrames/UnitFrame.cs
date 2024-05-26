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

    public GImage _currentTargetIcon;

    private IPlayerManager _playerManager;

    private List<OnAddEffect> _addedEffects = new List<OnAddEffect>();

    public void Init(UnityGameState gs, Unit unitIn)
    {
        _dispatcher.AddEvent<NewLevel>(this, OnLevelUpdate);
        _dispatcher.AddEvent<OnAddEffect>(this, AddVisualEffect);
        _dispatcher.AddEvent<OnRemoveEffect>(this, RemoveVisualEffect);
        _dispatcher.AddEvent<OnUpdateEffect>(this, UpdateVisualEffect);
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

        if (!_playerManager.TryGetUnit(out Unit unit))
        {
            return;
        }

        bool shouldShow = false;
        bool showStar = false;

        if (unit.TargetId == _unit.Id)
        {
            shouldShow = true;
            showStar = true;
        }

        if (_unit.TargetId == unit.Id)
        {
            shouldShow = true;
        }

        if (_unit.HasFlag(UnitFlags.IsDead))
        {
            shouldShow = false;
            showStar = false;
        }

        GEntityUtils.SetActive(_contentRoot, shouldShow);
        GEntityUtils.SetActive(_currentTargetIcon, showStar);
    }

    private void OnLevelUpdate(NewLevel newLevel)
    {
        if (_unit == null || _unit.Id != newLevel.UnitId)
        {
            return;
        }

        _unit.Level = newLevel.Level;
        _unitUI?.Init(_unit.Name, _unit.Level);
        return;
    }

    private void AddVisualEffect(OnAddEffect eff)
    {
        if (_unit == null || eff.TargetId != _unit.Id)
        {
            return;
        }

        return;
    }

    private void RemoveVisualEffect (OnRemoveEffect eff)
    {
        if (_unit == null || eff.TargetId != _unit.Id)
        {
            return;
        }

        return;

    }

    private void UpdateVisualEffect(OnUpdateEffect eff)
    {

        _logService.Debug("Update effect: " + eff.Id);

        return;
    }
}