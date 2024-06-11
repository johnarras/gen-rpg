using System.Collections.Generic;

using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;

public class RemoveSetupZonePatches : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        List<MyPoint2> deltas = new List<MyPoint2>();
        deltas.Add(new MyPoint2(-1, 0));
        deltas.Add(new MyPoint2(1, 0));
        deltas.Add(new MyPoint2(0, 1));
        deltas.Add(new MyPoint2(0, -1));

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed / 5);

        int numIterations = 0;
        bool somethingchanged = false;
      
        do
        {
            somethingchanged = false;
            numIterations++;
            List<MyPointF> addedVals = new List<MyPointF>();
            for (int x = 1; x < _mapProvider.GetMap().GetHwid()-1; x++)
            {
           
                for (int y = 1; y < _mapProvider.GetMap().GetHhgt()-1; y++)
                {
                    if (_md.mapZoneIds[x,y] <= MapConstants.MountainZoneId)
                    {
                        List<int> choices = new List<int>();
                        foreach (MyPoint2 d in deltas)
                        {
                            short nearZoneId = _md.mapZoneIds[x + (int)(d.X), y + (int)(d.Y)];
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
                _md.mapZoneIds[(int)(val.X),(int)(val.Y)] = (short)(val.Z);
            }
        }
        while (somethingchanged);
    }
}