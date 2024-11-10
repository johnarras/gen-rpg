using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.BoardGame.Helpers.TileTypeHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.RequestServer.Spawns.Services;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.Messages.RollDice;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Rewards.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserEnergy.Messages;
using Genrpg.Shared.UserEnergy.Settings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using System.Security;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class DiceRollService : IDiceRollService
    {
        private IGameData _gameData = null;
        private IWebSpawnService _spawnService = null;
        private IWebRewardService _rewardService = null;
        private ILogService _logService = null;
        private IBoardModeService _boardModeService = null;
        private IBoardPrizeService _boardPrizeService = null;
        public async Task RollDice(WebContext context, DiceRollParams rollData)
        {
            try
            {
                CoreUserData userData = await context.GetAsync<CoreUserData>();

                // Track if energy was above the storage or not, so we can know to start
                // the regen timer.
                long startEnergy = userData.Coins.Get(UserCoinTypes.Energy);
                UserEnergySettings energySettings = _gameData.Get<UserEnergySettings>(context.user);
                long energyStorage = energySettings.GetMaxStorage(context.user.Level);

                BoardData boardData = await context.GetAsync<BoardData>();

                RollDiceArgs args = new RollDiceArgs()
                {
                    Board = boardData,
                    UserData = userData,
                    Helper = _boardModeService.GetBoardModeHelper(boardData.BoardModeId),
                    Mode = _gameData.Get<BoardModeSettings>(context.user).Get(boardData.BoardModeId),
                };
                args.Result.StartIndex = boardData.TileIndex;

                await args.Helper.SetupRollDiceArgs(context, args);

                if (!args.Result.FreeRolls)
                {
                    if (userData.Coins.Get(UserCoinTypes.Energy) < userData.PlayMult)
                    {
                        args.Result.DiceRollResult = DiceRollResults.OutOfDice;
                        context.Results.Add(args.Result);
                        return;
                    }
                    else
                    {
                        userData.Coins.Add(UserCoinTypes.Energy, -userData.PlayMult);
                        args.Result.DiceCost = userData.PlayMult;
                    }
                }

                await GetTilesReached(context, args);

                await GetRewards(context, args);

                await UpdateAfterRoll(context, args);

                args.Result.NextBoard = args.Board;

                RewardData rewardData = new RewardData();

                foreach (RollStep step in args.Result.Steps)
                {
                    rewardData.Rewards.AddRange(step.Rewards);
                }

                await _rewardService.GiveRewardsAsync(context, rewardData.Rewards);

                // If the user started above their energy cap and just went below it, they now have a start timer.
                if (startEnergy >= energyStorage &&
                    userData.Coins.Get(UserCoinTypes.Energy) < energyStorage)
                {
                    userData.LastHourlyReset = DateTime.UtcNow;
                    context.Results.Add(new UpdateUserEnergyResult() { EnergyAdded = 0, LastHourlyReset = userData.LastHourlyReset });
                }

                context.Results.Add(args.Result);
            }
            catch (Exception ex)
            {
                _logService.Info(ex.Message + " " + ex.StackTrace);
            }
        }      

        private async Task GetTilesReached(WebContext context, RollDiceArgs args)
        {
            int startRollTotal = 0;

            List<int> rollValues = new List<int>();

            for (int i = 0; i < args.Result.DiceCount; i++)
            {
                int newRollValue = MathUtils.IntRange(1, args.Result.DiceSides, context.rand);
                rollValues.Add(newRollValue);
                startRollTotal += newRollValue;
            }

            List<int> tileIndexesReached = new List<int>();

            int tileIndex = args.Board.TileIndex;

            int endRollTotal = 0;
            for (int i = 0; i < startRollTotal; i++)
            {
                endRollTotal++;
                tileIndex = args.Helper.GetNextTileIndex(context, args, tileIndex);

                tileIndexesReached.Add(tileIndex);

                long tileTypeId = args.Board.Tiles[tileIndex];

                if (tileTypeId == TileTypes.EndPath)
                {
                    args.ExitMode = true;
                    break;
                }

                if (tileTypeId == TileTypes.Home && args.Helper.BonusModeEndType == EBonusModeEndTypes.HomeTile)
                {
                    args.Board.ModeLapsLeft--;
                    if (args.Board.ModeLapsLeft < 1)
                    {
                        args.ExitMode = true;
                        break;
                    }
                }              

                if (tileIndex == args.Result.StartIndex && args.Helper.BonusModeEndType == EBonusModeEndTypes.StartTile)
                {
                    args.Board.ModeLapsLeft--;
                    if (args.Board.ModeLapsLeft < 1)
                    {
                        args.ExitMode = true;
                        break;
                    }
                }
            }

            if (args.Helper.BonusModeEndType == EBonusModeEndTypes.RollCount)
            {
                args.Board.ModeRollsLeft--;

                if (args.Board.ModeRollsLeft < 1)
                {
                    args.ExitMode = true;
                }
            }

            if (endRollTotal < startRollTotal)
            {
                while (rollValues.Sum() > endRollTotal)
                {
                    for (int i = 0; i < rollValues.Count; i++)
                    {
                        if (rollValues[i]> 1)
                        {
                            rollValues[i]--;
                            break;
                        }
                    }
                }
            }

            args.Result.RollTotal = endRollTotal;
            args.Result.RollValues = rollValues;
            args.Result.TilesIndexesReached = tileIndexesReached;
            args.Board.TileIndex = tileIndexesReached.Last();
            args.Result.StartIndex = tileIndexesReached[0];
            args.Result.EndIndex = tileIndexesReached.Last();
           
            await Task.CompletedTask;
        }

        protected async Task GetRewards(WebContext context, RollDiceArgs args)
        {
            ActivationSettings activationSettings = _gameData.Get<ActivationSettings>(context.user);
            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(context.user).GetData();
            for (int step = 0; step < args.Result.TilesIndexesReached.Count; step++)
            {
                int tileIndex = args.Result.TilesIndexesReached[step];
                RollStep rollStep = new RollStep() { Step = step };
                args.Result.Steps.Add(rollStep);
                long tileTypeId = args.Board.Tiles[tileIndex];
                
                TileType tileType = tileTypes.FirstOrDefault(x => x.IdKey == tileTypeId);
                if (tileType == null)
                {
                    continue;
                }

                bool landing = step == args.Result.TilesIndexesReached.Count - 1;

                if (args.Mode.GiveDefaultTileRewards)
                {

                    if (tileType.ActivationCostScale > 0)
                    {
                        TileCharge charge = args.Board.Charges.FirstOrDefault(x => x.TileIndex == tileIndex);
                        if (charge == null)
                        {
                            charge = new TileCharge() { TileIndex = tileIndex };
                            charge.MaxCharge = activationSettings.GetMaxCharge(context.user.Level, tileType.ActivationCostScale);
                            charge.CurrCharge = 0;
                            args.Board.Charges.Add(charge);
                        }

                        charge.CurrCharge += (int)(Math.Pow(args.PlayMult, activationSettings.ChargeDiceMultPower));

                        if (charge.CurrCharge >= charge.MaxCharge)
                        {
                            charge.CurrCharge = 0;

                            RollData rollData = new RollData();

                            List<RewardList> activationRewards = await _spawnService.Roll(context, tileType.LandRewards, rollData);

                            foreach (RewardList rlist in activationRewards)
                            {
                                await _rewardService.GiveRewardsAsync(context, rlist.Rewards);
                            }
                            rollStep.Rewards.AddRange(activationRewards);
                        }
                    }
                    else
                    {
                        RollData rollData = new RollData()
                        {
                            RewardSourceId = RewardSources.Tile,
                            Scale = args.PlayMult,
                            Times = 1,
                        };

                        List<RewardList> passRewards = await _spawnService.Roll(context, tileType.PassRewards, rollData);

                        rollStep.Rewards.AddRange(passRewards);

                        if (landing)
                        {
                            rollData.RewardSourceId = RewardSources.Tile;

                            List<RewardList> landRewards = await _spawnService.Roll(context, tileType.PassRewards, rollData);

                            rollStep.Rewards.AddRange(landRewards);
                        }
                    }
                }

                long passReward = args.Board.PassRewards[tileIndex];
                args.Board.PassRewards[tileIndex] = 0;

                if (passReward > 0)
                {
                    BoardPrize prize = _gameData.Get<BoardPrizeSettings>(context.user).Get(passReward);

                    if (prize != null)
                    {
                        RollData rollData = new RollData()
                        {
                            RewardSourceId = RewardSources.PassTile,
                            EntityId = tileIndex,
                            Scale = args.PlayMult
                        };

                        List<RewardList> rewardLists = await _spawnService.Roll(context, prize.LootTable, rollData);
                        if (rewardLists.Count > 0)
                        {
                            rollStep.Rewards.AddRange(rewardLists);
                        }
                    }
                }
                if (landing)
                {
                    long landReward = args.Board.LandRewards[tileIndex];
                    args.Board.LandRewards[tileIndex] = 0;
                    BoardPrize prize = _gameData.Get<BoardPrizeSettings>(context.user).Get(landReward);

                    if (prize != null)
                    {
                        RollData rollData = new RollData()
                        {
                            RewardSourceId = RewardSources.LandTile,
                            EntityId = tileIndex,
                            Scale = args.PlayMult
                        };

                        List<RewardList> rewardLists = await _spawnService.Roll(context, prize.LootTable, rollData);
                        if (rewardLists.Count > 0)
                        {
                            rollStep.Rewards.AddRange(rewardLists);
                        }
                    }
                }
            }
        }

        private async Task UpdateAfterRoll(WebContext context, RollDiceArgs args)
        {
            await _boardPrizeService.UpdatePrizesForBoard(context, args.Board);

            NextBoardData nextData = context.TryGetFromCache<NextBoardData>();

            if (nextData != null && nextData.NextBoard != null)
            {
                BoardStackData stackData = await context.GetAsync<BoardStackData>();

                BoardData existingBoardData = stackData.Boards.FirstOrDefault(x => x.BoardModeId == args.Board.BoardModeId &&
                x.ZoneTypeId == args.Board.ZoneTypeId && x.OwnerId == args.Board.OwnerId);

                if (existingBoardData == null)
                {
                    stackData.Boards.Add(args.Board);
                }
                context.Remove<BoardData>(context.user.Id);

                context.Set<BoardData>(nextData.NextBoard);

                args.Result.NextBoard = nextData.NextBoard;

                await _boardPrizeService.UpdatePrizesForBoard(context, nextData.NextBoard);
            }
        }
    }
}
