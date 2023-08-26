using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

public class KillCollider :BaseBehaviour
{

    DateTime lastCollideTime = DateTime.UtcNow;
    private void OnTriggerEnter(UnityEngine.Collider other)
    {     

        if (_gs.md == null || _gs.md.GeneratingMap)
        {
            return;
        }
        if ((DateTime.UtcNow - lastCollideTime).TotalSeconds < 1)
        {
            return;
        }

        MonsterController cont = GameObjectUtils.FindInParents<MonsterController>(other.gameObject);

        if (cont == null)
        {
            return;
        }
    }
}