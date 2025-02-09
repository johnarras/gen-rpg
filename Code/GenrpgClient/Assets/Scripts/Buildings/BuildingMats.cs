using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Buildings
{
    public class BuildingMats : BaseBehaviour
    {
        public List<WeightedBuildingMaterial> WallMats = new List<WeightedBuildingMaterial>();
        public List<WeightedBuildingMaterial> DoorMats = new List<WeightedBuildingMaterial>();
        public List<WeightedBuildingMaterial> WindowMats = new List<WeightedBuildingMaterial>();
        public List<WeightedBuildingMaterial> ShinglesMats = new List<WeightedBuildingMaterial>();


        public List<WeightedBuildingMaterial> GetMatsFromSlot(EBuildingMatSlots slot)
        {
            if (slot == EBuildingMatSlots.Walls)
            {
                return WallMats;
            }
            else if (slot == EBuildingMatSlots.Doors)
            {
                return DoorMats;
            }
            else if (slot == EBuildingMatSlots.Windows)
            {
                return WindowMats;
            }
            else if (slot == EBuildingMatSlots.Shingles)
            {
                return ShinglesMats;
            }
            return WallMats;
        }

    }
}
