using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Genrpg.Shared.Core.Entities;


using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

public class KillCollider :BaseBehaviour
{

    protected IMapGenData _md;
    DateTime lastCollideTime = DateTime.UtcNow;
    private void OnTriggerEnter(UnityEngine.Collider other)
    {     

        if (_md.GeneratingMap)
        {
            return;
        }
        if ((DateTime.UtcNow - lastCollideTime).TotalSeconds < 1)
        {
            return;
        }

        MonsterController cont = GEntityUtils.FindInParents<MonsterController>(other.entity());

        if (cont == null)
        {
            return;
        }
    }
}