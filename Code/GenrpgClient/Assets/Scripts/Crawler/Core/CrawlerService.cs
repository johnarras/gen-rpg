using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Model;
using Assets.Scripts.ProcGen.RandomNumbers;
using Assets.Scripts.UI.Crawler;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;
using UnityEngine;

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

        const string SaveFileSuffix = ".sav";
        const string SaveFileName = "Start" + SaveFileSuffix;

        private SetupDictionaryContainer<ECrawlerStates, IStateHelper> _stateHelpers = new SetupDictionaryContainer<ECrawlerStates, IStateHelper>();

        private IUnityGameState _gs;
        private CancellationToken _token;
        private PartyData _party { get; set; }

        public PartyData GetParty()
        {
            return _party;
        }

        private Stack<CrawlerStateData> _stateData { get; set; } = new Stack<CrawlerStateData>();


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

        public async Awaitable Init(CancellationToken token)
        {
            _token = token;

            await Task.CompletedTask;
            ChangeState(ECrawlerStates.TavernMain, token);
        }

        public void ChangeState(ECrawlerStates crawlerState, CancellationToken token, object extraData = null)
        {
            CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.None);
            CrawlerStateAction action = new CrawlerStateAction(null, KeyCode.None, crawlerState, extraData: extraData);
            ChangeState(stateData, action, token);
        }

        public void ChangeState(CrawlerStateData data, CrawlerStateAction action,  CancellationToken token)
        {
            action.OnClickAction?.Invoke();
            AwaitableUtils.ForgetAwaitable(ChangeStateAsync(data, action, token));
        }

        public async Awaitable ChangeStateAsync (CrawlerStateData currData, CrawlerStateAction action, CancellationToken token)
        {

            CrawlerStateData nextStateData = null;
            foreach (CrawlerStateData stackData in _stateData)
            {
                if (stackData.Id == action.NextState)
                {
                    nextStateData = stackData;
                    break;
                }
            }

            if (nextStateData != null)
            {
                while (_stateData.Count > 0 && _stateData.Peek().Id != nextStateData.Id)
                {
                    _stateData.Pop();
                }
                if (_stateData.Count > 0)
                {
                    _stateData.Pop();
                }
            }

            if (_stateHelpers.TryGetValue(action.NextState, out IStateHelper stateHelper))
            {
                nextStateData = await stateHelper.Init(currData, action, token);

                if (stateHelper.IsTopLevelState())
                {
                    _stateData.Clear();
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
                    _stateData.Push(nextStateData);
                    _dispatcher.Dispatch(nextStateData);
                }
            }     
            else
            {
                _logService.Error("State not found: " + action.NextState);
            }
                
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

        public async Awaitable LoadParty()
        {
            _party = await _repoService.Load<PartyData>(SaveFileName);

            if (_party == null)
            {
                _party = new PartyData() { Id = SaveFileName, Seed = _rand.Next() };
                await SaveGame();
            }

            if (_party.Seed == 0)
            {
                _party.Seed = _rand.Next();
                await SaveGame();
            }

            SetupParty(_party);

            _statService.CalcPartyStats(_party, true);
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
            if (_stateData.TryPeek(out CrawlerStateData currentData))
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
            if (_stateData.TryPeek(out CrawlerStateData currentData))
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

        public void OnFinishMove(CancellationToken token)
        {
            UpdateIfNotMoving(true, token);
        }
    }
}
