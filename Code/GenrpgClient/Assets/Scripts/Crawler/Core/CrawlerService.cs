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
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.LoadSave.Constants;
using UnityEngine;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.ClientEvents;
using Assets.Scripts.ClientEvents;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Spells.Constants;
using System.Diagnostics.Eventing.Reader;
using System.Net.NetworkInformation;

namespace Assets.Scripts.Crawler.Services
{
    public class CrawlerService : ICrawlerService
    {
        private IClientUpdateService _updateService;
        private ICrawlerStatService _crawlerStatService;
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
        protected ITaskService _taskService;
        private IGameData _gameData;
        private IClientGameState _gs;
        private CancellationToken _token;
        private ICrawlerSpellService _spellService;
        private ILoadSaveService _loadSaveService;
        private IAwaitableService _awaitableService;
        private IStatService _statService;

        public const string SaveFileSuffix = ".sav";
        public const string StartSaveFileName = "Start" + SaveFileSuffix;

        private SetupDictionaryContainer<ECrawlerStates, IStateHelper> _stateHelpers = new SetupDictionaryContainer<ECrawlerStates, IStateHelper>();

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
            _updateService.AddTokenUpdate(this, UpdateGame, UpdateTypes.Regular, token);
            _updateService.AddTokenUpdate(this,OnLateUpdate, UpdateTypes.Late, token);
            await Task.CompletedTask;
        }


        public CancellationToken GetToken()
        {  
            return _token; 
        }

      

        public ScreenId GetCrawlerScreenId()
        {
            return ScreenId.Crawler;
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

                _dispatcher.Dispatch(new HideInfoPanelEvent());
                _taskService.ForgetTask(ChangeStateAsync(fullCrawlerState, token));
            }

            if (_inputService.GetKeyDown(CharCodes.Escape))
            {
                ActiveScreen activeScreen = _screenService.GetLayerScreen(ScreenLayers.Screens);
                if (activeScreen != null)
                {
                    _screenService.Close(activeScreen.ScreenId);
                }
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
            if (_stateStack.Count < 1)
            {
                return null;
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
                    newItem.Id = partyData.GetNextItemId();
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

        private void InitPartyAfterLoad(PartyData party)
        {
            _awaitableService.ForgetAwaitable(InitPartyAfterLoadAsync(party));  
        }

        private async Awaitable InitPartyAfterLoadAsync(PartyData party)
        { 
            if (party == null)
            {
                return;
            }

            _party = party;
            _party.Inventory = ConvertItemsFromSaveToGame(_party, _party.SaveInventory);

            foreach (PartyMember member in _party.Members)
            {
                member.Equipment = ConvertItemsFromSaveToGame(_party, member.SaveEquipment);
                member.ConvertDataAfterLoad();
            }

            foreach (PartyMember member in party.GetActiveParty())
            {
                _spellService.SetupCombatData(party, member);
            }

            _crawlerStatService.CalcPartyStats(_party, true);
            _inputService.SetDisabled(true);

            if (party.WorldId < 1)
            {
                party.WorldId = _rand.Next() % 5000000;
            }

            CrawlerWorld world = await _worldService.GetWorld(_party.WorldId);


            _screenService.Open(ScreenId.Crawler);

            while (_screenService.GetScreensNamed(ScreenId.Crawler) == null)
            {
                await Awaitable.NextFrameAsync(_token);
            }

            await UpdateCrawlerUI();
        }

        public bool ContinueGame()
        {
            PartyData party =_loadSaveService.ContinueGame<PartyData>();
            InitPartyAfterLoad(party);
            return party != null;
        }


        private PartyData CreatePartyDataForSlot(long slot, ECrawlerGameModes gameMode)
        {
            PartyData partyData = new PartyData() { Id = typeof(PartyData).Name + slot, SaveSlotId = slot, Seed = _rand.Next(), GameMode = gameMode };
            return partyData;
        }

        public PartyData LoadParty(long slot = 0)
        {
            PartyData party = _loadSaveService.LoadSlot<PartyData>(slot);
            
            if (party == null)
            {
                return null;
            }

            InitPartyAfterLoad(party);

            return party;
        }

        public void ClearAllStates()
        {
            _stateStack.Clear();
        }


        public async Task SaveGame()
        {
            if (_party != null)
            {

                if (_party.Combat != null)
                {
                    return;
                }

                _party.SaveInventory = ConvertItemsFromGameToSave(_party, _party.Inventory);

                foreach (PartyMember member in _party.Members)
                {
                    member.SaveEquipment = ConvertItemsFromGameToSave(_party, member.Equipment);
                    member.ConvertDataBeforeSave();
                }

                _loadSaveService.Save(_party, _party.SaveSlotId, true);
            }
            await Task.CompletedTask;
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
                    _taskService.ForgetTask(UpdateMovementAsync(token));
                }
            }
        }


        private async Task UpdateMovementAsync(CancellationToken token)
        {
            await _crawlerMapService.UpdateMovement(token);
        }


        public async Task UpdateCrawlerUI()
        {
            await Task.CompletedTask;
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
                    else if (detail.EntityTypeId == EntityTypes.QuestItem)
                    {
                        PartyQuestItem pqi = _party.QuestItems.FirstOrDefault(x => x.CrawlerQuestItemId == detail.EntityId);
                        if (pqi == null || pqi.Quantity < 1)
                        {
                            WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == detail.EntityId);

                            if (wqi != null)
                            {

                                InitialCombatState initialCombatState = new InitialCombatState()
                                {
                                    Difficulty = 1.5f,
                                    QuestItem = wqi,
                                };
                                ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
                            }
                            return;
                        }
                    }
                    else if (detail.EntityTypeId == EntityTypes.Riddle)
                    {
                        ChangeState(ECrawlerStates.Riddle, token, detail);
                    }
                }
            }

            ProcessEncounters(map, token);


            UpdateIfNotMoving(true, token);
        }


        private void ProcessEncounters(CrawlerMap map, CancellationToken token)
        { 

            int encounterTypeId = map.Get(_party.MapX, _party.MapZ, CellIndex.Encounter);

            MapEncounterType etype = _gameData.Get<MapEncounterSettings>(_gs.ch).Get(encounterTypeId);

            if (etype != null)
            {
                bool didVisitThisRun = _crawlerMapService.PartyHasVisited(_party.MapId, _party.MapX, _party.MapZ, true);
              
                if (!didVisitThisRun)
                {
                    if (!etype.CanRepeat && !_party.CompletedMaps.HasBit(map.IdKey))
                    {
                        CrawlerMapStatus mapStatus = _party.Maps.FirstOrDefault(x => x.MapId == map.IdKey);

                        if (mapStatus != null)
                        {
                            PointXZ pt = mapStatus.OneTimeEncounters.FirstOrDefault(x => x.X == _party.MapX && x.Z == _party.MapZ);
                            if (pt == null)
                            {
                                if (encounterTypeId == MapEncounters.Treasure)
                                {
                                    mapStatus.OneTimeEncounters.Add(new PointXZ() { X = _party.MapX, Z = _party.MapZ });
                                    ChangeState(ECrawlerStates.GiveLoot, token);
                                }
                                else if (encounterTypeId == MapEncounters.Stats)
                                {

                                    ChangeState(ECrawlerStates.GainStats, token);
                                }
                            }
                        }
                    }

                    if (encounterTypeId == MapEncounters.Monsters)
                    {
                        InitialCombatState initialCombatState = new InitialCombatState()
                        {
                            Difficulty = 1.5f,
                        };
                        ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
                        return;
                    }
                    else if (encounterTypeId == MapEncounters.Trap)
                    {

                        if (_party.Buffs.Get(PartyBuffs.Levitate) == 0 && !_party.CurrentMap.Cleansed.HasBit(map.GetIndex(_party.MapX, _party.MapZ)))
                        {
                            _dispatcher.Dispatch(new ShowFloatingText("It's a Trap!", EFloatingTextArt.Error));
                            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                            int maxStatusEffectTier = Math.Min(StatusEffects.Dead - 1, (int)(map.Level * mapSettings.TrapDebuffLevelScaling));

                            int minDam = map.Level * mapSettings.TrapMinDamPerLevel;
                            int maxDam = map.Level * mapSettings.TrapMaxDamagePerLevel;

                            foreach (PartyMember pm in _party.GetActiveParty())
                            {
                                double luckBonus = _crawlerStatService.GetStatBonus(_party, pm, StatTypes.Luck) / 100.0f;

                                if (_rand.NextDouble() < mapSettings.TrapHitChance - luckBonus)
                                {
                                    continue;
                                }

                                int damage = MathUtils.IntRange(minDam, maxDam, _rand);

                                _crawlerStatService.Add(_party, pm, StatTypes.Health, StatCategories.Curr, -damage, ElementTypes.Physical);

                                if (pm.Stats.Curr(StatTypes.Health) < 1)
                                {
                                    pm.StatusEffects.SetBit(StatusEffects.Dead);
                                    continue;
                                }

                                if (_rand.NextDouble() < mapSettings.TrapDebuffChance && maxStatusEffectTier > 0)
                                {
                                    int tier = Math.Min(MathUtils.IntRange(1, maxStatusEffectTier, _rand), MathUtils.IntRange(1, maxStatusEffectTier, _rand));


                                    StatusEffect effect = effects.FirstOrDefault(x => x.IdKey == tier);

                                    if (effect != null)
                                    {
                                        pm.StatusEffects.SetBit(tier);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public ForcedNextState TryGetNextForcedState(CrawlerMap map, int ex, int ez)
        {
            byte buildingId = map.Get(ex, ez, CellIndex.Building);

            BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(buildingId);

            if (btype == null)
            {
                return null;
            }

            List<MapCellDetail> details = map.Details.Where(d => d.X == ex && d.Z == ez).ToList();

            List<IStateHelper> helpers = _stateHelpers.GetDict().Values.ToList();

            foreach (MapCellDetail detail in details)
            {
                IStateHelper helper = helpers.FirstOrDefault(x => x.TriggerDetailEntityTypeId() == detail.EntityTypeId);
                if (helper != null)
                {
                    return new ForcedNextState() { NextState = helper.GetKey(), Detail = detail };
                }
            }

            IStateHelper buildingHelper = helpers.FirstOrDefault(x=>x.TriggerBuildingId() == buildingId);   

            if (buildingHelper != null)
            {
                return new ForcedNextState() { NextState = buildingHelper.GetKey() };
            }

            return null;
        }

        public void NewGame(ECrawlerGameModes gameMode)
        {
            PartyData party = CreatePartyDataForSlot(LoadSaveConstants.MinSlot, gameMode);
            _party = party;
            SaveGame().Wait();
            InitPartyAfterLoad(party);
        }
    }
}
