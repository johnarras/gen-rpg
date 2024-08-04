using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Events;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Model;
using Assets.Scripts.ProcGen.RandomNumbers;
using Assets.Scripts.UI.Crawler;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Assets.Scripts.Crawler.Services
{
    public class CrawlerService : ICrawlerService
    {
        private IUnityUpdateService _updateService;
        private ICrawlerStatService _statService;
        private IScreenService _screenService;
        private ICrawlerMapService _crawlerMapService;
        protected ILogService _logService;
        protected IRepositoryService _repoService;
        protected IDispatcher _dispatcher;
        protected IClientRandom _rand;
        protected ICombatService _combatService;
        protected ICrawlerWorldService _worldService;
        protected ILootGenService _lootGenService;
        private IInputService _inputService;

        public const string SaveFileSuffix = ".sav";
        public const string StartSaveFileName = "Start" + SaveFileSuffix;

        private SetupDictionaryContainer<ECrawlerStates, IStateHelper> _stateHelpers = new SetupDictionaryContainer<ECrawlerStates, IStateHelper>();

        private IUnityGameState _gs;
        private CancellationToken _token;
        private PartyData _party { get; set; }

        public PartyData GetParty()
        {
            return _party;
        }

        private Stack<CrawlerStateData> _stateStack { get; set; } = new Stack<CrawlerStateData>();


        private Dictionary<KeyCode, KeyCode> _equivalentKeys = new Dictionary<KeyCode, KeyCode>();

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _updateService.AddTokenUpdate(this, UpdateGame, UpdateType.Regular);

            char alpha0 = (char)(KeyCode.Alpha0);
            char keypad0 = (char)(KeyCode.Keypad0);

            for (int i = 0; i <= 9; i++)
            {
                _equivalentKeys[(KeyCode)(i + alpha0)] = (KeyCode)(i + keypad0);
                _equivalentKeys[(KeyCode)(i + keypad0)] = (KeyCode)(i + alpha0);
            }


            await Task.CompletedTask;
        }

        public async Awaitable Init(PartyData party, CancellationToken token)
        {
            _inputService.SetDisabled(true);
            _token = token;
            _party = party;

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
            CrawlerWorld world = await _worldService.GetWorld(_party.WorldId);

            await Task.CompletedTask;
            ChangeState(ECrawlerStates.TavernMain, token);
            await UpdateCrawlerUI();
        }

        public void ChangeState(ECrawlerStates crawlerState, CancellationToken token, object extraData = null)
        {
            CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.None) { ExtraData = extraData };
            CrawlerStateAction action = new CrawlerStateAction(null, KeyCode.None, crawlerState, extraData: extraData);
            ChangeState(stateData, action, token);
        }

        public void ChangeState(CrawlerStateData data, CrawlerStateAction action, CancellationToken token)
        {
            action.OnClickAction?.Invoke();
            AwaitableUtils.ForgetAwaitable(ChangeStateAsync(data, action, token));
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



        public async Awaitable ChangeStateAsync(CrawlerStateData currData, CrawlerStateAction action, CancellationToken token)
        {
            try
            {
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

        private void SetupParty(PartyData party)
        {

            ActiveScreen activeScreen = _screenService.GetScreen(ScreenId.Crawler);

            if (activeScreen != null)
            {
                CrawlerScreen crawlerScreen = activeScreen.Screen as CrawlerScreen;

                if (crawlerScreen != null)
                {
                    party.WorldPanel = crawlerScreen.WorldPanel;
                    party.ActionPanel = crawlerScreen.ActionPanel;
                    party.StatusPanel = crawlerScreen.StatusPanel;
                }
            }
            party.SpeedupListener = this;

        }

        public async Awaitable<PartyData> LoadParty(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = StartSaveFileName;
            }

            _party = await _repoService.Load<PartyData>(filename);

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

            SetupParty(_party);

            _statService.CalcPartyStats(_party, true);
            return _party;
        }

        public void ClearAllStates()
        {
            _stateStack.Clear();
        }


        public async Awaitable SaveGame()
        {
            if (_party != null)
            {
                IClientRepositoryService clientRepoService = _repoService as IClientRepositoryService;

                await clientRepoService.SavePrettyPrint(_party);

            }
        }

        private void UpdateGame(CancellationToken token)
        {
            UpdateMovement(token);

            if (_crawlerMapService.UpdatingMovement())
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
                        if (Input.GetKeyDown(action.Key))
                        {
                            ChangeState(currentData, action, token);
                        }
                        else if (_equivalentKeys.TryGetValue(action.Key, out KeyCode otherKey))
                        {
                            if (Input.GetKey(otherKey))
                            {
                                ChangeState(currentData, action, token);
                            }
                        }

                    }
                }

                if (currentData.ShouldCheckInput() &&
                    (Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    currentData.CheckInput();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
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
                    AwaitableUtils.ForgetAwaitable(UpdateMovementAsync(token));
                }
            }
        }


        private async Awaitable UpdateMovementAsync(CancellationToken token)
        {
            await _crawlerMapService.UpdateMovement(token);
        }


        public async Awaitable UpdateCrawlerUI()
        {
            CrawlerWorld world = await _worldService.GetWorld(_party.WorldId);
            CrawlerMap map = world.GetMap(_party.MapId);

            _dispatcher.Dispatch(new CrawlerUIUpdate() { Map = map, World = world, Party = _party });

        }
        public async Awaitable OnFinishMove(bool movedPosition, CancellationToken token)
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

        public void CreateSpline()
        {
            WorldPanel worldPanel = _party.WorldPanel as WorldPanel;

            worldPanel.CreateSpline();
        }
    }
}
