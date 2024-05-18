using Assets.Scripts.Pathfinding.Utils;
using Assets.Scripts.Tokens;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Pathfinding.Entities;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GEntity = UnityEngine.GameObject;


public interface IPlayerManager : IInitializable, IMapTokenService
{
    void SetUnit(UnitController unitController);
    bool TryGetUnit(out Unit unit);
    GEntity GetEntity();
    string GetUnitId();
    bool Exists();
    void SetCurrentTarget(string targetId);
    void TargetNext(); 
    void SetKeyPercent(string commandName, float percent);
    void MoveAboveObstacles();
}

public class PlayerManager : IPlayerManager
{
    private Unit _unit;
    private UnitController _unitController;
    private GameObject _entity;

    private CancellationToken _mapToken;

    private IClientPathfindingUtils _clientPathfindingUtils;
    private IClientMapObjectManager _mapObjectManager;
    private IPathfindingService _pathfindingService;
    private IRealtimeNetworkService _networkService;

    public UnityGameState _gs { get; set; }

    public async Task Initialize(GameState gs, CancellationToken token)
    {
        _gs = gs as UnityGameState;
        await Task.CompletedTask;
    }

    public void SetUnit(UnitController unitController)
    {
        _unitController = unitController;
        if (_unitController != null)
        {
            _entity = _unitController.gameObject;
            _unit = _unitController.GetUnit();
            ClientUnitUtils.UpdateMapPosition(_unitController, _unit);
        }
        else
        {
            _entity = null;
            _unit = null;
        }
    }

    public void SetMapToken(CancellationToken mapToken)
    {
        _mapToken = mapToken;
    }

    public void SetKeyPercent(string keyName, float percent)
    {
        _unitController?.SetKeyPercent(keyName, percent);
    }

  
    public bool TryGetUnit(out Unit unit)
    {
        unit = _unit;
        return unit != null;
    }

    public string GetUnitId()
    {
        return _unit?.Id ?? null;
    }

    public GEntity GetEntity()
    {
        return _entity;
    }

    public UnitController GetController()
    {
        return _unitController;
    }

    public bool Exists()
    {
        return _unit != null && _unitController != null;
    }

    public void SetCurrentTarget(string unitId)
    {
        if (_unit == null || _unitController == null)
        {
            return;
        }

        _mapObjectManager.GetUnit(unitId, out Unit finalUnit);

        if (finalUnit != null)
        {
            _lastUnitsTabbed.Add(finalUnit);
            _unit.TargetId = finalUnit.Id;
            int sx = (int)(_unitController.entity.transform.position.x);
            int sz = (int)(_unitController.entity.transform.position.z);
            int ex = (int)(finalUnit.X);
            int ez = (int)(finalUnit.Z);
            WaypointList list = _pathfindingService.GetPath(_gs, sx, sz, ex, ez);

            _clientPathfindingUtils.ShowPath(_gs, list, _mapToken).Forget();
        }
        else
        {
            _unit.TargetId = null;
        }

        _networkService.SendMapMessage(new SetTarget() { TargetId = finalUnit.Id });
    }

    private List<Unit> _lastUnitsTabbed = new List<Unit>();
    public void TargetNext()
    {
        int dist = 20;
        int rad = dist + 10;

        while (_lastUnitsTabbed.Count > 2)
        {
            _lastUnitsTabbed.RemoveAt(0);
        }

        if (_unit != null)
        {
            GVector3 newPos = GVector3.Create(_unitController.entity.transform().position + _unitController.entity.transform().forward * dist);
            List<Unit> units = _mapObjectManager.GetTypedObjectsNear<Unit>(newPos.x, newPos.z, rad);
            if (units.Count < 1)
            {
                return;
            }

            _lastUnitsTabbed = _lastUnitsTabbed.Where(X => X != null && X != _unit).ToList();

            float minDistToPlayer = 1000000;
            float cx = newPos.x;
            float cz = newPos.z;
            foreach (Unit obj in units)
            {
                if (obj == _unit)
                {
                    continue;
                }
                float dx = obj.X - cx;
                float dz = obj.Z - cz;
                float currDist = (float)Math.Sqrt(dx * dx + dz * dz);
                if (currDist < minDistToPlayer)
                {
                    minDistToPlayer = currDist;
                }
            }


           List<Unit> closeUnits = new List<Unit>();
            foreach (Unit obj in units)
            {

                if (obj == _unit)
                {
                    continue;
                }
                float dx = obj.X - cx;
                float dz = obj.Z - cz;
                float currDist = (float)Math.Sqrt(dx * dx + dz * dz);

                if (currDist < minDistToPlayer + 10)
                {
                    closeUnits.Add(obj);
                }
            }

            List<Unit> nontabbedUnits = new List<Unit>();
            foreach (Unit obj in closeUnits)
            {
                if (!_lastUnitsTabbed.Contains(obj))
                {
                    nontabbedUnits.Add(obj);
                }
            }


            List<Unit> finalUnits = (closeUnits.Count > 0 ? closeUnits : nontabbedUnits);


            if (nontabbedUnits.Count < 1)
            {
                _lastUnitsTabbed.Clear();
            }

            if (finalUnits.Count < 1)
            {
                return;
            }

            finalUnits = finalUnits.Where(x => !x.HasFlag(UnitFlags.IsDead)).ToList();


            if (finalUnits.Count < 1)
            {
                return;
            }

            int unitPos = _gs.rand.Next() % finalUnits.Count;

            Unit finalUnit = finalUnits[unitPos];

            if (finalUnit == _unit)
            {
                return;
            }
            SetCurrentTarget(finalUnit.Id);
        }
    }
    public void MoveAboveObstacles()
    {
        if (_entity == null)
        {
            return;
        }
        List<GEntity> hits = GPhysics.RaycastAll(GVector3.Create(_entity.transform().position) + GVector3.up * 500, GVector3.down);

        foreach (GEntity hit in hits)
        {
            if (hit.name.IndexOf("Water") >= 0)
            {
                if (hit.transform().position.y > _entity.transform().position.y)
                {
                    _entity.transform().position = GVector3.Create(_entity.transform().position.x, hit.transform().position.y + 1,
                        _entity.transform().position.z);
                }
            }
        }
        _entity.transform().position += GVector3.Create(0, 10, 0);
    }
}