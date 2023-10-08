using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.ResultHandlers.TypedHandlers
{
    public class OnUpdatePosHandler : BaseClientMapMessageHandler<OnUpdatePos>
    { 
        protected override void InnerProcess(UnityGameState gs, OnUpdatePos pos, CancellationToken token)
        {
            if (pos.ObjId == PlayerObject.GetUnit()?.Id)
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
                    obj.ToX = pos.GetX();
                    obj.ToZ = pos.GetZ();
                    obj.Speed = pos.GetSpeed();
                    obj.Moving = true;
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
