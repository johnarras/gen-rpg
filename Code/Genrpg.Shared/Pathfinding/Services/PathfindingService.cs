using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.Pathfinding.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.WebRequests.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Pathfinding.Services
{

    public interface IPathfindingService : IInitializable
    {
        Task LoadPathfinding(string urlPrefix);
        bool[,] ConvertBytesToGrid(byte[] bytes);
        byte[] ConvertGridToBytes(bool[,] grid);
        /// <summary>
        /// Update path. This uses a callback so that it can be put onto a different server eventually.
        /// </summary>
        void UpdatePath(Unit tracker, IRandom rand, int endx, int endz, Action<IRandom, Unit> callback, bool showFullLine = false);
        void CalcPath(Unit tracker, IRandom rand, int worldStartX, int worldStartZ, int worldEndX, int worldEndZ, bool showFullLine = false);
        bool CellIsBlocked(int x, int z);
        long GetPathSearchCount();
        void SetPathfinding(bool[,] grid);
        bool[,] GetPathfinding();
    }

    public class PathfindingService : IPathfindingService
    {

        ConcurrentQueue<PathWorkbook> _workbookCache = new ConcurrentQueue<PathWorkbook>();
        private ILogService _logService = null;
        private IMapProvider _mapProvider = null;

        private bool[,] _grid = null;
        private long _pathSearchCount = 0;
        public long GetPathSearchCount()
        {
            return _pathSearchCount;
        }

        public async Task Initialize( CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void SetPathfinding(bool[,] grid)
        {
            _grid = grid;
        }

        public bool[,] GetPathfinding()
        {
            return _grid;
        }

        public async Task LoadPathfinding(string urlPrefix)
        {
            if (_grid != null || _mapProvider.GetMap() == null)
            {
                return;
            }

            try
            {
                string filename = MapUtils.GetMapObjectFilename(PathfindingConstants.Filename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);

                string fullUrl = urlPrefix + filename;

                byte[] compressedBytes = await WebRequestUtils.DownloadBytes(fullUrl);

                byte[] decompressedBytes = CompressionUtils.DecompressBytes(compressedBytes);

                SetPathfinding(ConvertBytesToGrid(decompressedBytes));
            }
            catch (Exception e)
            {
                _logService.Exception(e, "LoadPathfinding");
            }
        }


        public bool[,] ConvertBytesToGrid(byte[] bytes)
        {
            int xsize = _mapProvider.GetMap().GetHwid() / PathfindingConstants.BlockSize;
            int zsize = _mapProvider.GetMap().GetHhgt() / PathfindingConstants.BlockSize;
            bool[,] grid = new bool[xsize,zsize];

            int x = 0;
            int y = 0;

            try
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    byte currByte = bytes[i];

                    for (int b = 0; b < 8; b++)
                    {
                        if (x >= grid.GetLength(0) || y >= grid.GetLength(1))
                        {
                            break;
                        }
                        if ((currByte & 1 << (7 - b)) != 0)
                        {
                            grid[x, y] = true;
                        }
                        x++;
                        if (x >= grid.GetLength(0))
                        {
                            x = 0;
                            y++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "Patfinding Bytes to Grid");
            }
            return grid;
        }

        public byte[] ConvertGridToBytes(bool[,] grid)
        {
            int xsize = _mapProvider.GetMap().GetHwid() / PathfindingConstants.BlockSize;
            int zsize = _mapProvider.GetMap().GetHhgt() / PathfindingConstants.BlockSize;
            int size = (int)Math.Ceiling(xsize*zsize / 8.0);

            byte[] output = new byte[size];

            int dx = 0;
            int dz = 0;

            int offset = 0;
            for (offset = 0; offset < size; offset++)
            {
                if (dx >= xsize || dz >= zsize)
                {
                    break;
                }
                byte newByte = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    if (grid[dx, dz])
                    {                       
                        newByte |= (byte)(1 << (7 - bit));
                    }
                    dx++;
                    if (dx >= xsize)
                    {
                        dx = 0;
                        dz++;
                    }
                    if (dx >= xsize|| dz >= zsize)
                    {
                        break;
                    }
                }
                output[offset] = newByte;
            }

            return output;
        }


        class PathCell
        {
            public int X;
            public int Z;
            public float Cost;
            public float MoveCost;
            public PathCell CameFrom;
            public bool IsClosed;
            public bool IsOpen;

            public void Clear()
            {
                Cost = -1;
                MoveCost = 0;
                CameFrom = null;
                X = -1;
                Z = -1;
                IsClosed = false;
                IsOpen = false;
            }
        }

        const int GridSize = 64;
        const int GridCenter = GridSize / 2;
        const int CellCacheInitialLength = 128;
        const int OpenSetInitialLength = 128;

        const float DistanceCostScale = 3.0f;
        const float BaseMoveCost = 1.0f;
        const float TurnCostScale = 0.5f;

        class PathWorkbook
        {
            public int CenterX { get; set; }
            public int CenterZ { get; set; }
            public int MinX { get; set; }
            public int MaxX { get; set; }
            public int MinZ { get; set; }
            public int MaxZ { get; set; }

            public int MaxOpenCellX { get; set; } = GridCenter;
            public int MinOpenCellX { get; set; } = GridCenter;
            public int MaxOpenCellZ { get; set; } = GridCenter;
            public int MinOpenCellZ { get; set; } = GridCenter;

            public PathCell[,] Cells { get; set; } = new PathCell[GridSize, GridSize];


            public List<PathCell>[] OpenSet { get; set; }
            public int OpenSetMinIndex { get; set; }
            public int OpenSetMaxIndex { get; set; }

            public List<PathCell> CameFrom { get; set; } = new List<PathCell>();
            public List<PathCell> LineCellCache { get;set; } = new List<PathCell>();


            public int MaxBlockedCellsInARow = 0;
            public int CurrentBlockedCellsCount = 0;
            public bool DidFindBlockedCell = false;

            public List<PathCell> BlockedCells { get; set; } = new List<PathCell>();

            public PathWorkbook()
            {
                OpenSet = new List<PathCell>[OpenSetInitialLength];
                for (int i = 0; i < OpenSet.Length; i++)
                {
                    OpenSet[i] = new List<PathCell>();
                }

                for (int x =0; x < GridSize; x++)
                {
                    for (int y =0; y < GridSize; y++)
                    {
                        Cells[x,y] = new PathCell();
                    }
                }
                for (int i = 0; i< GridSize*4; i++)
                {
                    LineCellCache.Add(new PathCell());
                }

                Clear();
            }

            public void Clear()
            {
                CameFrom.Clear();

                for (int i = 0; i < OpenSet.Length; i++)
                {
                    OpenSet[i].Clear();
                }
                OpenSetMinIndex = 0;
                OpenSetMaxIndex = 0;

                BlockedCells.Clear();
                for (int x = MinOpenCellX; x <= MaxOpenCellX; x++)
                {
                    for (int z = MinOpenCellZ; z <= MaxOpenCellZ; z++)  
                    {
                        Cells[x, z].Clear();
                    }
                }
                MinOpenCellX = GridCenter;
                MaxOpenCellX = GridCenter;
                MinOpenCellZ = GridCenter;
                MaxOpenCellZ = GridCenter;
            }

            public PathCell GetNextOpenCell()
            {
                if (OpenSetMinIndex >= OpenSet.Length)
                {
                    return null;
                }

                for (int i = Math.Max(0, OpenSetMinIndex-3); i <= OpenSetMaxIndex; i++)
                {
                    if (OpenSet[i].Count > 0)
                    {
                        OpenSetMinIndex = i;
                        break;
                    }
                }

                if (OpenSet[OpenSetMinIndex].Count < 1)
                {
                    return null;
                }

                PathCell cell = OpenSet[OpenSetMinIndex][0];
                OpenSet[OpenSetMinIndex].RemoveAt(0);

                return cell;
            }

            public void RemoveOpenCell (PathCell cell)
            {
                if (!cell.IsOpen)
                {
                    return;
                }
                int index = MathUtils.Clamp(0, (int)cell.Cost, OpenSet.Length - 1);

                if (index >= 0 && index < OpenSet.Length)
                {
                    OpenSet[index].Remove(cell);
                }
            }

            public void AddOpenCell(PathCell cell, PathCell fromCell)
            {
                if (cell.IsClosed || cell.IsOpen)
                {
                    return;
                }

                int gx = cell.X - MinX;
                int gz = cell.Z - MinZ;

                MinOpenCellX = Math.Min(gx, MinOpenCellX);
                MaxOpenCellX = Math.Max(gx, MaxOpenCellX);
                MinOpenCellZ = Math.Min(gz, MinOpenCellZ);
                MaxOpenCellZ = Math.Max(gz, MaxOpenCellZ);

                int index = MathUtils.Clamp(0, (int)cell.Cost, OpenSet.Length-1);

                List<PathCell> openList = OpenSet[index];
                cell.IsOpen = true;
                cell.CameFrom = fromCell;
                if (fromCell != null)
                {
                    cell.MoveCost = fromCell.MoveCost + BaseMoveCost;
                }
                else
                {
                    cell.MoveCost = 0;
                }

                if (openList.Count < 1)
                {
                    openList.Add(cell);
                }
                else
                {
                    bool didInsert = false;
                    for (int c = 0; c < openList.Count; c++)
                    {
                        if (cell.Cost < openList[c].Cost)
                        {
                            openList.Insert(c, cell);
                            didInsert = true;
                            break;
                        }
                    }
                    if (!didInsert)
                    {
                        openList.Add(cell);
                    }
                }

                if (index > OpenSetMaxIndex)
                {
                    OpenSetMaxIndex = index;
                }              
            }
        }

        private PathWorkbook CheckoutWorkbook(IRandom rand)
        {
            if (_workbookCache.TryDequeue(out PathWorkbook workbook))
            {
                workbook.Clear();
                return workbook;
            }
           return new PathWorkbook();
        }

        private void ReturnWorkbook(IRandom rand, PathWorkbook workbook)
        {
            _workbookCache.Enqueue(workbook);
        }

        public void CalcPath(Unit tracker, IRandom rand, int worldStartX, int worldStartZ, int worldEndX, int worldEndZ, bool showFullLine = false)
        {
            tracker.Waypoints.Clear();
            if (worldEndX < 0 || worldEndZ < 0)
            {
                return;
            }
            if (_grid == null)
            {
                tracker.Waypoints.RetvalType = "No pathfinding data";
                return;
            }
            _pathSearchCount++;

            PathWorkbook workbook = CheckoutWorkbook(rand);
          
            // Map to pathfinding grid values.
            int startGridX = (worldStartX + PathfindingConstants.BlockSize / 2) / PathfindingConstants.BlockSize;
            int startGridZ = (worldStartZ + PathfindingConstants.BlockSize / 2) / PathfindingConstants.BlockSize;

            int endGridX = (worldEndX + PathfindingConstants.BlockSize / 2) / PathfindingConstants.BlockSize;
            int endGridZ = (worldEndZ + PathfindingConstants.BlockSize / 2) / PathfindingConstants.BlockSize;

            if (startGridX == endGridX && startGridZ == endGridZ)
            {
                tracker.Waypoints.AddGridCell(endGridX, endGridZ);
                ReturnWorkbook(rand, workbook);
                tracker.Waypoints.RetvalType = "Start Is End";
                return;
            }

            int lineLength = GetLineLength(workbook, startGridX, startGridZ, endGridX, endGridZ);

            if (lineLength <= 0 || lineLength > workbook.LineCellCache.Count)
            {
                ReturnWorkbook(rand, workbook);
                tracker.Waypoints.RetvalType = "No direct path found";
                return;
            }


            workbook.DidFindBlockedCell = false;
            workbook.MaxBlockedCellsInARow = 0;
            workbook.CurrentBlockedCellsCount = 0;
            workbook.BlockedCells.Clear();
            for (int i = 1; i < workbook.LineCellCache.Count && i < lineLength; i++)
            {
                PathCell lineCell = workbook.LineCellCache[i];

                if (lineCell.X < 1 || lineCell.X >= _grid.GetLength(0)-1 ||
                    lineCell.Z < 1 || lineCell.Z >= _grid.GetLength(1)-1)
                {
                    workbook.DidFindBlockedCell = true;
                    workbook.MaxBlockedCellsInARow = 100;
                    break;
                }

                if (_grid[lineCell.X,lineCell.Z] == true)
                {
                    workbook.CurrentBlockedCellsCount++;
                    workbook.DidFindBlockedCell = true;
                    if (workbook.CurrentBlockedCellsCount > workbook.MaxBlockedCellsInARow)
                    {
                        workbook.MaxBlockedCellsInARow = workbook.CurrentBlockedCellsCount;
                    }
                    workbook.BlockedCells.Add(lineCell);
                }
                else
                {
                    workbook.CurrentBlockedCellsCount = 0;
                }
            }

            if (!workbook.DidFindBlockedCell)
            {
                if (showFullLine)
                {
                    for (int i = 1; i < lineLength; i++)
                    {
                        tracker.Waypoints.AddGridCell(workbook.LineCellCache[i].X, workbook.LineCellCache[i].Z);
                    }
                }
                else
                {
                    tracker.Waypoints.AddGridCell(endGridX, endGridZ); 
                }
                ReturnWorkbook(rand, workbook);
                tracker.Waypoints.RetvalType = "Straight Path";
                return;
            }

            int dx = Math.Abs(endGridX - startGridX);
            int dz = Math.Abs(endGridZ - startGridZ);

            if (workbook.MaxBlockedCellsInARow == 1)
            {
                bool xislonger = Math.Abs(startGridX - endGridX) >= Math.Abs(startGridZ - endGridZ);

                int ddx = 0;
                int ddz = 0;

                if (dx >= dz * 2 / 3)
                {
                    ddz = 1;
                }
                else if (dz >= dx * 2 / 3)
                {
                    ddx = 1;
                }
                else
                {
                    ddx = 1;
                    ddz = 1;
                }

                bool allAdjacentCellsClear = true;
                for (int b = 0; b < workbook.BlockedCells.Count; b++)
                {
                    PathCell bc = workbook.BlockedCells[b];

                    for (int xx = bc.X-1; xx <= bc.X+1; xx++)
                    {
                        for (int zz = bc.Z- 1; zz <= bc.Z+1; zz++)
                        {
                            if (_grid[xx,zz])
                            {
                                allAdjacentCellsClear = false;
                                break;
                            }
                        }

                        if (!allAdjacentCellsClear)
                        {
                            break;
                        }
                    }

                    if (!allAdjacentCellsClear)
                    {
                        break;
                    }
                }

                if (allAdjacentCellsClear)
                {
                    for (int b = 0; b < workbook.BlockedCells.Count; b++)
                    {
                        PathCell bc = workbook.BlockedCells[b];

                        if ((bc.X+bc.Z)% 2 == 0)
                        {
                            tracker.Waypoints.AddGridCell(bc.X + ddx, bc.Z + ddz);
                                
                        }
                        else
                        {
                            tracker.Waypoints.AddGridCell(bc.X - ddx, bc.Z - ddz);
                        }
                    }
                    tracker.Waypoints.AddGridCell(endGridX, endGridZ);
                    ReturnWorkbook(rand, workbook);
                    tracker.Waypoints.RetvalType = "Small Bad Points";
                    return;
                }
            }


            // More than 1 cell in a row blocked, do real A*

            PathCell startCell = workbook.Cells[GridCenter,GridCenter];
            startCell.X = startGridX;
            startCell.Z = startGridZ;
            startCell.Cost = CalcCost(startCell, null, endGridX, endGridZ);

            workbook.CenterX = startGridX;
            workbook.CenterZ = startGridZ;
            workbook.MinX = MathUtils.Clamp(0, workbook.CenterX - GridCenter, _grid.GetLength(0) - 1);
            workbook.MaxX = MathUtils.Clamp(0, workbook.CenterX + GridCenter - 1, _grid.GetLength(0) - 1);
            workbook.MinZ = MathUtils.Clamp(0, workbook.CenterZ - GridCenter, _grid.GetLength(1) - 1);
            workbook.MaxZ = MathUtils.Clamp(0, workbook.CenterZ + GridCenter - 1, _grid.GetLength(1) - 1);

            workbook.AddOpenCell(startCell, null);

            int openCellIteration = 0;
            while (true)
            {
                openCellIteration++;
                PathCell activeCell = workbook.GetNextOpenCell();
                if (activeCell == null)
                {
                    ReturnWorkbook(rand, workbook);
                    tracker.Waypoints.RetvalType = "No Open Cells Left " + openCellIteration;
                    return;
                }
                activeCell.IsClosed = true;

                if (activeCell.X == endGridX && activeCell.Z == endGridZ)
                {
                    tracker.Waypoints.AddGridCell(endGridX, endGridZ);

                    PathCell prevCell = activeCell;
                    while (prevCell != null)
                    {
                        prevCell = prevCell.CameFrom;
                        if (prevCell != null)
                        {
                            Waypoint currentWaypoint = tracker.Waypoints.Waypoints.FirstOrDefault(x => x.X == prevCell.X && x.Z == prevCell.Z);

                            if (currentWaypoint != null)
                            {
                                _logService.Info("Added dupe cell to waypoint list!");
                                break;
                            }
                            else
                            {
                                tracker.Waypoints.AddGridCell(prevCell.X, prevCell.Z);
                            }
                        }
                    }

                    tracker.Waypoints.Waypoints.Reverse();
                    ReturnWorkbook(rand, workbook);
                    tracker.Waypoints.RetvalType = "ASTAR SUCCESS";
                    return;
                }

                for (int xx = activeCell.X-1; xx <= activeCell.X+1; xx++)
                {
                    if (xx < workbook.MinX || xx > workbook.MaxX)
                    {
                        continue;
                    }

                    for (int zz = activeCell.Z - 1; zz <= activeCell.Z + 1; zz++)
                    {
                        if (zz < workbook.MinZ || zz > workbook.MaxZ)
                        {
                            continue;
                        }

                        if (_grid[xx,zz]) // Blocked anyway.
                        {
                            continue;
                        }

                        int gx = xx - workbook.MinX;
                        int gz = zz - workbook.MinZ;

                        PathCell currentCellInSpot = workbook.Cells[gx, gz];
                       
                        if (currentCellInSpot.IsClosed)
                        {
                            continue;
                        }
                        currentCellInSpot.X = xx;
                        currentCellInSpot.Z = zz;

                        float oldCost = currentCellInSpot.Cost;
                        float newCost = CalcCost(currentCellInSpot, activeCell, endGridX, endGridZ);

                        if (oldCost != newCost)
                        {
                            currentCellInSpot.Cost = newCost;
                            workbook.RemoveOpenCell(currentCellInSpot);
                            workbook.AddOpenCell(currentCellInSpot, activeCell);
                        }
                    }
                }
            }
        }
       
        private float CalcCost(PathCell cell, PathCell fromCell, int endx, int endz)
        {
            float distCost = DistanceCostScale * (float)Math.Sqrt((cell.X - endx) * (cell.X - endx) + (cell.Z - endz) * (cell.Z - endz));

            float moveCost = BaseMoveCost * (fromCell != null ? fromCell.MoveCost : 0);

            return distCost + moveCost;
        }


        /// <summary>
        /// Bressenham's
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="startx"></param>
        /// <param name="startz"></param>
        /// <param name="endx"></param>
        /// <param name="endz"></param>
        /// <returns></returns>
        private int GetLineLength(PathWorkbook workbook, int startx, int startz, int endx, int endz)
        {


           
            List<PathCell> retval = new List<PathCell>();

            int cx = startx;
            int cz = startz;
            int dx = Math.Abs(endx - startx);
            int sx = startx < endx ? 1 : -1;
            int dz = -Math.Abs(endz - startz);
            int sz = startz < endz ? 1 : -1;
            int error = dx + dz;
            int error2 = 2 * error;

            int lineCellIndex = 0;
            PathCell cell = workbook.LineCellCache[lineCellIndex++];
            cell.X = cx;
            cell.Z = cz;
            retval.Add(cell);

            while (true)
            {

                try
                {

                    if (cx == endx && cz == endz)
                    {
                        break;
                    }

                    error2 = 2 * error;

                    if (error2 >= dz)
                    {
                        if (lineCellIndex >= workbook.LineCellCache.Count)
                        {
                            break;
                        }
                        if (cx == endx)
                        {
                            break;
                        }

                        error = error + dz;

                        cx = cx + sx;
                        cell = workbook.LineCellCache[lineCellIndex++];
                        cell.X = cx;
                        cell.Z = cz;
                        retval.Add(cell);
                    }
                    if (error2 <= dx)
                    {
                        if (lineCellIndex >= workbook.LineCellCache.Count)
                        {
                            break;
                        }
                        if (cz == endz)
                        {
                            break;
                        }
                        error = error + dx;
                        cz = cz + sz;
                        cell = workbook.LineCellCache[lineCellIndex++];
                        cell.X = cx;
                        cell.Z = cz;
                        retval.Add(cell);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Exception(ex, "Pathfinding.GetLine");
                }
            }

            return lineCellIndex;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="tracker"></param>
        /// <param name="endx"></param>
        /// <param name="endz"></param>
        /// <returns>true if the path was altered, false if not</returns>
        public void UpdatePath(Unit tracker, IRandom rand, int endx, int endz, Action<IRandom,Unit> callback, bool showFullLine = false)
        {
            if (endx < 0 || endz < 0)
            {
                return;
            }
            if (tracker.Waypoints != null && tracker.Waypoints.Waypoints.Count > 0)
            {
                Waypoint lastWaypoint = tracker.Waypoints.Waypoints.Last();

                int lastGridX = lastWaypoint.X / PathfindingConstants.BlockSize;
                int lastGridZ = lastWaypoint.Z / PathfindingConstants.BlockSize;

                int newGridX = endx / PathfindingConstants.BlockSize;
                int newGridZ = endz / PathfindingConstants.BlockSize;

                if (lastGridX == newGridX && lastGridZ == newGridZ)
                {
                    return;
                }

                // Only went one cell, incrementally update the path
                if (Math.Abs(lastGridX-newGridX) <= 1 &&
                    Math.Abs(lastGridZ-newGridZ) <= 1 &&
                    newGridX >= 0 && newGridZ >= 0 &&
                    newGridX < _grid.GetLength(0) &&
                    newGridZ < _grid.GetLength(1) &&
                    !_grid[newGridX,newGridZ])
                {
                    int oldWaypointIndex = -1;
                    for (int w = tracker.Waypoints.Waypoints.Count-1; w >=0; w--)
                    {
                        Waypoint wp = tracker.Waypoints.Waypoints[w];
                        if (wp.X/PathfindingConstants.BlockSize == newGridX &&
                            wp.Z/PathfindingConstants.BlockSize == newGridZ)
                        {
                            oldWaypointIndex = w;
                            break;
                        }
                    }

                    if (oldWaypointIndex >= 0)
                    {                      
                        while (tracker.Waypoints.Waypoints.Count > oldWaypointIndex)
                        {
                            tracker.Waypoints.RemoveWaypointAt(tracker.Waypoints.Waypoints.Count - 1);
                        }
                        return;
                    }
                    else
                    {
                        tracker.Waypoints.AddGridCell(newGridX, newGridZ);
                        return;
                    }
                }
            }

            CalcPath(tracker, rand, (int)tracker.X, (int)tracker.Z, endx, endz);
            callback(rand, tracker);
            return;
        }

        public bool CellIsBlocked(int x, int z)
        {
            if (_grid == null || 
                x < 0 || x >= _grid.GetLength(0) ||
                z < 0 || z >= _grid.GetLength(1) ||
                _grid[x,z] == true)
            {
                return true;
            }
            return false;
        }
    }
}
