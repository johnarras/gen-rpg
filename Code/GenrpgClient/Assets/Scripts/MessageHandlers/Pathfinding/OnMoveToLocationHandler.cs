using Assets.Scripts.Pathfinding.Utils;
using Genrpg.Shared.Pathfinding.Entities;
using Genrpg.Shared.Pathfinding.Messages;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Pathfinding
{
    public class OnMoveToLocationHandler : BaseClientMapMessageHandler<OnMoveToLocation>
    {
        private IPathfindingService _pathfindingService;
        protected override void InnerProcess(UnityGameState gs, OnMoveToLocation msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.ObjId, out Unit unit))
            {
                return;
            }

            unit.Speed = msg.Speed;

            if (true)
            {
                _pathfindingService.UpdatePath(gs, unit, (int)(msg.FinalX), (int)msg.FinalZ);
            }
            //else
            //{
            //    unit.Waypoints = _pathfindingService.GetPath(gs, (int)msg.ObjX, (int)msg.ObjZ, (int)msg.FinalX, (int)msg.FinalZ);


            //    if (false && unit.TargetId == "1.1")
            //    {
            //        GameObject pl = PlayerObject.Get();
            //        gs.logger.Info("PFPlayer: " + unit.Id + " " + unit.X + " " + unit.Z + " Obj: " + msg.ObjX + " " + msg.ObjZ + " Final " + msg.FinalX + " " + msg.FinalZ +
            //            " RealPlayer: " + gs.ch.X + " " + gs.ch.Z + " PLGO: " + pl.transform.position.x + " " + pl.transform.position.z);


            //        ClientPathfindingUtils.ShowPath(gs, unit.Waypoints);
            //    }
            //}
        }
    }
}
