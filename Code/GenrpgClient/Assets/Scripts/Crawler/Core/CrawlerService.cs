using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Model;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.States.StateHelpers;
using System.Collections.Concurrent;

namespace Assets.Scripts.Crawler.Services
{
    public class CrawlerService : ICrawlerService
    {
        private IClientUpdateService _updateService;
        private ICrawlerStatService _statService;
        private IScreenService _screenService;
        private ICrawlerMapService _crawlerMapService;
        protected ILogService _logService;
        protected IRepositoryService _repoService;
        protected IDispatcher _dispatcher;
        protected IClientRandom _rand;
        protected ICrawlerCombatService _combatService;
        protected ICrawlerWorldService _worldService;
        protected ILootGenService _lootGenService;
        private IInputService _inputService;

        public const string SaveFileSuffix = ".sav";
        public const string StartSaveFileName = "Start" + SaveFileSuffix;

        private SetupDictionaryContainer<ECrawlerStates, IStateHelper> _stateHelpers = new SetupDictionaryContainer<ECrawlerStates, IStateHelper>();

        private IClientGameState _gs;
        private CancellationToken _token;
        private ICrawlerSpellService _spellService;
        private PartyData _party { get; set; }

        public PartyData GetParty()
        {
            return _party;
        }

        private Stack<CrawlerStateData> _stateStack { get; set; } = new Stack<CrawlerStateData>();


        private Dictionary<char, char> _equivalentKeys = new Dictionary<char, char>();

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _updateService.AddTokenUpdate(this, UpdateGame, UpdateType.Regular, token);
            _updateService.AddTokenUpdate(this,OnLateUpdate, UpdateType.Late, token);
            await Task.CompletedTask;
        }

        public async Task Init(PartyData party, CancellationToken token)
        {
            _inputService.SetDisabled(true);
            _token = token;
            this._party = party;

            if (party.WorldId < 1)
            {
                party.WorldId = _rand.Next() % 5000000;
            }
#if UNITY_EDITOR
            if (InitClient.EditorInstance.MapGenSeed > 0)
            {
                party.WorldId = InitClient.EditorInstance.MapGenSeed;
            }
#endif
            CrawlerWorld world = await _worldService.GetWorld(this._party.WorldId);

            await Task.CompletedTask;
            ChangeState(ECrawlerStates.TavernMain, token);
            await UpdateCrawlerUI();
        }

        public void ChangeState(ECrawlerStates crawlerState, CancellationToken token, object extraData = null , ECrawlerStates returnState= ECrawlerStates.None)
        {
            CrawlerStateData stateData = new CrawlerStateData(returnState) { ExtraData = extraData };
            CrawlerStateAction action = new CrawlerStateAction(null, CharCodes.None, crawlerState, extraData: extraData);
            ChangeState(stateData, action, token);
        }

        class FullCrawlerState
        {
            public CrawlerStateData StateData;
            public CrawlerStateAction Action;
        }

        private ConcurrentQueue<FullCrawlerState> _stateQueue = new ConcurrentQueue<FullCrawlerState>();

        public void ChangeState(CrawlerStateData data, CrawlerStateAction action, CancellationToken token)
        {
            _stateQueue.Enqueue(new FullCrawlerState() { Action = action, StateData = data });
        }

        private void OnLateUpdate(CancellationToken token)
        {

            if (_stateQueue.TryDequeue(out FullCrawlerState fullCrawlerState))
            {
                fullCrawlerState.Action.OnClickAction?.Invoke();

                // This lets you enter commands without changing state.
                if (fullCrawlerState.Action.NextState == ECrawlerStates.DoNotChangeState)
                {
                    return;
                }

                _crawlerMapService.ClearMovement();

                TaskUtils.ForgetTask(ChangeStateAsync(fullCrawlerState, token));
            }
        }

        public CrawlerStateData PopState()
        {
            if (_stateStack.Count > 1)
            {
                _stateStack.Pop();
            }
            CrawlerStateData stateData = _stateStack.Peek();
            _dispatcher.Dispatch(stateData);
            return stateData;
        }

        public CrawlerStateData GetTopLevelState()
        {
            while (_stateStack.Count > 1)
            {
                _stateStack.Pop();
            }
            return _stateStack.Peek();
        }

        public ECrawlerStates GetState()
        {
            if (_stateStack.Count < 1)
            {
                return ECrawlerStates.None;
            }
            return _stateStack.Peek().Id;
        }


        private async Task ChangeStateAsync(FullCrawlerState fullState, CancellationToken token)
        {
            try
            {
                CrawlerStateData currData = fullState.StateData;
                CrawlerStateAction action = fullState.Action;
                CrawlerStateData nextStateData = null;
                foreach (CrawlerStateData stackData in _stateStack)
                {
                    if (stackData.Id == action.NextState)
                    {
                        nextStateData = stackData;
                        break;
                    }
                }

                if (nextStateData != null)
                {
                    while (_stateStack.Count > 1 && _stateStack.Peek().Id != nextStateData.Id)
                    {
                        _stateStack.Pop();
                    }
                }

                IStateHelper stateHelper = GetStateHelper(action.NextState);
                if (stateHelper != null)
                {
                    nextStateData = await stateHelper.Init(currData, action, token);

                    if (nextStateData.DoNotTransitionToThisState)
                    {
                        return;
                    }

                    if (stateHelper.IsTopLevelState())
                    {
                        _stateStack.Clear();
                    }
                }

                if (nextStateData != null)
                {
                    if (nextStateData.ForceNextState)
                    {
                        ChangeState(nextStateData.Id, token, nextStateData.ExtraData);
                    }
                    else
                    {
                        _stateStack.Push(nextStateData);
                        _dispatcher.Dispatch(nextStateData);
                    }
                }
                else
                {
                    _logService.Error("State not found: " + action.NextState);
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CrawlerChangeState");
            }
        }

        private IStateHelper GetStateHelper(ECrawlerStates state)
        {
            if (_stateHelpers.TryGetValue(state, out IStateHelper stateHelper))
            {
                return stateHelper;
            }
            return null;
        }

        private List<CrawlerSaveItem> ConvertItemsFromGameToSave(PartyData partyData, List<Item> items)
        {
            List<CrawlerSaveItem> retval = new List<CrawlerSaveItem>();

            if (items == null)
            {
                return retval;
            }

            foreach (Item item in items)
            {
                CrawlerSaveItem newItem = new CrawlerSaveItem()
                {
                    Id = item.Id,
                    Name = item.Name,
                };

                if (string.IsNullOrEmpty(newItem.Id) || newItem.Id.Length > 6)
                {
                    newItem.Id = (++partyData.NextItemId).ToString();
                }

                newItem.Set(CIdx.ItemTypeId, item.ItemTypeId);
                newItem.Set(CIdx.LootRankId, item.LootRankId);
                newItem.Set(CIdx.Level, item.Level);
                newItem.Set(CIdx.ScalingTypeId, item.ScalingTypeId);
                newItem.Set(CIdx.EquipSlotId, item.EquipSlotId);
                newItem.Set(CIdx.BuyCost, item.BuyCost);
                newItem.Set(CIdx.SellValue, item.SellValue);
                newItem.Set(CIdx.QualityTypeId, item.QualityTypeId);

                int statIndex = 0;
                for (int e = 0; e < item.Effects.Count && statIndex < 4; e++)
                {
                    ItemEffect eff = item.Effects[e];
                    if (eff.EntityTypeId == EntityTypes.Stat &&
                        eff.EntityId > 0  &&
                        eff.Quantity > 0)
                    {
                        newItem.Set(CIdx.Stat0 + statIndex * 2, eff.EntityId);
                        newItem.Set(CIdx.Val0 + statIndex * 2, eff.Quantity);
                        statIndex++;
                    }
                }
                newItem.CreateDatString();
                retval.Add(newItem);
            }
            return retval;
        }

        private List<Item> ConvertItemsFromSaveToGame(PartyData partyData, List<CrawlerSaveItem> saveItems)
        {
            List<Item> retval = new List<Item>();
            if (saveItems == null)
            {
                return retval;
            }

            foreach (CrawlerSaveItem saveItem in saveItems)
            {
                Item newItem = new Item()
                {
                    Id = saveItem.Id,
                    Name = saveItem.Name,
                    BuyCost = saveItem.Get(CIdx.BuyCost),
                    ScalingTypeId = saveItem.Get(CIdx.ScalingTypeId),
                    EquipSlotId = saveItem.Get(CIdx.EquipSlotId),
                    ItemTypeId = saveItem.Get(CIdx.ItemTypeId),
                    Level = saveItem.Get(CIdx.Level),
                    LootRankId = saveItem.Get(CIdx.LootRankId),
                    QualityTypeId = saveItem.Get(CIdx.QualityTypeId),
                    Quantity = 1,
                    SellValue = saveItem.Get(CIdx.SellValue),
                    Procs = new List<ItemProc>()
                };

                for (int i = 0; i < 4; i++)
                {
                    long statId = saveItem.Get(CIdx.Stat0 + i * 2);
                    long statVal = saveItem.Get(CIdx.Val0 + i * 2);    
                    if (statId > 0 && statVal > 0)
                    {
                        newItem.Effects.Add(new ItemEffect() { EntityTypeId = EntityTypes.Stat, EntityId = statId, Quantity = statVal });
                    }
                }

                retval.Add(newItem);

            }
            return retval;
        }

        private void SetupPartyForGameplay(PartyData party)
        {
            
            party.SpeedupListener = this;


            foreach (PartyMember member in party.GetActiveParty())
            {
                _spellService.SetupCombatData(party, member);
            }
        }

        private void InitPartyAfterLoad(PartyData party)
        {
            if (party == null)
            {
                return;
            }

            this._party.Inventory = ConvertItemsFromSaveToGame(this._party, this._party.SaveInventory);

            foreach (PartyMember member in this._party.Members)
            {
                member.Equipment = ConvertItemsFromSaveToGame(this._party, member.SaveEquipment);
                member.ConvertDataAfterLoad();
            }

        }

        public async Task<PartyData> LoadParty(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = StartSaveFileName;
            }

            _party = await _repoService.Load<PartyData>(filename);

            InitPartyAfterLoad(_party);

            if (_party == null)
            {
                _party = new PartyData() { Id = filename, Seed = _rand.Next() };
                await SaveGame();
            }

            if (_party.Seed == 0)
            {
                _party.Seed = _rand.Next();
                await SaveGame();
            }

            SetupPartyForGameplay(_party);


            _statService.CalcPartyStats(_party, true);
            return _party;
        }

        public void ClearAllStates()
        {
            _stateStack.Clear();
        }


        public async Task SaveGame()
        {
            if (_party != null)
            {
                IClientRepositoryService clientRepoService = _repoService as IClientRepositoryService;

                _party.SaveInventory = ConvertItemsFromGameToSave(_party, _party.Inventory);

                foreach (PartyMember member in _party.Members)
                {
                    member.SaveEquipment = ConvertItemsFromGameToSave(_party, member.Equipment);
                    member.ConvertDataBeforeSave();
                }

                await clientRepoService.SavePrettyPrint(_party);

            }
        }

        private void UpdateGame(CancellationToken token)
        {
            UpdateMovement(token);

            if (_crawlerMapService.UpdatingMovement() || _stateQueue.Count > 0)
            {
                return;
            }

            UpdateIfNotMoving(false, token);
        }

        private void UpdateIfNotMoving(bool atEndOfMove, CancellationToken token)
        {
            UpdateInputs(token);
            UpdateEncounters(token, atEndOfMove);
        }

        private void UpdateInputs(CancellationToken token)
        {
            if (_crawlerMapService.UpdatingMovement())
            {
                return;
            }
            if (_stateStack.TryPeek(out CrawlerStateData currentData))
            {
                if (currentData.Actions.Count > 0)
                {
                    foreach (CrawlerStateAction action in currentData.Actions)
                    {
                        //Explcitly set Escape to go back up a level, Do not have a global escape
                        if (_inputService.GetKeyDown(action.Key))
                        {
                            ChangeState(currentData, action, token);
                        }
                        else if (_equivalentKeys.TryGetValue(action.Key, out char otherKey))
                        {
                            if (_inputService.GetKey(otherKey))
                            {
                                ChangeState(currentData, action, token);
                            }
                        }

                    }
                }

                if (currentData.ShouldCheckInput() &&
                    (_inputService.GetKeyDown(CharCodes.Return) ||
                    _inputService.GetKeyDown(CharCodes.Enter)))
                {
                    currentData.CheckInput();
                }
            }

            if (_inputService.GetKeyDown(CharCodes.Escape) || _inputService.GetKeyDown(CharCodes.Space) || _inputService.GetKeyDown(CharCodes.Enter))
            {
                _shouldTriggerSpeedup = true;
            }
        }

        private bool _shouldTriggerSpeedup = false;
        public void ClearSpeedup()
        {
            _shouldTriggerSpeedup = false;
        }

        public bool TriggerSpeedupNow()
        {
            if (_shouldTriggerSpeedup)
            {
                _shouldTriggerSpeedup = false;
                return true;
            }
            return false;
        }

        private void UpdateEncounters(CancellationToken token, bool atEndOfMove)
        {
            if (!atEndOfMove)
            {
                if (_crawlerMapService.UpdatingMovement())
                {
                    return;
                }
            }
            _combatService.CheckForEncounter(atEndOfMove, token);
        }

        private void UpdateMovement(CancellationToken token)
        {
            if (_stateStack.TryPeek(out CrawlerStateData currentData))
            {
                if (currentData.Id == ECrawlerStates.ExploreWorld)
                {
                    TaskUtils.ForgetTask(UpdateMovementAsync(token));
                }
            }
        }


        private async Task UpdateMovementAsync(CancellationToken token)
        {
            await _crawlerMapService.UpdateMovement(token);
        }


        public async Task UpdateCrawlerUI()
        {
            CrawlerWorld world = await _worldService.GetWorld(_party.WorldId);
            CrawlerMap map = world.GetMap(_party.MapId);

            _dispatcher.Dispatch(new CrawlerUIUpdate());

        }
        public async Task OnFinishMove(bool movedPosition, CancellationToken token)
        {
            CrawlerWorld world = await _worldService.GetWorld(_party.WorldId);
            CrawlerMap map = world.GetMap(_party.MapId);

            MapCellDetail detail = map.Details.FirstOrDefault(x => x.X == _party.MapX && x.Z == _party.MapZ);
            
            await UpdateCrawlerUI();

            if (movedPosition)
            {
                if (detail != null)
                {
                    if (detail.EntityTypeId == EntityTypes.Map)
                    {
                        ChangeState(ECrawlerStates.MapExit, token, detail);
                    }
                    else if (detail != null && detail.EntityTypeId == EntityTypes.QuestItem)
                    {
                        PartyQuestItem pqi = _party.QuestItems.FirstOrDefault(x => x.CrawlerQuestItemId == detail.EntityId);
                        if (pqi == null || pqi.Quantity < 1)
                        {
                            InitialCombatState initialCombatState = new InitialCombatState()
                            {
                                Difficulty = 3,
                            };
                            ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
                            return;
                        }
                    }
                    else if (detail.EntityTypeId == EntityTypes.Riddle)
                    {
                        ChangeState(ECrawlerStates.Riddle, token, detail);
                    }
                }
            }

            int encounterBits = map.Get(_party.MapX, _party.MapZ, CellIndex.Encounter);

            if (encounterBits != 0 && encounterBits != MapEncounters.OtherFeature)
            {
                bool thisRunOnly = encounterBits != MapEncounters.Treasure;
                string encounterName = encounterBits == MapEncounters.Treasure ? nameof(MapEncounters.Treasure)
                    : encounterBits == MapEncounters.Trap ? nameof(MapEncounters.Trap) :
                    encounterBits == MapEncounters.Monsters ? nameof(MapEncounters.Monsters) : "Unknown";

                bool didVisit = _crawlerMapService.PartyHasVisited(_party.MapId, _party.MapX, _party.MapZ, thisRunOnly);

                if (!didVisit)
                {
                    if (FlagUtils.IsSet(encounterBits, MapEncounters.Monsters))
                    {
                        InitialCombatState initialCombatState = new InitialCombatState()
                        {
                            Difficulty = 2,
                        };
                        ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
                        return;
                    }
                    else if (FlagUtils.IsSet(encounterBits, MapEncounters.Treasure))
                    {
                        ChangeState(ECrawlerStates.GiveLoot, token);
                    }  
                }
            }

            UpdateIfNotMoving(true, token);
        }

    }
}
