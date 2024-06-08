using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Corpses
{
    public class ClearLootHandler : BaseClientMapMessageHandler<ClearLoot>
    {
        protected override void InnerProcess(ClearLoot msg, CancellationToken token)
        {
            if (_objectManager.GetUnit(msg.UnitId,out Unit unit))
            {
                unit.Loot = null;
            }
        }
    }
}
