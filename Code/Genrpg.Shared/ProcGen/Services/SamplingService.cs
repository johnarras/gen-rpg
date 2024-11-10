using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
public interface ISamplingService : IInitializable
{
    List<MyPoint2> PlanePoissonSample(SamplingData sd);
}
public class SamplingService : ISamplingService
{
    private INoiseService _noiseService = null;
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public List<MyPoint2> PlanePoissonSample(SamplingData sd)
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

        float[,] noise = null;

        int width = (int)(sd.XMax - sd.XMin + 1);
        int height = (int)(sd.YMax - sd.YMin + 1);
        if (sd.NoiseAmp > 0 && sd.NoiseFreq > 0)
        {
            float pers = MathUtils.FloatRange(0.2f, 0.6f, rand);

            if (width <= 20000 && height <= 20000)
            {
                noise = _noiseService.Generate(pers, sd.NoiseFreq, sd.NoiseAmp, 2, rand.Next(), width, height);
            }
        }

        int maxNumTimes = sd.Count * sd.MaxAttemptsPerItem;

        for (int i = 0; i < maxNumTimes && list.Count < sd.Count; i++)
        {
            MyPoint2 newpt = new MyPoint2();
            newpt.X = MathUtils.FloatRange(sd.XMin, sd.XMax, rand);
            newpt.Y = MathUtils.FloatRange(sd.YMin, sd.YMax, rand);

            double newDist = GeomUtils.GetMinDistance2(list, newpt);

            double currSeparation = sd.MinSeparation;

            if (noise != null)
            {
                int dx = MathUtils.Clamp(0, (int)(newpt.X - sd.XMin), width - 1);
                int dy = MathUtils.Clamp(0, (int)(newpt.Y - sd.YMin), height - 1);
                currSeparation *= (1 + noise[dx, dy]);
                currSeparation = MathUtils.Clamp(sd.MinSeparation / 4, currSeparation, sd.MinSeparation * 2);
            }

            if (newDist > currSeparation)
            {
                list.Add(newpt);
            }

        }



        return list;
    }
}
