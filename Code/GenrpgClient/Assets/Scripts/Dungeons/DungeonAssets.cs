using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Assets.Scripts.Dungeons
{
    public class DungeonAssets : BaseBehaviour
    {
        // Must be in same order as DungeonMaterials Material order
        public List<WeightedDungeonAsset> Walls = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Doors = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Floors = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Ceilings = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Pillars = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Fences = new List<WeightedDungeonAsset>();

        public List<WeightedDungeonMaterials> Materials = new List<WeightedDungeonMaterials>();

        public List<WeightedDungeonAsset> GetAssetList(int assetIndex)
        {
            if (assetIndex == DungeonAssetIndex.Walls)
            {
                return Walls;
            }
            else if (assetIndex == DungeonAssetIndex.Doors)
            {
                return Doors;
            }
            else if (assetIndex == DungeonAssetIndex.Floors)
            {
               return Floors;
            }
            else if (assetIndex == DungeonAssetIndex.Ceilings)
            {
                return Ceilings;            
            }
            else if (assetIndex == DungeonAssetIndex.Pillars)
            {
                return Pillars;
            }
            else if (assetIndex == DungeonAssetIndex.Fences)
            {
                return Fences;
            }
            return Walls;
        }


        public string BGImageName;
    }
}
