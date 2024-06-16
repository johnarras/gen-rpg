using Assets.Scripts.Pathfinding.Utils;
using Assets.Scripts.ProcGen.RandomNumbers;

using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Pathfinding.Entities;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.ResultHandlers.TypedHandlers
{
    public class OnUpdatePosHandler : BaseClientMapMessageHandler<OnUpdatePos>
    {

        private IPathfindingService _pathfindingService;
        private IClientPathfindingUtils _pathfindingUtils;
        private IPlayerManager _playerManager;

        protected override void InnerProcess(OnUpdatePos pos, CancellationToken token)
        {
            if (pos.ObjId == _playerManager.GetUnitId())
            {
                return;

            }

            if (_objectManager.GetGridItem(pos.ObjId, out ClientMapObjectGridItem gridItem))
            {
                if (gridItem.Controller != null)
                {
                    gridItem.Controller.LastPosUpdate = DateTime.UtcNow;
                }
            }

            if (_objectManager.GetObject(pos.ObjId, out MapObject obj))
            {
                float oldFX = obj.FinalX;
                float oldFZ = obj.FinalZ;
                string oldTarget = obj.TargetId;
                float oldSpeed = obj.Speed;

                obj.FinalX = pos.GetX();
                obj.FinalZ = pos.GetZ();
                obj.Speed = pos.GetSpeed();
                obj.Moving = true;
                obj.TargetId = pos.TargetId;

                if (obj is Unit unit)
                {

                    if (oldFX != obj.FinalX || oldFZ != obj.FinalZ || oldSpeed != obj.Speed ||
                        oldTarget != obj.TargetId)
                    {
                        _pathfindingService.UpdatePath(_rand, unit, (int)obj.FinalX, (int)obj.FinalZ, OnUpdatePath);
                    }

                    if (unit.HasFlag(UnitFlags.ProxyCharacter))
                    {
                        if (_objectManager.GetController(pos.ObjId, out UnitController unitController))
                        {
                            unitController.SetInputValues(pos.GetKeysDown(), pos.GetRot());
                        }
                    }
                }
            }
        }     
        
        private void OnUpdatePath(IRandom rand, Unit unit, WaypointList list)
        {
            unit.Waypoints = list;
            _pathfindingUtils.ShowPath(unit.Waypoints, _pathfindingUtils.GetToken());
        }
    }
}
