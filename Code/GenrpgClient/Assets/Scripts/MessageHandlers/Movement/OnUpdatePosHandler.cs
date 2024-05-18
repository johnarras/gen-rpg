using Assets.Scripts.Pathfinding.Utils;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
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

        protected override void InnerProcess(UnityGameState gs, OnUpdatePos pos, CancellationToken token)
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
                if (!(obj is Character ch))
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
                    
                    if (oldFX != obj.FinalX || oldFZ != obj.FinalZ || oldSpeed != obj.Speed ||
                        oldTarget != obj.TargetId)
                    {
                        _pathfindingService.UpdatePath(gs, obj, (int)obj.FinalX, (int)obj.FinalZ);
                        _pathfindingUtils.ShowPath(gs, obj.Waypoints, token).Forget();
                    }

                    if (obj is Unit unit && unit.HasFlag(UnitFlags.ProxyCharacter))
                    {
                        if (_objectManager.GetController(pos.ObjId, out UnitController unitController))
                        {
                            unitController.SetInputValues(pos.GetKeysDown(), pos.GetRot());
                        }
                    }
                }
            }
        }
    }
}
