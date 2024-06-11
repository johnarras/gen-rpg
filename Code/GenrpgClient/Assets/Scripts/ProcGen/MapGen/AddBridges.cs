
using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.ProcGen.Settings.Bridges;
using Genrpg.Shared.ProcGen.Settings.MapWater;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Zones.WorldData;
using Assets.Scripts.ProcGen.Loading.Utils;
using UnityEngine;

public class AddBridges : BaseZoneGenerator
{

    private IAddPoolService _addPoolService;
    private List<WaterGenData> _waterGenData = new List<WaterGenData>();

	public const string DefaultBridgeArtName = "Bridge";
	public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);
        if (_md.bridgeDistances == null)
        {
            _md.bridgeDistances = new ushort[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
        }

        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                _md.bridgeDistances[x, y] = 10000;
            }
        }
        if (_md.currBridges == null)
        {
            _md.currBridges = new List<MyPointF>();
        }

        if (_md.roads == null)
        {
            return;
        }

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 12389292 + 333);

		if (_md.creviceBridges != null)
		{
			foreach (MyPointF bpos in _md.creviceBridges)
			{
				foreach (List<MyPointF> road in _md.roads)
				{
					foreach (MyPointF pt in road)
					{
						if (Math.Abs (bpos.X-pt.X) <= 2 && Math.Abs (bpos.Y-pt.Y) <= 2 &&
						    pt.Z == 1)
						{
							AddOneBridge (road,rand,bpos);
						}
					}
				}
			}
		}

		
		
		foreach (List<MyPointF> road in _md.roads)
		{
			int numBridgesToTry = 0;
			if (rand.Next() % 14 == 0)
            {
                numBridgesToTry++;
            }

            if (rand.Next() % 50 == 0)
            {
                numBridgesToTry++;
            }

            if (road.Count > 0)
            {
                int dx = (int)Math.Abs(road[0].X - road[road.Count - 1].X);
                int dy = (int)Math.Abs(road[0].Y - road[road.Count - 1].Y);
                int maxDist = Math.Max(dx, dy);
                numBridgesToTry += maxDist / (60 + rand.Next() % 60);
            }
		
			for (int tries = 0; tries < numBridgesToTry; tries++)
			{
				AddOneBridge (road, rand);
			}
		}

        foreach (WaterGenData wgd in _waterGenData)
        {
            _addPoolService.TryAddPool(wgd);
        }
	}
	
	protected void AddOneBridge (
	                             List<MyPointF> road,
	                             MyRandom rand, 
	                             MyPointF centerPointIn = null)
	{
		if (road == null)
        {
            return;
        }

        List<MyPointF> road2 = new List<MyPointF>();

		foreach (MyPointF pt in road)
		{
			if (pt.Z == 1)
			{
				road2.Add (pt);
			}
		}

		road=road2;


        int xpos = MapConstants.TerrainPatchSize;
        int ypos = MapConstants.TerrainPatchSize;

        if (road.Count > 0)
        {
            MyPointF midpoint = road[road.Count / 2];
            xpos = (int)midpoint.X;
            ypos = (int)midpoint.Y;
        }

        
        int zoneId = _md.mapZoneIds[xpos, ypos]; // zoneobject

        Zone zone = _mapProvider.GetMap().Get<Zone>(zoneId);

        long zoneTypeId = (zone != null ? zone.ZoneTypeId : 1);

	    BridgeType bt = GetRandomBridgeType (zoneTypeId, rand);
		
		float bridgeLength = 6;
		string bridgeArt = DefaultBridgeArtName;
		
		if (bt != null && !string.IsNullOrEmpty(bt.Art))
		{
			bridgeArt = bt.Art;
			bridgeLength = bt.Length;
		}

        float radius = 12;

		// Get length of bridge
		int halfBridgeLength = (int)(bridgeLength/2);
		int bridgeDistanceFromEnd = (int)(radius+halfBridgeLength*2);
		int bridgeDistanceFromStart = (int)(radius+halfBridgeLength*2);
		if (road.Count <= bridgeDistanceFromEnd+bridgeDistanceFromStart)
        {
            return;
        }

		int cval = 0;
		
		MyPointF centerpt = null;

		// Don't let the bridge go too close to a location.

		for (int times = 0; times < 80; times++)
		{
			if (times > 0 && centerPointIn != null)
			{
				break;
			}

			// Not to close to road end
			centerpt = null;
			cval = MathUtils.IntRange (bridgeDistanceFromStart,road.Count-bridgeDistanceFromEnd,rand);

			if (cval < halfBridgeLength || cval >= road.Count-halfBridgeLength)
            {
                continue;
            }

            centerpt = road[cval];

			if (centerPointIn != null)
			{
				centerpt = centerPointIn;
			}

			if (centerpt == null) 
			{
				continue;
			}

            int avgRadius = 3; // Average points down the road, with 2*this+1 points being used.

            int numCellsChecked = 2 * avgRadius + 1;
            int startIndex = MathUtils.Clamp(avgRadius, cval - halfBridgeLength, road.Count - 1-avgRadius);
            int endIndex = MathUtils.Clamp(avgRadius, cval + halfBridgeLength, road.Count -1 -avgRadius);

            float fsx = 0;
            float fsy = 0;
            float fex = 0;
            float fey = 0;

            for (int i = -avgRadius; i <= avgRadius; i++)
            {
                int sindex = startIndex + i;
                int eindex = endIndex + i;

                fsx += road[sindex].X / numCellsChecked;
                fsy += road[sindex].Y / numCellsChecked;
                fex += road[eindex].X / numCellsChecked;
                fey += road[eindex].Y / numCellsChecked;
            }

			
			int ex = MathUtils.Clamp (0,(int)(fex),_mapProvider.GetMap().GetHwid());
			int ez = MathUtils.Clamp (0,(int)(fey),_mapProvider.GetMap().GetHhgt());
			int sx = MathUtils.Clamp (0,(int)(fsx),_mapProvider.GetMap().GetHwid());
			int sz = MathUtils.Clamp (0,(int)(fsy),_mapProvider.GetMap().GetHhgt());


            int shrinkMod = rand.Next() % 2;
			// Move ex and sx closer together if they're too far apart.
			int shrinkLengthTimes = 0;
			do
			{
				float lx = ex-sx;
				float lz = ez-sz;
				float len = MathUtils.Sqrt (lx*lx+lz*lz);
				if (len <= bridgeLength+1)
                {
                    break;
                }

                if (ez < sz)
                {
                    ez++;
                }
                else if (sz < ez)
                {
                    sz++;
                }

                if (ex < sx)
                {
                    ex++;
                }
                else if (sx < ex)
                {
                    sx++;
                }
            }
			while (++shrinkLengthTimes < 20);
			
			
			
			
			int cx = (int)(ex+sx)/2;
			int cz = (int)(ez+sz)/2;

            Location loc = _zoneGenService.FindMapLocation(cx, cz, 15);

            if (loc != null)
            {
                continue;
            }

            // These are the actual location points for the bridge. They are
            // calced using s and e average, rather than cx,cy.
            float px = (ex+sx)/2.0f;
			float pz = (ez+sz)/2.0f;

            // Not too close to the edge of the map
            float edgeSize = MapConstants.TerrainPatchSize;

			if (px < edgeSize || px > _mapProvider.GetMap().GetHwid()-edgeSize ||
			    pz < edgeSize || pz > _mapProvider.GetMap().GetHhgt()-edgeSize)
			{
				continue;
			}


            // Now make sure we're not near a bridge.

            int minBridgeSeparation = 110;


            
			bool nearBridge = false;
			foreach (MyPointF pt3 in _md.currBridges)
			{

				if (Math.Abs (px-pt3.X) <= minBridgeSeparation &&
				    Math.Abs (pz-pt3.Y) <= minBridgeSeparation)
				{
					nearBridge = true;
					break;
				}
			}
			if (nearBridge)
			{
				centerpt = null;
				continue;		

			}

		

			if (cx-halfBridgeLength < 0 ||
			    cz-halfBridgeLength < 0 ||
			    cx+halfBridgeLength >= _mapProvider.GetMap().GetHwid() ||
			    cz+halfBridgeLength >= _mapProvider.GetMap().GetHhgt())
			{	
				centerpt = null;
				continue;
			}

			
			float sy = _md.heights[sx,sz]*MapConstants.MapHeight;
            float my = _md.heights[(sx + ex) / 2, (sz + ez) / 2] * MapConstants.MapHeight;
			float ey = _md.heights[ex,ez]*MapConstants.MapHeight;
			float cy = (sy+ey+my)/3;

            float minHeight = Math.Min(sy, Math.Min(my, ey));
            float maxHeight = Math.Max(sy, Math.Max(my, ey));

            float startHeightDiff = maxHeight - minHeight;

            cy = minHeight + MathUtils.FloatRange(0.0f, 0.25f, rand) * startHeightDiff;

			float cyscale = cy/MapConstants.MapHeight;
			
			float bridgeHeight = cy-0.4f;

            if (bridgeHeight <= MapConstants.MinLandHeight)
            {
                continue;
            }

			float bridgeHeightPercent = bridgeHeight/MapConstants.MapHeight;
			float heightAtEnds = bridgeHeightPercent+0.15f/MapConstants.MapHeight;

			float maxHeightchange = 3.0f;
			if (Math.Abs (sy-ey) > maxHeightchange)
			{
				continue;
			}
			if (centerpt == null)
			{
				continue;
			}

            string bridgeName = "Bridge" + (int)(px) + "x" + (int)(pz);


            float dx = ex - sx;
            float dy = ez - sz;

            float angle = (float)Math.Atan2(dy, dx);

            angle = (float)(angle * 180.0f / Math.PI);


            if (rand.Next() % 2 == 0)
            {
                angle += 180;
            }

            float oldpx = px;
            float oldpz = pz;

            int ipx = (int)(px);
            int ipz = (int)(pz);

            if (ipz % MapConstants.TerrainPatchSize > MapConstants.TerrainPatchSize - 3)
            {
                continue;
            }


            float lengthMult = Math.Max(1.0f, halfBridgeLength / 5.0f);

            // Now dig out the middle.

            float bdist = MathUtils.Sqrt ((ex-sx)*(ex-sx)+(ez-sz)*(ez-sz));


			int fullcl = MathUtils.IntRange(8*halfBridgeLength,22*halfBridgeLength,rand);


            List<int> xvals = new List<int>();

            xvals.Add((int)(Math.Max(0, cx - fullcl)));
            xvals.Add((int)(Math.Min(_mapProvider.GetMap().GetHwid() - 1, cx + fullcl)));


            List<int> yvals = new List<int>();
            yvals.Add((int)(Math.Max(0, cy - fullcl)));
            yvals.Add((int)(Math.Min(_mapProvider.GetMap().GetHhgt() - 1, cy + fullcl)));

            bool nearWater = false;

            for (int x = xvals[0]; x <= xvals[1]; x++)
            {
                if (nearWater)
                {
                    break;
                }

                for (int y = 0; y < yvals.Count; y++)
                {
                    if (FlagUtils.IsSet(_md.flags[x,y],MapGenFlags.NearWater))
                    {
                        nearWater = true;
                        break;
                    }
                }
            }

            for (int y = yvals[0]; y <= yvals[1]; y++)
            {
                if (nearWater)
                {
                    break;
                }

                for (int x = 0; x < xvals.Count; x++)
                {
                    if (FlagUtils.IsSet(_md.flags[x,y], MapGenFlags.NearWater))
                    {
                        nearWater = true;
                        break;
                    }
                }
            }

            if (nearWater)
            {
                continue;
            }


            // This obtuseness stuff below is representative of 3 points:
            // start, end and the current (x,y) that's somewhere near them.
            // Those three points make a triangle, and the obtusness
            // is a mreasurement of how obtuse this triangle can be.


            float endDistScale = MathUtils.FloatRange (1.5f, 4.0f,rand);
			float edgeDistScale = MathUtils.FloatRange (3.0f,7.0f,rand);
			
			float baseObtuseness = MathUtils.FloatRange(0.95f,1.40f, rand);
			// The obtuseness allowed for the dug out area increases as we
			// move toward the edge of the region, but randomize how much
			// it can increase so some walls don't curve out so much
			// 1.0f was the original value here.
			float obtusnessIncreaseNearEdgesScale = MathUtils.FloatRange(1.2f,2.0f,rand);

			// Normally the walls stop after the obtuse triangle gets too big.
			// 0.7f was the original value here
			//float maxExtraObtusenessAllowed = MathUtils.FloatRange (0.6f,0.85f,rand);
			float maxExtraObtusenessAllowed = MathUtils.FloatRange(0.5f, 0.7f, rand);


			float holeDepthScale = MathUtils.FloatRange(1.2f, 1.6f, rand) * (float)Math.Sqrt(lengthMult);

			
			// Used for scaling how far up the ends of the roads go to make them
			// easier to walk on
			float bridgeEndRoadSmoothScale = 0.20f;
			
			// If this is a crevice bridge, use some special stats)
			if (centerPointIn != null)
			{
				fullcl = (int)(fullcl*1.4f);
				holeDepthScale *= 1.1f;

			}

            List<float[,]> noises = new List<float[,]>();

			int numNoises = 2;

			int noiseSize = fullcl * 2 + 1;
			for (int n = 0; n < numNoises; n++)
			{
                float freq = MathUtils.FloatRange(0.02f, 0.05f, rand) * noiseSize;
                float amp = MathUtils.FloatRange(0.4f, 0.8f, rand);
                float pers = MathUtils.FloatRange(0.2f, 0.5f, rand);
                int octaves = 2;

                float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), noiseSize, noiseSize);
				noises.Add(noise);
			}


			List<MyPoint> loweredPoints = new List<MyPoint>();

            ipx = (int)(px);
            ipz = (int)(pz);

            if (ipz % MapConstants.TerrainPatchSize > MapConstants.TerrainPatchSize - 1 ||
                ipx % MapConstants.TerrainPatchSize > MapConstants.TerrainPatchSize - 1)
            {
                continue;
            }


            for (int x = cx-fullcl; x <= cx+fullcl; x++)
			{
				if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
				{
					continue;
				}
				for (int z = cz-fullcl; z <= cz+fullcl; z++)
				{
					if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
					{
						continue;
					}
					float cdist = MathUtils.Sqrt ((x-cx)*(x-cx)+(z-cz)*(z-cz));
					float sdist = MathUtils.Sqrt ((x-sx)*(x-sx)+(z-sz)*(z-sz));
					float edist = MathUtils.Sqrt ((x-ex)*(x-ex)+(z-ez)*(z-ez));
					
					int ax = (int)(1.0f*x/_mapProvider.GetMap().GetHwid()*_md.awid);
					int az = (int)(1.0f*z/_mapProvider.GetMap().GetHhgt()*_md.ahgt);
                    // Set splats under the bridge to base terrain

                    // Get length of hypotenuse and legs of the b e s triangle.

                    float legSum = MathUtils.Sqrt (edist*edist+sdist*sdist);

                    bool closeToMid = false;
                    if (false && Math.Max(edist, sdist) < bridgeLength + 2.0f)
                    {
                        closeToMid = true;
                        _md.ClearAlphasAt(ax, az);
                        _md.alphas[ax, az, MapConstants.BaseTerrainIndex] = 0.99f;
                        _md.alphas[ax, az, MapConstants.RoadTerrainIndex] = 0.01f;
                    }

                    float minEndPtSize = 2.0f;
                    float maxEndPtSize = 3.5f+lengthMult-1;
                    float legSumVal = 3.3f;

                    float distDiff = Math.Abs(edist - sdist);
                    // Don't sink the road if it's near the endpoints.
                    if ((edist > bdist - minEndPtSize || sdist > bdist - minEndPtSize) && 
                        (sdist <= maxEndPtSize || edist <= maxEndPtSize))
                    {
                        if (bdist <= legSum + legSumVal && distDiff >= bdist - minEndPtSize*2)
                        {
                            float backDist = Math.Max(0, Math.Max(sdist, edist) - bdist);
                            float delta = bridgeEndRoadSmoothScale * backDist / MapConstants.MapHeight;
                            _md.heights[x, z] = MathUtils.Clamp(heightAtEnds, _md.heights[x, z],
                                                         heightAtEnds + delta);
                            if (!closeToMid)
                            {
                                //_md.ClearAlphasAt(ax, az);
                                //_md.alphas[ax, az, MapConstants.RoadTerrainIndex] = 1.0f;
                            }
                            continue;
                        }
                    }

                    float radiusPercent = cdist/fullcl;
					float obtusenessMult = baseObtuseness*(1-(0.1f*radiusPercent+
					                                          0.2f*radiusPercent*radiusPercent)
					                                       *obtusnessIncreaseNearEdgesScale);
					
					float smoothingScale = 1.0f;
					
					
					// If too far along an obtuse angle, lower areas not near the
					// road
					float ratio = 0.0f;
					
					if (sdist*sdist+bdist*bdist <= edist*edist*obtusenessMult)
					{
						ratio = (sdist*sdist+bdist*bdist)/(edist*edist);
					}
					if (edist*edist+bdist*bdist <= sdist*sdist*obtusenessMult)
					{
                        float ratio2 = (edist*edist+bdist*bdist)/(sdist*sdist);
						if (ratio2 < ratio || ratio == 0)
                        {
                            ratio = ratio2;
                        }
                    }
					
					if (ratio > 0)
					{
						float minObtuseness = obtusenessMult*maxExtraObtusenessAllowed;
						
						if (ratio < minObtuseness)
						{
							smoothingScale = 0.0f;
							continue;
						}
						else
						{
							smoothingScale = (ratio-minObtuseness)/(obtusenessMult-minObtuseness)*1.0f;
						}
						
					}


                    float distFromBridgeEnd = Math.Min(edist, sdist) * endDistScale;
					float distFromEdge = (fullcl*0.95f-cdist)/fullcl*edgeDistScale;
					float depthMult = Math.Min (distFromEdge,distFromBridgeEnd);
					if (depthMult < 0)
                    {
                        depthMult = 0;
                    }

                    int rad = 4;


					float aveSplat = 0;
                    float distToRoad = _md.roadDistances[ax, az];
					if (distToRoad < rad)
					{
						aveSplat = _md.GetAverageSplatNear(ax, az, rad, MapConstants.RoadTerrainIndex);
					}
					float currSplat = _md.alphas[ax,az,MapConstants.RoadTerrainIndex];	
					float maxRoadSplatAllowed = 0.50f;

					currSplat = 0;
					// If this cell is near another bridge, don't let it get sunk.
					if (currSplat > 0)
					{
                        if (_md.bridgeDistances[x,z] < 20)
                        {
                            continue;
                        }
					}

					// Are we too close to a straight line?
					bool almostStraightLineAlongBridge = false;
					float almostStraightScale = 1.03f;
					
					if ((edist > bdist &&
					     (bdist+sdist) <= almostStraightScale*edist) ||
					    (sdist > bdist &&
					 (bdist+edist) <= almostStraightScale*sdist))
					{
						almostStraightLineAlongBridge = true;
					}

					if (edist < bdist-1 && sdist < bdist-1)
					{
						currSplat = 0;
						aveSplat = aveSplat*aveSplat;
					}
					if (almostStraightLineAlongBridge)
					{

						depthMult = 0;
					}
					else
					{
						depthMult *= (1 - aveSplat) / (maxRoadSplatAllowed);

						// If this thing is far away from the main road, repaint the 
						// cell as base.
						if (currSplat > 0)
						{
							// If encounter a road segment not near the current road, ignore
							// it.
							float minDist = 10000;

							foreach (MyPointF rd in road)
							{
								float currDist = MathUtils.Sqrt((x - rd.X) * (x - rd.X) +
															 (z - rd.Y) * (z - rd.Y));
								if (currDist < minDist)
								{
									minDist = currDist;
								}
							}

							if (minDist >= 10)
							{
								_md.ClearAlphasAt(x, z);
								_md.alphas[x, z, MapConstants.BaseTerrainIndex] = 1;
							}
						}
					}
					
					// How far down the hole goes
					float holeDepth = depthMult/MapConstants.MapHeight*holeDepthScale*smoothingScale;
					if (holeDepth > 0)
					{
					    loweredPoints.Add (new MyPoint(ax,az));
					}
					float locy = _md.heights[x,z];
					if (locy < cyscale)
					{
						holeDepth -= (cyscale-locy)*0.95f;
						if (holeDepth < 0)
                        {
                            holeDepth = 0;
                        }
                    }

                    float noiseDepthScale = 1.0f;

					foreach (float[,] noise in noises)
					{
						noiseDepthScale *= (1 + MathUtils.Clamp(-1, noise[x - (cx - fullcl), z - (cz - fullcl)], 1));
					}

					holeDepth *= noiseDepthScale;

					_md.heights[x,z] -= holeDepth;

					if (cdist < halfBridgeLength+2 && distFromBridgeEnd > 2)
					{
                        float currRoad = _md.alphas[ax, az, MapConstants.RoadTerrainIndex];
						_md.alphas[ax, az, MapConstants.RoadTerrainIndex] = 0;
						_md.alphas[ax, az, MapConstants.BaseTerrainIndex] += currRoad / 2;
						_md.alphas[ax, az, MapConstants.DirtTerrainIndex] += currRoad / 2;
					}
					
				}
			}


            if (ipx >= 0 && ipz >= 0 && ipx < _mapProvider.GetMap().GetHwid() - 1 && ipz < _mapProvider.GetMap().GetHhgt() - 1)
            {


                _md.mapObjects[ipx, ipz] = MapConstants.BridgeObjectOffset + (int)(bt.IdKey);
                int startval = _md.mapObjects[ipx, ipz];
                float clampedAngle = angle;
                while (clampedAngle < 0)
                {
                    clampedAngle += MapConstants.BridgeMaxAngle;
                }

                while (clampedAngle >= MapConstants.BridgeMaxAngle)
                {
                    clampedAngle -= MapConstants.BridgeMaxAngle;
                }

                ushort nextVal = (byte)((clampedAngle / MapConstants.BridgeAngleDiv) % MapConstants.BridgeAngleMod);

                ushort nextval1 = nextVal;

                ushort heightDiff = (ushort)(Math.Max(0, bridgeHeight - MapConstants.MinLandHeight));

                ushort shiftedHeight = (ushort)(heightDiff << MapConstants.BridgeHeightBitShift);

                nextVal += shiftedHeight;

                int shiftNext = (nextVal << 16);

                _md.mapObjects[ipx, ipz] += shiftNext;
                int currVal = _md.mapObjects[ipx, ipz];


                int maxDelta = 3;
                int midLen = (int)(bridgeLength * 1.5);


                for (int poolTries = 0; poolTries < 10; poolTries++)
                {



                    int poolx = ipx + MathUtils.IntRange(-maxDelta, maxDelta, rand);
                    int poolz = ipz + MathUtils.IntRange(-maxDelta, maxDelta, rand);

                    WaterGenData wgd = new WaterGenData()
                    {
                        x = poolx,
                        z = poolz,
                        maxHeight = bridgeHeight,
                    };

                    if (poolx != ipx || poolz != ipz)
                    {

                        _waterGenData.Add(wgd);
                        break;
                    }
                }



                _md.mapObjects[ipx, ipz] = currVal;
                _md.currBridges.Add(new MyPointF(ipx, ipz));
                SetBridgeDistancesNear((int)(ipx), (int)(ipz));

            }


            break;
		}
		
	}



    public BridgeType GetRandomBridgeType(long zoneTypeId, MyRandom rand)
    {
        ZoneType zt = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);
        if (zt == null || zt.BridgeTypes == null || zt.BridgeTypes.Count < 1)
        {
            return null;
        }

        int totalChance = 0;
        int chanceChosen = 0;


        for (int times = 0; times < 2; times++)
        {
            for (int b = 0; b < zt.BridgeTypes.Count; b++)
            {
                ZoneBridgeType zbt = zt.BridgeTypes[b];
                BridgeType bt = _gameData.Get<BridgeTypeSettings>(_gs.ch).Get(zbt.BridgeTypeId);
                if (bt != null && !string.IsNullOrEmpty(bt.Art))
                {
                    if (times == 0)
                    {
                        totalChance += zbt.Chance;
                    }
                    else
                    {
                        chanceChosen -= zbt.Chance;
                        if (chanceChosen <= 0)
                        {
                            return bt;
                        }
                    }
                }
            }

            if (times == 0)
            {
                if (totalChance < 1)
                {
                    return null;
                }
                chanceChosen = rand.Next() % totalChance;
            }
            else
            {
                break;
            }

        }

        return null;

    }

    protected void SetBridgeDistancesNear (int cx, int cy)
    {
        if (_md.bridgeDistances == null)
        {
            return;
        }

        int bridgeRadius = MapConstants.MaxBridgeCheckDistance;
        for (int xx = cx - bridgeRadius; xx <= cx + bridgeRadius; xx++)
        {
            if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }
            float dbx = cx - xx;

            for (int yy = cy - bridgeRadius; yy <= cy + bridgeRadius; yy++)
            {
                if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }
                float dby = yy - cy;
                float dist = (float)Math.Sqrt(dbx * dbx + dby * dby);
                if (dist < bridgeRadius)
                {
                    if (dist < _md.bridgeDistances[xx,yy])
                    {
                        _md.bridgeDistances[xx,yy] = (ushort)dist;
                    }
                }
            }
        }
    }
}

