using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Threading;

public class RemoveSetupZonePatches : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {

        await base.Generate(gs, token);
        List<MyPoint2> deltas = new List<MyPoint2>();
        deltas.Add(new MyPoint2(-1, 0));
        deltas.Add(new MyPoint2(1, 0));
        deltas.Add(new MyPoint2(0, 1));
        deltas.Add(new MyPoint2(0, -1));

        MyRandom rand = new MyRandom(gs.map.Seed / 5);

        int numIterations = 0;
        bool somethingchanged = false;
      
        do
        {
            somethingchanged = false;
            numIterations++;
            List<MyPointF> addedVals = new List<MyPointF>();
            for (int x = 1; x < gs.map.GetHwid()-1; x++)
            {
           
                for (int y = 1; y < gs.map.GetHhgt()-1; y++)
                {
                    if (gs.md.mapZoneIds[x,y] <= MapConstants.MountainZoneId)
                    {
                        List<int> choices = new List<int>();
                        foreach (MyPoint2 d in deltas)
                        {
                            short nearZoneId = gs.md.mapZoneIds[x + (int)(d.X), y + (int)(d.Y)];
                            if (nearZoneId > MapConstants.MountainZoneId)
                            { 
                                choices.Add(nearZoneId);
                            }
                        }
                        if (choices.Count > 0)
                        {
                            int choice = choices[rand.Next() % choices.Count];
                            addedVals.Add(new MyPointF(x, y, choice));
                            somethingchanged = true;
                        }
                    }
                }
            }

            foreach (MyPointF val in addedVals)
            {
                gs.md.mapZoneIds[(int)(val.X),(int)(val.Y)] = (short)(val.Z);
            }
        }
        while (somethingchanged);
    }
}