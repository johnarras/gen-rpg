using Cysharp.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnStartCastHandler : BaseClientMapMessageHandler<OnStartCast>
    {
        protected override void InnerProcess(UnityGameState gs, OnStartCast msg, CancellationToken token)
        {
            if (_objectManager.GetGridItem(msg.CasterId, out ClientMapObjectGridItem gridItem))
            {
                gridItem.Controller?.StartCasting(msg);
            }
        }
    }
}
