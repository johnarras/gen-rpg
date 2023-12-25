using System;
using System.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.WebRequests.Utils;
using Genrpg.Shared.Core.Entities;
using System.IO.Compression;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Pathfinding.Services
{

    public interface IPathfindingService : IService
    {
        Task LoadPathfinding(GameState gs, string urlPrefix);
        bool[,] ConvertBytesToGrid(GameState gs, byte[] bytes);
        byte[] ConvertGridToBytes(GameState gs, bool[,] grid);
    }

    public class PathfindingService : IPathfindingService
    {
        public async Task LoadPathfinding(GameState gs, string urlPrefix)
        {
            if (gs.pathfinding != null || gs.map == null)
            {
                return;
            }

            try
            {
                string filename = MapUtils.GetMapObjectFilename(gs, PathfindingConstants.Filename, gs.map.Id, gs.map.MapVersion);

                string fullUrl = urlPrefix + filename;

                byte[] compressedBytes = await WebRequestUtils.DownloadBytes(fullUrl);

                byte[] decompressedBytes = CompressionUtils.DecompressBytes(compressedBytes);

                gs.pathfinding = ConvertBytesToGrid(gs, decompressedBytes);
            }
            catch (Exception e)
            {
                gs.logger.Exception(e, "LoadPathfinding");
            }
        }


        public bool[,] ConvertBytesToGrid(GameState gs, byte[] bytes)
        {


            int xsize = gs.map.GetHwid() / PathfindingConstants.BlockSize;
            int zsize = gs.map.GetHhgt() / PathfindingConstants.BlockSize;
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
                gs.logger.Exception(e, "Patfinding Bytes to Grid");
            }
            return grid;
        }

        public byte[] ConvertGridToBytes(GameState gs, bool[,] grid)
        {

            int xsize = gs.map.GetHwid() / PathfindingConstants.BlockSize;
            int zsize = gs.map.GetHhgt() / PathfindingConstants.BlockSize;
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
    }
}
