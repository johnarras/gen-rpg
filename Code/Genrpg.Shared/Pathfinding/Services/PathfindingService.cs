using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.WebRequests.Utils;
using Genrpg.Shared.Assets.Utils;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.Shared.Pathfinding.Services
{

    public interface IPathfindingService : IService
    {
        Task LoadPathfinding(GameState gs);
        bool[,] ConvertBytesToGrid(GameState gs, byte[] bytes);
        byte[] ConvertGridToBytes(GameState gs, bool[,] grid);
    }

    public class PathfindingService : IPathfindingService
    {
        public async Task LoadPathfinding(GameState gs)
        {
            if (gs.pathfinding != null || gs.map == null)
            {
                return;
            }

            try
            {
                string filename = MapUtils.GetMapObjectFilename(gs, PathfindingConstants.Filename, gs.map.Id, gs.map.MapVersion);

                string url = AssetUtils.GetArtURLPrefix(gs);

                string fullUrl = url + filename;

                byte[] bytes = await WebRequestUtils.DownloadBytes(gs, fullUrl);

                gs.pathfinding = ConvertBytesToGrid(gs, bytes);
            }
            catch (Exception e)
            {
                gs.logger.Exception(e, "LoadPathfinding");
            }
        }


        public bool[,] ConvertBytesToGrid(GameState gs, byte[] bytes)
        {

            bool[,] grid = new bool[gs.map.GetHwid(), gs.map.GetHhgt()];

            int x = 0;
            int y = 0;

            try
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    byte currByte = bytes[i];

                    for (int b = 0; b < 8; b++)
                    {
                        if ((currByte & 1 << 7 - b) != 0)
                        {
                            grid[x, y] = true;
                        }
                        x++;
                        if (x >= grid.GetLength(0))
                        {
                            x = 0;
                            y++;
                        }
                        if (x >= grid.GetLength(0) || y >= grid.GetLength(1))
                        {
                            break;
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

            int size = (int)Math.Ceiling(gs.map.GetHwid() * gs.map.GetHhgt() / 8.0);

            byte[] output = new byte[size];

            int dx = 0;
            int dy = 0;

            int offset = 0;
            for (offset = 0; offset < size; offset++)
            {
                if (dx >= gs.map.GetHwid() || dy >= gs.map.GetHhgt())
                {
                    break;
                }
                byte newByte = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    if (grid[dx, dy])
                    {
                        newByte |= (byte)(1 << 7 - bit);
                    }
                    dx++;
                    if (dx >= gs.map.GetHwid())
                    {
                        dx = 0;
                        dy++;
                    }
                    if (dx >= gs.map.GetHwid() || dy >= gs.map.GetHhgt())
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
