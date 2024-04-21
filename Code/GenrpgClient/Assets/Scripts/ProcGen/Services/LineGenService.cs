using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.LineGen;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
public interface ILineGenService : IInitializable
{
    List<MyPointF> GetBressenhamLine(GameState gs, MyPoint start, MyPoint end, LineGenParameters lg = null);
    List<MyPointF> GetBressenhamCircle(GameState gs, MyPoint center, LineGenParameters pars);
    List<ConnectedPairData> ConnectPoints(GameState gs, List<ConnectPointData> points, MyRandom rand, float extraConnectionPct = 0.0f);
}

public class LineGenService : ILineGenService
{
    protected INoiseService _noiseService;
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public List<MyPointF> GetBressenhamLine(GameState gs, MyPoint start, MyPoint end, LineGenParameters lg = null)
    {
        if (lg == null)
        {
            lg = new LineGenParameters();
        }


        start.X = MathUtils.Clamp(lg.XMin, start.X, lg.XMax);
        start.Y = MathUtils.Clamp(lg.YMin, start.Y, lg.YMax);
        end.X = MathUtils.Clamp(lg.XMin, end.X, lg.XMax);
        end.Y = MathUtils.Clamp(lg.YMin, end.Y, lg.YMax);

        MyRandom rand = new MyRandom(lg.Seed);

        bool addToEndOnEvenWidth = rand.NextDouble() < 0.5f;

        List<MyPointF> retval = new List<MyPointF>();
        if (start == null || end == null)
        {
            return retval;
        }

        double widthPosDrift = lg.MaxWidthPosDrift * (1 - 2 * rand.NextDouble());

        // Only used to determine which axis is longer, we move along that axis.
        int dx = end.X - start.X;
        int dy = end.Y - start.Y;

        int remainder = 0;

        int pathWidth = lg.WidthSize; // curr width of the line.

        bool xAxis = false;

        int length = 0; // Along major axis
        int width = 0; // Along minor axis

        int dl = 0;
        int dw = 0;

        int sl = 0;
        int sw = 0;
        int el = 0;
        int ew = 0;
        float cl = 0;
        float widthDelta = Math.Abs(lg.MaxWidthSize - lg.MinWidthSize);
        float startWidth = lg.MinWidthSize;

        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            xAxis = true;
            length = Math.Abs(dx);
            width = Math.Abs(dy);
            dl = Math.Sign(dx);
            dw = Math.Sign(dy);
            sl = start.X;
            sw = start.Y;
            el = end.X;
            ew = end.Y;
            cl = (sl + el) / 2.0f;
        }
        else // Swap x and y axis.
        {
            xAxis = false;
            length = Math.Abs(dy);
            width = Math.Abs(dx);
            dl = Math.Sign(dy);
            dw = Math.Sign(dx);
            sl = start.Y;
            sw = start.X;
            el = end.Y;
            ew = end.X;
            cl = (sl + el) / 2.0f;
        }
        int[] offsets = new int[length + 1];

        if (lg.LinePathNoiseScale > 0)
        {
            float freq = MathUtils.FloatRange(0.0150f, 0.020f, rand) * length * 0.3f;
            if (rand.NextDouble() < 0.2f)
            {
                freq *= MathUtils.FloatRange(0.8f, 1.2f, rand);

            }

            if (lg.LinePathNoiseScale > 1)
            {
                freq *= lg.LinePathNoiseScale;
            }

            float amp = MathUtils.FloatRange(0.3f, 0.4f, rand);
            int octaves = 2;
            float pers = MathUtils.FloatRange(0.3f, 0.4f, rand);
            float[,] offsets2 = _noiseService.Generate(gs, pers, freq, amp, octaves, rand.Next(), length + 1, length + 1);
            if (offsets2 != null && offsets2.Length > length)
            {
                for (int x = 0; x < length + 1; x++)
                {
                    offsets2[x, x] *= lg.LinePathNoiseScale * length;
                    float distPctFromEnd = Math.Min(1.0f * x / length, (length - x) * 1.0f / length);
                    offsets2[x, x] *= distPctFromEnd;
                    int newOffset = (int)offsets2[x, x];
                    if (x > 0)
                    {
                        offsets[x] = MathUtils.Clamp(offsets[x - 1] - 1, newOffset, offsets[x - 1] + 1);
                    }
                    else
                    {
                        offsets[x] = newOffset;
                    }
                    offsets[x] = MathUtils.Clamp(-x, offsets[x], x);
                    offsets[x] = MathUtils.Clamp(-(length + 1 - x), offsets[x], length + 1 - x);
                }
            }
        }


        int correctWidthPos = sw;
        int startWidthPos = 0;
        int endWidthPos = 0;

        int currWidthPos = sw;

        List<MyPoint> oldPos = new List<MyPoint>();

        oldPos.Add(new MyPoint(sw, sw));

        int index = 0;
        for (int l = sl; l != el; l += dl, index++)
        { 
            bool canShift = Math.Abs(l - sl) >= lg.InitialNoPosShiftLength;

            remainder += width;
            startWidthPos = currWidthPos;
            if (remainder >= length)
            {
                remainder -= length;
                correctWidthPos += dw;
                currWidthPos += dw;
            }
            endWidthPos = currWidthPos;

            // Possibly divert the path to left or right.

            if (lg.WidthPosShiftChance >= rand.NextDouble() && canShift)
            {
                int skewAmount = rand.Next(0, lg.WidthPosShiftSize + 1);
                if (rand.NextDouble() >= 0.5)
                {
                    skewAmount = -skewAmount;
                }
                startWidthPos += skewAmount;
                endWidthPos += skewAmount;
                currWidthPos += skewAmount;
            }

            if (widthPosDrift < 0 && rand.NextDouble() < -widthPosDrift)
            {
                startWidthPos--;
                endWidthPos--;
                currWidthPos--;
            }
            else if (widthPosDrift > 0 && rand.NextDouble() < widthPosDrift)
            {

                startWidthPos++;
                endWidthPos++;
                currWidthPos++;
            }

            // Push the path back to its correct position along the width.
            int widthPosDelta = 0;
            int widthPosError = currWidthPos - correctWidthPos;
            float snapToCenterChance = 0.02f;
            if (length > 0)
            {
                snapToCenterChance = 0.1f / length;
            }
            int absWidthPosError = Math.Abs(widthPosError);
            int signWidthPosError = Math.Sign(widthPosError);
            if (rand.NextDouble() < snapToCenterChance * absWidthPosError)
            {
                widthPosDelta = -signWidthPosError;
            }

            currWidthPos += widthPosDelta;
            startWidthPos += widthPosDelta;
            endWidthPos += widthPosDelta;

            // Now do a final correction toward the final destination

            float errorGapMult = 0.3f;
            int finalErrorGapToEnd = Math.Abs(l - el);
            int errorGapToEnd = (int)(errorGapMult * Math.Abs(l - el));
            if (absWidthPosError > errorGapToEnd && rand.NextDouble() < errorGapMult ||
                absWidthPosError > finalErrorGapToEnd)
            {
                int widthPosGap = absWidthPosError - errorGapToEnd;
                if (widthPosGap > 2)
                {
                    widthPosGap = 2;
                }

                widthPosGap *= -signWidthPosError;

                currWidthPos += widthPosGap;
                startWidthPos += widthPosGap;
                endWidthPos += widthPosGap;
            }

            // Make path wider or narrower.

            if (lg.WidthSizeChangeChance >= rand.NextDouble())
            {
                int size = Math.Max(1, lg.WidthSizeChangeAmount);
                int delta = rand.Next(-size, size + 1);
                pathWidth = MathUtils.Clamp(lg.MinWidthSize, pathWidth + delta, lg.MaxWidthSize);
            }

            // Now make path return to its normal size slowly.
            if (pathWidth > lg.WidthSize && rand.NextDouble() < 0.1 * (pathWidth - lg.WidthSize))
            {
                pathWidth--;
            }

            if (pathWidth < lg.WidthSize && rand.NextDouble() < 0.1 * (lg.WidthSize - pathWidth))
            {
                pathWidth++;
            }

            if (pathWidth > 1)
            {
                endWidthPos += pathWidth / 2;
                startWidthPos -= pathWidth / 2;

                if (pathWidth % 2 != 0)
                {
                    if (addToEndOnEvenWidth)
                    {
                        endWidthPos++;
                    }
                    else
                    {
                        startWidthPos--;
                    }
                }

            }

            // Ellipse shape from minWidth to MaxWidth in center along path and then back down to MinWidth.
            if (lg.UseOvalWidth && length > 6)
            {
                float distFromCenter = Math.Abs(l - cl);
                double currWidth = lg.MinWidthSize +
                                    Math.Sqrt(Math.Max(0.1f, (1 - distFromCenter * distFromCenter / (length / 2.0f * (length / 2.0f))) * widthDelta * widthDelta));
                pathWidth = (int)currWidth;
            }

            if (oldPos.Count > 0)
            {
                MyPoint prevPos = oldPos[oldPos.Count - 1];

                if (endWidthPos < prevPos.X)
                {
                    endWidthPos = prevPos.X;
                }

                if (startWidthPos > prevPos.Y)
                {
                    startWidthPos = prevPos.Y;
                }

                int biggestPrevStart = -100000000;
                int smallestPrevEnd = 100000000;

                for (int c = oldPos.Count - 1; c >= 0 && c >= oldPos.Count - lg.MinOverlap - 1; c--)
                {
                    int pstart = oldPos[c].X;
                    int pend = oldPos[c].Y;

                    if (pstart > biggestPrevStart)
                    {
                        biggestPrevStart = pstart;
                    }

                    if (pend < smallestPrevEnd)
                    {
                        smallestPrevEnd = pend;
                    }
                }

                int lowOverlap = endWidthPos + 1 - biggestPrevStart;
                int highOverlap = smallestPrevEnd + 1 - startWidthPos;


                if (lowOverlap > highOverlap && lowOverlap < lg.MinOverlap)
                {
                    endWidthPos += lg.MinOverlap - lowOverlap;
                }
                else if (highOverlap < lg.MinOverlap)
                {
                    startWidthPos -= lg.MinOverlap - highOverlap;
                }

                int oldDiff = endWidthPos - startWidthPos;
                if (oldDiff > 5)
                {
                    oldDiff = 5;
                }

                if (startWidthPos < lg.XMin)
                {
                    startWidthPos = lg.XMin;
                    endWidthPos = startWidthPos + oldDiff;
                }
                if (endWidthPos > lg.XMax)
                {
                    endWidthPos = lg.XMax;
                    startWidthPos = endWidthPos - oldDiff;
                }
            }

            for (int w = startWidthPos; w <= endWidthPos; w++)
            {
                MyPointF pt = null;
                if (xAxis)
                {
                    pt = new MyPointF(l, w + offsets[index]);

                }
                else
                {
                    pt = new MyPointF(w + offsets[index], l);
                }

                if (w == (startWidthPos + endWidthPos) / 2)
                {
                    pt.Z = 1;
                }
                else if (w == startWidthPos || w == endWidthPos)
                {
                    pt.Z = -1;
                }

                retval.Add(pt);

            }
            oldPos.Add(new MyPoint(startWidthPos, endWidthPos));
        }
        return retval;
    }
    public List<MyPointF> GetBressenhamCircle(GameState gs, MyPoint center, LineGenParameters pars)
    {
        List<MyPointF> retval = new List<MyPointF>();
        if (center == null || pars == null)
        {
            return retval;
        }

        int numSegments = 16;

        MyRandom circRand = new MyRandom(pars.Seed);

        pars.MaxWidthSize = 1;



        for (int i = 0; i < numSegments; i++)
        {
            double startx = center.X + pars.XRadius * Math.Cos(1.0f * i / numSegments * Math.PI * 2);
            double starty = center.Y + pars.YRadius * Math.Sin(1.0f * i / numSegments * Math.PI * 2);
            double endx = center.X + pars.XRadius * Math.Cos(1.0f * (i + 1) / numSegments * Math.PI * 2);
            double endy = center.Y + pars.YRadius * Math.Sin(1.0f * (i + 1) / numSegments * Math.PI * 2);
            MyPoint startPt = new MyPoint((int)startx, (int)starty);
            MyPoint endPt = new MyPoint((int)endx, (int)endy);
            pars.Seed = circRand.Next();
            pars.MaxWidthPosDrift = 3;
            List<MyPointF> newPts = GetBressenhamLine(gs, startPt, endPt, pars);
            if (newPts != null)
            {
                foreach (MyPointF item in newPts)
                {
                    retval.Add(item);
                }
            }
        }

        return retval;
    }

    public List<ConnectedPairData> ConnectPoints(GameState gs, List<ConnectPointData> points, MyRandom rand, float extraConnectionPct = 0.0f)
    {
        if (points == null || points.Count < 1 || rand == null)
        {
            return new List<ConnectedPairData>();
        }


        int nextConnectSet = 1;

        List<ConnectedPairData> allPairs = new List<ConnectedPairData>();

        for (int p1 = 0; p1 < points.Count; p1++)
        {
            for (int p2 = p1 + 1; p2 < points.Count; p2++)
            {
                ConnectedPairData cpd = new ConnectedPairData();
                cpd.Point1 = points[p1];
                cpd.Point2 = points[p2];
                double dx = cpd.Point1.X - cpd.Point2.X;
                double dy = cpd.Point1.Z - cpd.Point2.Z;
                cpd.Distance = Math.Sqrt(dx * dx + dy * dy);
                allPairs.Add(cpd);
            }
        }

        allPairs = allPairs.OrderBy(x => x.Distance).ToList();

        List<ConnectedPairData> remainingPairs = new List<ConnectedPairData>(allPairs);

        List<ConnectedPairData> finalConnections = new List<ConnectedPairData>();


        foreach (ConnectedPairData pair in allPairs)
        {
            ConnectPointData center1 = pair.Point1;
            ConnectPointData center2 = pair.Point2;

            if (center1.Adjacencies.Count >= center1.MaxConnections ||
                center2.Adjacencies.Count >= center2.MaxConnections)
            {
                continue;
            }

            if (center1.ConnectSet == 0 && center2.ConnectSet == 0)
            {
                center1.ConnectSet = nextConnectSet;
                center2.ConnectSet = nextConnectSet;
                nextConnectSet++;
            }
            else if (center1.ConnectSet == 0 && center2.ConnectSet > 0)
            {
                center1.ConnectSet = center2.ConnectSet;
            }
            else if (center2.ConnectSet == 0 && center1.ConnectSet > 0)
            {
                center2.ConnectSet = center1.ConnectSet;
            }
            else if (center1.ConnectSet != center2.ConnectSet)
            {
                // Set the zoneSet to the min value.

                int maxValue = Math.Max(center1.ConnectSet, center2.ConnectSet);
                int minValue = Math.Min(center1.ConnectSet, center2.ConnectSet);

                // Loop over all centers and set their ZoneSet to the min value.
                foreach (ConnectPointData point in points)
                {
                    if (point.ConnectSet == maxValue)
                    {
                        point.ConnectSet = minValue;
                    }
                }
            }
            else // Same component. Keep it in remaining roads but don't make it now.
            {
                continue;
                // Do nothing.
            }

            finalConnections.Add(pair);
            remainingPairs.Remove(pair);
            center1.Adjacencies.Add(center2);
            center2.Adjacencies.Add(center1);


            if (pair.Distance < center1.MinDistToOther)
            {
                center1.MinDistToOther = pair.Distance;
            }
            if (pair.Distance < center2.MinDistToOther)
            {
                center2.MinDistToOther = pair.Distance;
            }
        }
        int midRoadsToAdd = (int)(finalConnections.Count * extraConnectionPct);

        int maxRoadsToAdd = MathUtils.IntRange(midRoadsToAdd / 2, midRoadsToAdd * 3 / 2, rand);

        for (int i = 0; i < maxRoadsToAdd; i++)
        {
            List<ConnectedPairData> okSecondaryConnections = new List<ConnectedPairData>();

            foreach (ConnectedPairData pair in remainingPairs)
            {
                if (IsOkSecondaryConnection(pair, finalConnections, points))
                {
                    okSecondaryConnections.Add(pair);
                }
            }

            if (okSecondaryConnections.Count < 1)
            {
                break;
            }

            ConnectedPairData newSecondaryConnection = okSecondaryConnections[rand.Next() % okSecondaryConnections.Count];

            ConnectPointData point1 = newSecondaryConnection.Point1;
            ConnectPointData point2 = newSecondaryConnection.Point2;
            finalConnections.Add(newSecondaryConnection);
            okSecondaryConnections.Remove(newSecondaryConnection);
            point1.Adjacencies.Add(point2);
            point2.Adjacencies.Add(point1);
            remainingPairs = okSecondaryConnections;
        }

        return finalConnections;
    }


    private bool IsOkSecondaryConnection(ConnectedPairData pair, List<ConnectedPairData> allPairs, List<ConnectPointData> points)
    {
        float extraDistMult = 2.5f;
        float legSumMult = 1.2f; // Sum of lengths of road legs need to be at least this times the direct distance.

        ConnectPointData c1 = pair.Point1;
        ConnectPointData c2 = pair.Point2;


        if (pair.Distance > extraDistMult * c1.MinDistToOther)
        {
            return false;
        }

        if (pair.Distance > extraDistMult * c2.MinDistToOther)
        {
            return false;
        }


        // Get everything connected to c1.
        List<ConnectedPairData> firstConnList = allPairs.Where(x => x.Point1 == c1 || x.Point2 == c1).ToList();

        // Iterate over all of those connections to get adjacent points that are not c2.
        foreach (ConnectedPairData firstConn in firstConnList)
        {
            ConnectPointData centerPoint = null;

            if (firstConn.Point1 != c1 && firstConn.Point1 != c2)
            {
                centerPoint = firstConn.Point1;
            }
            else if (firstConn.Point2 != c1 && firstConn.Point2 != c2)
            {
                centerPoint = firstConn.Point2;
            }

            if (centerPoint == null)
            {
                continue;
            }

            // Now find everything connected to otherPoint that's also connected to c2.

            foreach (ConnectedPairData secondConn in allPairs)
            {
                ConnectPointData otherPoint = null;
                if (secondConn.Point1 == c2 && secondConn.Point2 != c1)
                {
                    otherPoint = secondConn.Point2;
                }
                else if (secondConn.Point2 == c2 && secondConn.Point1 != c1)
                {
                    otherPoint = secondConn.Point1;
                }

                if (otherPoint == null)
                {
                    continue;
                }

                double distSum = firstConn.Distance + secondConn.Distance;

                // If the sums of the legs of these two roads is less than legSumMult (1.2 or so) of the
                // distance between the two points, then the two connections are too close to where the
                // new road will go, so disallow it.
                if (distSum < pair.Distance * legSumMult)
                {
                    return false;
                }

            }
        }

        return true;

    }
}