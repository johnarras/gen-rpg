using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Buildings
{ 
    public  class CrawlerBuilding : MapBuilding
    {
        public List<MeshRenderer> Walls = new List<MeshRenderer>();
        public List<MeshRenderer> Doors = new List<MeshRenderer>();
        public List<MeshRenderer> Windows = new List<MeshRenderer>();
        public List<MeshRenderer> Shingles = new List<MeshRenderer>();
        public List<MeshRenderer> RoofPeaks = new List<MeshRenderer>();

        public void InitData(BuildingType btype, long seed, BuildingMats mats)
        {
            base.Init(btype, new OnSpawn());
            MyRandom rand = new MyRandom(seed);


            SetMaterialToSlot(Walls, mats.GetMatsFromSlot(EBuildingMatSlots.Walls), rand);
            SetMaterialToSlot(RoofPeaks, mats.GetMatsFromSlot(EBuildingMatSlots.Walls), rand);
            SetMaterialToSlot(Doors, mats.GetMatsFromSlot(EBuildingMatSlots.Doors), rand);
            SetMaterialToSlot(Windows, mats.GetMatsFromSlot(EBuildingMatSlots.Windows), rand);
            SetMaterialToSlot(Shingles, mats.GetMatsFromSlot(EBuildingMatSlots.Shingles), rand);

        }

        public void SetMaterialToSlot(List<MeshRenderer> meshes, List<WeightedBuildingMaterial> mats, IRandom rand)
        {
            if (mats.Count < 1)
            {
                return;
            }

            double weightSum = mats.Sum(x=>x.Weight);
            double weightChosen = rand.NextDouble() * weightSum;    

            foreach (WeightedBuildingMaterial mat in mats)
            {
                weightChosen -= mat.Weight;

                if (weightChosen <= 0)
                {
                    foreach (MeshRenderer renderer in meshes)
                    {
                        renderer.material = mat.Mat;
                    }

                    if (mat.ColorTargets.Count > 0)
                    {
                        Color colorTarget = mat.ColorTargets[rand.Next() % mat.ColorTargets.Count];

                        float targetPercent = (float)rand.NextDouble();
                        Color newColor = new Color((float)(colorTarget.r + (1 - colorTarget.r) * targetPercent),
                            (float)(colorTarget.g + (1 - colorTarget.g) * targetPercent),
                            (float)(colorTarget.b + (1 - colorTarget.b) * targetPercent), 1);

                        foreach (MeshRenderer renderer in meshes)
                        {
                            renderer.material.color = newColor;
                        }
                    }
                    break;
                }
            }
        }

    }
}
