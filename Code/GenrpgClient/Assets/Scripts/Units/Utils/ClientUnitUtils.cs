using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ClientUnitUtils
{
    public static void UpdateMapPosition(UnitController controller, Unit unit)
    {
        if (controller != null && unit != null)
        {

            controller.transform.position = new Vector3(unit.X, 0, unit.Z);
            controller.transform.eulerAngles = new Vector3(0, unit.Rot, 0);
        }
    }
}
