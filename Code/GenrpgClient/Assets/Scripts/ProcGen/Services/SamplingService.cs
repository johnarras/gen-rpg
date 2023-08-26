using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
namespace Services.ProcGen
{
    public interface ISamplingService : IService
    {
        List<MyPoint2> PlanePoissonSample(GameState gs, SamplingData sd);
    }
    public class SamplingService : ISamplingService
    {

        public List<MyPoint2> PlanePoissonSample(GameState gs, SamplingData sd)
        {
            List<MyPoint2> list = new List<MyPoint2>();
            if (sd == null)
            {
                return list;
            }

            if (sd.XMin >= sd.XMax || sd.YMin >= sd.YMax)
            {
                return list;
            }


            if (sd.Count < 1 || sd.MaxAttemptsPerItem < 1)
            {
                return list;
            }

            MyRandom rand = null;
            if (sd.Seed > 0)
            {
                rand = new MyRandom(sd.Seed);
            }
            else
            {
                rand = new MyRandom();
            }


            int maxNumTimes = sd.Count * sd.MaxAttemptsPerItem;

            for (int i = 0; i < maxNumTimes && list.Count < sd.Count; i++)
            {
                MyPoint2 newpt = new MyPoint2();
                newpt.X = MathUtils.FloatRange(sd.XMin, sd.XMax, rand);
                newpt.Y = MathUtils.FloatRange(sd.YMin, sd.YMax, rand);

                double newDist = GeomUtils.GetMinDistance2(list, newpt);
                if (newDist > sd.MinSeparation)
                {
                    list.Add(newpt);
                }

            }



            return list;
        }


    }
}
