
using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.LineGen;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Client.Core;

public interface IAddRoadService : IInjectable
{
    void AddRoad(int sx, int sy, int cx, int cy, long extraSeed, MyRandom rand, bool primaryRoad,
        float roadTextureScale = 1.0f, int extraMapFlags = 0);
}


public class AddRoadService : IAddRoadService
{
    protected ILineGenService _lineGenService;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IMapGenData _md;

    protected MyPoint GetClosestEndpoint (List<MyPoint> list, int sx, int sy, int areaSize)
	{
		if (list == null || list.Count < 1)
        {
            return new MyPoint(areaSize/2,areaSize/2);
        }

        float minDist = areaSize*areaSize+10000;
		MyPoint closestPoint = null;

		foreach (MyPoint item in list)
		{
			float dist = MathUtils.Sqrt((item.X-sx)*(item.X-sx)+
			                        (item.Y-sy)*(item.Y-sy));
			if (dist < minDist)
			{
				minDist = dist;
				closestPoint = item;
			}
		}
		if (closestPoint == null)
		{
			closestPoint = new MyPoint(areaSize/2,areaSize/2);
		}
		return closestPoint;

	}
	
	public void AddRoad(int sx, int sy, int cx, int cy, long extraSeed, MyRandom rand, bool primaryRoad,
        float roadTextureScale = 1.0f, int extraMapFlags = 0)
	{
		if (rand == null)
        {
            return;
        }

		int extraDist = 0;

        MyRandom nextRand = new MyRandom(extraSeed);
		
		for (int i = 0; i < extraDist; i++)
		{
			if (sx < cx)
			{
				if (nextRand.Next () % 2 == 0)
                {
                    sx--;
                }

                if (nextRand.Next () % 2 == 0)
                {
                    cx++;
                }
            }
			if (sx > cx)
			{
				if (nextRand.Next () % 2 == 0)
                {
                    sx++;
                }

                if (nextRand.Next () % 2 == 0)
                {
                    cx--;
                }
            }
			if (sy < cy)
			{
				if (nextRand.Next () % 2 == 0)
                {
                    sy--;
                }

                if (nextRand.Next () % 2 == 0)
                {
                    cy++;
                }
            }
			if (sy > cy)
			{
				if (nextRand.Next () % 2 == 0)
                {
                    sy++;
                }

                if (nextRand.Next () % 2 == 0)
                {
                    cy--;
                }
            }
		}
		
		int awid = _md.awid;
		int ahgt = _md.ahgt;

        float[,,] alphamaps = _md.alphas;
		
		int ex = cx;
		int ey = cy;



        MyPoint sp = new MyPoint(sx,sy);
        MyPoint ep = new MyPoint(ex,ey);

        int startWidth = 4 + rand.Next() % 3 + rand.Next() % 3;

        if (!primaryRoad)
        {
            startWidth = startWidth / 2;
            if (startWidth < 4)
            {
                startWidth = 4;
            }
        }

        LineGenParameters ld = new LineGenParameters();
		ld.MinWidthSize = startWidth;
        ld.WidthSize = ld.MinWidthSize + 1 + (rand.NextDouble() < 0.3f ? 1 : 0);
        if (rand.NextDouble() < 0.50f)
        {
            ld.WidthSize++;
        }
        ld.MaxWidthSize = ld.WidthSize + 1;


        int extraBuffer = 8;
        ld.XMin = MapConstants.MapEdgeSize+extraBuffer;
        ld.YMin = MapConstants.MapEdgeSize+extraBuffer;
        ld.XMax = _mapProvider.GetMap().GetHwid() - MapConstants.MapEdgeSize-extraBuffer;
        ld.YMax = _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize-extraBuffer;

        for (int times = 0; times < 3; times++)
        {
            if (rand.NextDouble() < 0.50f-0.15*times)
            {
                ld.MaxWidthSize++;
            }
            else
            {
                break;
            }
        }
		
		ld.WidthSizeChangeAmount = 1+((rand.Next ()%3)/2);
		ld.WidthSizeChangeChance = 0.02f+0.07f*(float)(rand.NextDouble ());
	
		ld.WidthPosShiftChance = 0.03f + 0.05f*(float)(rand.NextDouble ());
		ld.WidthPosShiftSize = 1;

        ld.LinePathNoiseScale = MathUtils.FloatRange(0.6f, 1.0f, rand);

        ld.InitialNoPosShiftLength = 5 + rand.Next() % 5;

        ld.MaxWidthPosDrift = (Math.Abs(ey - sy) + Math.Abs(ex - sx)) / 4;

        if (FlagUtils.IsSet(extraMapFlags, MapGenFlags.VeryCurvedRoad))
        {
            ld.WidthPosShiftChance *= 2;
            ld.WidthPosShiftSize++;
            ld.LinePathNoiseScale *= 2;
            ld.InitialNoPosShiftLength = 3;
        }

        if (FlagUtils.IsSet(extraMapFlags, MapGenFlags.MinorRoad))
        {
            ld.WidthPosShiftChance = 0.1f;
            ld.WidthPosShiftSize = 1;
            ld.LinePathNoiseScale = 0;
            ld.InitialNoPosShiftLength = 10;
            ld.MaxWidthPosDrift = 1;
            ld.WidthSizeChangeChance = 0.01f;
            ld.WidthSizeChangeChance = 1;
            ld.MinWidthSize = 4;
            ld.WidthSize = 4;
            ld.MaxWidthSize = 4;
        }

		ld.Seed = _mapProvider.GetMap().Seed/7+147*extraSeed+3*extraSeed*extraSeed+163;

        List<MyPointF> line = _lineGenService.GetBressenhamLine(sp, ep, ld);

		MyRandom endpointRandom = new MyRandom(rand.Next ());
	
		List<MyPointF> centers = new List<MyPointF>();
		for (int l = 0; l < line.Count; l++)
		{
            MyPointF pt = line[l];
			int px = (int)(pt.X);
			int py = (int)(pt.Y);
			if (pt.X < 0 || pt.Y < 0 || pt.X >= awid || pt.Y >= ahgt)
            {
                continue;
            }

            _md.flags[px,py] |= extraMapFlags;

			float roadPercent = (float)(rand.NextDouble ()*0.2+0.8f)*roadTextureScale;

            float dirtPercent = 1 - roadPercent;
            float basePercent = 0;
            if (!primaryRoad)
            {
                float oldRoadPercent = roadPercent;
                roadPercent *= MathUtils.FloatRange(0.2f, 0.4f, rand);
                dirtPercent = oldRoadPercent - roadPercent;
                basePercent = 1 - roadPercent - dirtPercent;
            }

            if (FlagUtils.IsSet(extraMapFlags, MapGenFlags.MinorRoad))
            {
                dirtPercent = MathUtils.FloatRange(0.6f, 1.0f, rand);
                roadPercent = 1 - dirtPercent;
                basePercent = 0;
            }


            if (primaryRoad || alphamaps[px, py, MapConstants.RoadTerrainIndex] == 0 ||
                FlagUtils.IsSet(extraMapFlags, MapGenFlags.MinorRoad))
            {
                _md.ClearAlphasAt(px, py);
                alphamaps[px, py, MapConstants.BaseTerrainIndex] = basePercent;
                alphamaps[px, py, MapConstants.RoadTerrainIndex] = roadPercent;
                alphamaps[px, py, MapConstants.DirtTerrainIndex] = dirtPercent;
            }

			if (pt.Z > 0)
			{
				centers.Add (pt);
			}


            if (_md.mountainHeights != null)
            {
                _md.mountainHeights[px, py] = 0;
                _md.roadDistances[px, py] = 0;
            }
            // Scan out up down and left right to make things near road get flattened.
            for (int x = Math.Max(0,px- MapConstants.MaxRoadCheckDistance); x <= Math.Min(_mapProvider.GetMap().GetHwid()-1,px+ MapConstants.MaxRoadCheckDistance); x++)
            {
                float distFromRoad = Math.Abs(px - x);
                if (_md.roadDistances[x,py] > distFromRoad)
                {
                    _md.roadDistances[x, py] = distFromRoad;
                }
            }
            for (int y = Math.Max(0, py- MapConstants.MaxRoadCheckDistance); y <= Math.Min(_mapProvider.GetMap().GetHwid()-1,py+ MapConstants.MaxRoadCheckDistance); y++)
            {
                float distFromRoad = Math.Abs(py - y);
                if (_md.roadDistances[px,y] > distFromRoad)
                {
                    _md.roadDistances[px, y] = distFromRoad;
                }
            }
        }


		float endpointRadius = 2.5f;

		// Now add blobs around the endpoints
		if (centers.Count > 1)
		{
			MyPointF firstCenter = centers[0];
			MyPointF lastCenter = centers[centers.Count-1];
			AddRoadAroundPoint (firstCenter, endpointRadius, endpointRandom);
			AddRoadAroundPoint  (lastCenter, endpointRadius, endpointRandom);
		}

		// Now smooth the road to make points have heights close to their
		// nearest center points
		SmoothRoadNearCenterline(line,centers);


		if (_md.roads == null)
        {
            _md.roads = new List<List<MyPointF>>();
        }

        _md.roads.Add (centers);
		if (ex != cx || ey != cy)
		{		
			int ex1 = ex;
			int ey1 = ey;
			int cx1 = cx;
			int cy1 = cy;
			int edist = 5;
			ex1 -= edist*Math.Sign (cx1-ex1);
			cx1 += edist*Math.Sign (cx1-ex1);
			ey1 -= edist*Math.Sign (cy1-ey1);
			cy1 += edist*Math.Sign (cy1-ey1);
			
			AddRoad (ex1,ey1,cx1,cy1,extraSeed+101,rand,primaryRoad,extraMapFlags);
		}
		
		// Now draw a line from the mountains out to the edge
		// of the map.
		
		ld.Seed++;
	}

	public void AddRoadAroundPoint (MyPointF pt, float radius, MyRandom rand)
	{
		if (_md.alphas == null || pt == null || radius <= 0 || rand == null)
        {
            return;
        }

        int sz = (int)(Math.Abs (radius)+1);

		int px = (int)(pt.X);
		int py = (int)(pt.Y);

		for (int x = px-sz; x <= px+sz; x++)
		{
			if (x < 0 || x >= _md.awid)
            {
                continue;
            }

            for (int y = py-sz; y <= py + sz; y++)
			{
				if (y < 0 || y >= _md.ahgt)
                {
                    continue;
                }

                float dist = MathUtils.Sqrt((x-px)*(x-px)+(y-py)*(y-py));
				if (dist > radius)
                {
                    continue;
                }

                float roadPct = MathUtils.FloatRange(0.8f,1.0f,rand);
				_md.ClearAlphasAt(x,y);
				_md.alphas[x,y,MapConstants.BaseTerrainIndex] = 1-roadPct;
				_md.alphas[x,y,MapConstants.RoadTerrainIndex] = roadPct;
			}
		}
	}


    public void SmoothRoadNearCenterline(List<MyPointF> line, List<MyPointF> centers, float distScaling = 0.1f)
    {
        if (line == null || centers == null)
        {
            return;
        }

        bool xSame = false;
        bool ySame = false;


        for (int p = 0; p < line.Count; p++)
        {
            MyPointF pt = line[p];
            if (pt.Z != 1)
            {
                continue;
            }

            int rad = 1;
            for (int p1 = p - rad; p1 <= p + rad; p1++)
            {
                if (p1 != p && p1 >= 0 && p1 < line.Count)
                {
                    MyPointF pt1 = line[p1];
                    if (pt1.Z == 0)
                    {
                        // vertical line
                        if (pt1.X == pt.X)
                        {
                            xSame = false;
                            ySame = true;
                            break;
                        }
                        if (pt1.Y == pt.Y)
                        {
                            xSame = true;
                            ySame = false;
                        }
                    }
                }
                if (xSame || ySame)
                {
                    break;
                }
            }
            if (xSame || ySame)
            {
                break;
            }
        }

        if (!xSame && !ySame)
        {

        }

        for (int p = 0; p < line.Count; p++)
        {
            if (line[p].Z != 1)
            {
                continue;
            }

            int startx = (int)line[p].X;
            int endx = (int)line[p].X;
            int starty = (int)line[p].Y;
            int endy = (int)line[p].Y;

            for (int times = 0; times < 2; times++)
            {
                int dp = 1 - 2 * times;

                for (int p1 = p - 1; p1 >= 0 && p1 < line.Count; p1 += dp)
                {
                    if (xSame && line[p1].X == line[p].X)
                    {
                        int yval = (int)(line[p1].X);
                        starty = Math.Min(yval, starty);
                        endy = Math.Max(yval, endy);
                    }
                    else if (ySame && line[p1].Y == line[p].Y)
                    {
                        int xval = (int)(line[p1].Y);
                        startx = Math.Min(xval, startx);
                        endx = Math.Max(xval, endx);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            startx = MathUtils.Clamp(0, startx, _mapProvider.GetMap().GetHwid()-1);
            endx = MathUtils.Clamp(0, endx, _mapProvider.GetMap().GetHwid() - 1);
            starty = MathUtils.Clamp(0, starty, _mapProvider.GetMap().GetHhgt() - 1);
            endy = MathUtils.Clamp(0, endy, _mapProvider.GetMap().GetHhgt() - 1);


            int totalCells = 0;
            float totalHeight = 0;

            float aveHeight = 0;

            if (startx != endx)
            {
                for (int x = startx; x < endx; x++)
                {
                    totalHeight += _md.heights[x, starty];
                    totalCells++;
                }
                aveHeight = totalHeight / totalCells;
                for (int x = startx; x < endx; x++)
                {
                    _md.heights[x, starty] = aveHeight;
                }
            }
            else if (starty != endy)
            {
                for (int y = starty; y <= endy; y++)
                {
                    totalHeight += _md.heights[startx, y];
                    totalCells++;
                }
                aveHeight = totalHeight / totalCells;
                for (int y = starty; y <= endy; y++)
                {
                    _md.heights[startx, y] = aveHeight;
                }
            }           
        }
    }
}

