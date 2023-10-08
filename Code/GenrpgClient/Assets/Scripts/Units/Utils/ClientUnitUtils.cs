using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEntity = UnityEngine.GameObject;

public class ClientUnitUtils
{
    public static void UpdateMapPosition(UnitController controller, Unit unit)
    {
        if (controller != null && unit != null)
        {

            controller.transform().position = GVector3.Create(unit.X, 0, unit.Z);
            controller.transform().eulerAngles = GVector3.Create(0, unit.Rot, 0);
        }
    }
}
