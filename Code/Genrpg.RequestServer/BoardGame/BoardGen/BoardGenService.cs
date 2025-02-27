using Genrpg.RequestServer.BoardGame.BoardModeHelpers;
using Genrpg.RequestServer.BoardGame.Services;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Settings;
using Google.Apis.AndroidPublisher.v3.Data;
using System;

namespace Genrpg.RequestServer.BoardGame.BoardGen
{
    public interface IBoardGenService : IInjectable
    {
        Task<BoardData> GenerateBoard(WebContext context, BoardGenArgs genData = null);
        Task UpdateTiles(WebContext context, BoardData boardData, int startIndex, int length);
    }
    public class BoardGenService : IBoardGenService
    {
        protected IBoardService _boardService = null!;
        protected IGameData _gameData = null!;
        private ILogService _logService = null!;
        private IBoardModeService _boardModeService = null!;

        public Type GetKey() { return GetType(); }

        public async Task<BoardData> GenerateBoard(WebContext context, BoardGenArgs args = null)
        {
            if (args == null)
            {
                // These are reasonable defaults for making a regular board for this user.
                args = new BoardGenArgs()
                {
                    BoardModeId = BoardModes.Primary
                };
            }

            BoardData boardData = await context.GetAsync<BoardData>();


            CoreUserData userData = await context.GetAsync<CoreUserData>();

            BoardGenSettings genSettings = _gameData.Get<BoardGenSettings>(context.user);
            
            boardData.Clear();
            boardData.OwnerId = args.OwnerId;
            boardData.Seed = args.Seed;
            boardData.BoardModeId = args.BoardModeId;
            boardData.Width = BoardGameConstants.MapWidth;
            boardData.Height = BoardGameConstants.MapHeight;


            if (string.IsNullOrEmpty(boardData.OwnerId))
            {
                boardData.OwnerId = context.user.Id;
            }
            int[,] grid = new int[boardData.Width, boardData.Height];


            int xc = boardData.Width / 2;
            int zc = boardData.Height / 2;

            float xrad = boardData.Width / 2 - MathUtils.FloatRange(genSettings.EdgeMinGap, genSettings.EdgeMaxGap, context.rand);
            float zrad = boardData.Height / 2 - MathUtils.FloatRange(genSettings.EdgeMinGap, genSettings.EdgeMaxGap, context.rand);

            float xslope = MathUtils.FloatRange(-genSettings.SlopeMax,genSettings.SlopeMax, context.rand);

            int pointCount = MathUtils.IntRange(genSettings.MinPointCount, genSettings.MaxPointCount, context.rand);
            float angleGap = 360 / pointCount;
            float angleDelta = 0.0f;
            float startAngle = MathUtils.FloatRange(0, 360, context.rand);
            float angleRange = angleGap * angleDelta;

            List<MyPoint> startPoints = new List<MyPoint>();

            for (int i = 0; i < pointCount; i++)
            {
                float angle = startAngle + i * 360 / pointCount;

                float currAngle = angle + MathUtils.FloatRange(-angleRange, angleRange, context.rand);

                float radMultX = MathUtils.FloatRange(1 - genSettings.RadDelta, 1 + genSettings.RadDelta, context.rand);
                float radMultZ = MathUtils.FloatRange(1 - genSettings.RadDelta, 1 + genSettings.RadDelta, context.rand);

                float ax = (float)Math.Cos(currAngle * Math.PI / 180);
                float az = (float)Math.Sin(currAngle * Math.PI / 180);

                float x = xc + ax * xrad * radMultX;
                float z = zc + az * zrad * radMultZ;

                z += (xc - x) * xslope;

                int fx = (int)(MathUtils.Clamp(1, x, boardData.Width - 2));
                int fz = (int)(MathUtils.Clamp(1, z, boardData.Height - 2));

                startPoints.Add(new MyPoint(fx, fz));

            }

            if (context.rand.NextDouble() < 0.5f)
            {
                startPoints.Reverse();
            }

            List<PointXZ> finalPoints = new List<PointXZ>();

            for (int p = 0; p < startPoints.Count; p++)
            {
                MyPoint start = startPoints[p];
                MyPoint end = startPoints[(p + 1) % startPoints.Count];

                int dsx = start.X - xc;
                int dsz = start.Y - zc;
                int dex = end.X - xc;
                int dez = end.Y - zc;

                float dist1 = dsx * dsx + dez * dez;
                float dist2 = dex * dex + dsz * dsz;


                bool xFirst = dist1 < dist2;

                if (context.rand.NextDouble() < 0.50f)
                {
                    xFirst = !xFirst;
                }

                int times = 0;
                if (xFirst)
                {
                    int xdelta = Math.Sign(end.X - start.X);
                    for (int x = start.X; x != end.X; x += xdelta)
                    {

                        if (++times >= 1000)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(x, start.Y, true);
                    }
                    int ydelta = Math.Sign(end.Y - start.Y);
                    for (int y = start.Y; y != end.Y; y += ydelta)
                    {
                        if (++times >= 100)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(end.X,y,true);
                    }
                }
                else
                {
                    int ydelta = Math.Sign(end.Y - start.Y);
                    for (int y = start.Y; y != end.Y; y += ydelta)
                    {
                        if (++times >= 100)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(start.X, y, true);
                    }

                    int xdelta = Math.Sign(end.X - start.X);
                    for (int x = start.X; x != end.X; x += xdelta)
                    {
                        if (++times >= 100)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(x, end.Y, true);
                    }
                }
            }


            int pathLength = 0;
            for (int x = 0; x < boardData.Width; x++)
            {
                for (int z = 0; z < boardData.Height; z++)
                {
                    if (boardData.IsOnPath(x,z))
                    {
                        pathLength++;
                    }
                }
            }

            boardData.Length = pathLength;

            await UpdateTiles(context, boardData, 0, boardData.Length);

            if (!boardData.IsValid())
            {
                args.Seed = context.rand.Next() % 1000000000;
                await GenerateBoard(context, args);
            }
            else
            {
                boardData.Tiles.Trim();
                // Get the actual tiles.
            }
            return boardData;
        }

        public async Task UpdateTiles(WebContext context, BoardData boardData, int startIndex, int length)
        {
            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(context.user).GetData().Where(x => x.Weight > 0).ToList();

            IBoardModeHelper helper = _boardModeService.GetBoardModeHelper(boardData.BoardModeId);

            int index = startIndex;

            for (int i = 0; i < length; i++)
            { 
                if (true || boardData.Tiles.Get(index) == 0)
                {
                    boardData.Tiles.Set(index, (short)(RandomUtils.GetRandomElement(tileTypes, context.rand).IdKey));
                }

                index = helper.GetNextTileIndex(context, boardData, index);
            }
            await Task.CompletedTask;
        }
    }
}
