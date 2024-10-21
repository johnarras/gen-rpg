using ClientEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Skills;
using Genrpg.Shared.Spells.Utils;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

internal class InputContainer
{
    public KeyCode Code;
    public KeyComm Command;
    public int MouseButton = -1;
}

public interface IInputService : IInitializable
{
    bool MouseClickNow(int index);
    float GetDeltaTime();
    bool MouseIsDown(int mouseIndex);
    Vector3 MousePosition();
    void PerformAction(int actionButtonIndex);
    bool ModifierIsActive(string keyCommand);
    string[] MoveInputsToCheck();
    void SetDisabled(bool isDisabled);
    bool GetKeyDown(char key);
    bool GetKey(char key);
}

public class InputService : IInputService
{

    private ICameraController _cameraController;
    private IPlayerManager _playerManager;
    protected IMapGenData _md;
    private IClientUpdateService _updateService;
    private IDispatcher _dispatcher;
    private IClientGameState _gs;
    private ILogService _logService;
    private IScreenService _screenService;
    private IGameData _gameData;
    private IRealtimeNetworkService _networkService;
    IClientMapObjectManager _objectManager;
    private IClientEntityService _clientEntityService;

    public async Task Initialize(CancellationToken token)
    {
        _updateService.AddUpdate(this, InputUpdate, UpdateType.Regular, token);
        _dispatcher.AddListener<MapIsLoadedEvent>(UpdateInputs, token);

        await Task.CompletedTask;
    }

    public bool GetKeyDown(char c)
    {
        char c2 = (c < 256 ? char.ToLower(c) : c);
        if (Input.GetKeyDown((KeyCode)c2))
        {
            return true;
        }
        return false;
    }

    public bool GetKey(char c)
    {
        char c2 = (c < 256 ? char.ToLower(c) : c);
        if (Input.GetKey((KeyCode)c2))
        {
            return true;
        }
        return false;
    }

    private bool _isDisabled = false;
    public void SetDisabled(bool isDisabled)
    {
        _isDisabled = isDisabled;
    }

    private bool EditingText()
    {
        return _clientEntityService.GetComponent<GInputField>(EventSystem.current.currentSelectedGameObject) != null;
    }

    private Dictionary<string, InputContainer> _stringInputs = null;
    private Dictionary<KeyCode, InputContainer> _keyCodeInputs = null;

    private List<InputContainer> _checkEachFrameInputs = null;


    public float GetDeltaTime() { return Time.deltaTime; }



    private List<String> _actionKeysTocheck = null;

    private List<int> _currActionIndexes = null;


    private void UpdateInputs(MapIsLoadedEvent worldLoaded)
    {

        SetupAbilityIndexes();
        UpdateFromInputs();
    }

    private void UpdateFromInputs ()
    {
        KeyCommData inputList = _gs.ch.Get<KeyCommData>();

        if (inputList == null || inputList.GetData().Count < 1)
        {
            return;
        }

        _stringInputs = new Dictionary<string, InputContainer>();
        _keyCodeInputs = new Dictionary<KeyCode, InputContainer>();
        _checkEachFrameInputs = new List<InputContainer>();

        foreach (KeyComm item in inputList.GetData())
        {
            if (string.IsNullOrEmpty(item.KeyPress) || string.IsNullOrEmpty(item.KeyCommand))
            {
                continue;
            }
            item.KeyPress = item.KeyPress.ToLower();
            int mouseButton = -1;
            char kc = CharCodes.None;
            if (item.KeyPress.Length == 1)
            {
                try
                {
                    kc = (char)(item.KeyPress[0]);
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "Bad KeyCode");
                    continue;
                }
            }
            else if (item.KeyPress == "space")
            {
                kc = CharCodes.Space;
            }
            else if (item.KeyPress == "esc")
            {
                kc = CharCodes.Escape;
            }
            else if (item.KeyPress == "tab")
            {
                kc = CharCodes.Tab;
            }
            else if (item.KeyPress.IndexOf("mouse") == 0)
            {
                string mouseButtonString = item.KeyPress.Replace("mouse", "");
                Int32.TryParse(mouseButtonString, out mouseButton);              
            }

            if (kc == CharCodes.None && mouseButton < 0)
            {
                continue;
            }

            if (mouseButton >= 0 && mouseButton < 6)
            {
                kc = (char)((int)(KeyCode.Mouse0) + mouseButton);
            }

            InputContainer kci = new InputContainer() { Code = (KeyCode)kc, Command = item, MouseButton = mouseButton };
            if (!_stringInputs.ContainsKey(item.KeyCommand))
            {
                _stringInputs[item.KeyCommand] = kci;
                if (item.KeyCommand.ToLower().IndexOf("screen") >= 0)
                {
                    _checkEachFrameInputs.Add(kci);
                }              
            }
        }
    }

    public bool MouseClickNow(int index)
    {
        if (index < 0 || index > 2)
        {
            return false;
        }

        return Input.GetMouseButtonDown(index);
    }

    public bool MouseIsDown(int index)
    {
        if (index < 0 || index > 2)
        {
            return false;
        }

        return Input.GetMouseButton(index);
    }
     
    public Vector3 MousePosition()
    {
        return Input.mousePosition;
    }

    public bool KeyPressNow (string keyCommand)
    {
        if (string.IsNullOrEmpty(keyCommand) || _stringInputs == null || !_stringInputs.ContainsKey(keyCommand))
        {
            return false;
        }
        if (_stringInputs[keyCommand].MouseButton >= 0)
        { 
            if (Input.GetMouseButtonDown(_stringInputs[keyCommand].MouseButton))
            {
                return true;
            }
        }

        KeyCode code = _stringInputs[keyCommand].Code;

        return Input.GetKeyDown(code);
    }

    public bool ModifierIsActive(string keyCommand)
    {
        if (keyCommand == KeyComm.ShiftName)
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
        else if (keyCommand == KeyComm.CtrlName)
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }
        else if (keyCommand == KeyComm.AltName)
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }
        return false;
    }

    public bool KeyIsDown (string keyCommand)
    {
        
        if (string.IsNullOrEmpty(keyCommand) || _stringInputs == null || !_stringInputs.ContainsKey(keyCommand))
        {
            return false;
        }

        if (_stringInputs[keyCommand].MouseButton >= 0)
        {
            if (MouseIsDown(_stringInputs[keyCommand].MouseButton) ||
                MouseClickNow(_stringInputs[keyCommand].MouseButton))
            {
                return true;
            }      
        }

        KeyCode code = _stringInputs[keyCommand].Code;

        return Input.GetKeyDown(code) || Input.GetKey(code);

    }

    int mouseLayerMask = 0;
    bool screenIsShowing = false;
    List<ActiveScreen> screens = null;

    private void InputUpdate()
    {
        if (_isDisabled)
        {
            return;
        }
        if (_stringInputs == null || _checkEachFrameInputs == null)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            if (_md.GeneratingMap)
            {
                return;
            }
        }

        GetMapMouseHit();
        
        UpdateUIInputs();
        
        UpdateMovementInputs();

        UpdateTarget(false);

        UpdateActionInputs();
        
    }

    RaycastHit hit;
    Ray ray;
    GameObject hitObject = null;
    InteractableObject interactObject = null;
    bool didHitObject = false;
    Camera mainCam = null;
    float hitObjectDistance = 0;
    GameObject playerObject = null;
    float errorDistance = 1000000;
    private void GetMapMouseHit()
    {
        if (mainCam == null)
        {
            mainCam = _cameraController.GetMainCamera();
        }

        if (mouseLayerMask == 0)
        {
            mouseLayerMask = LayerUtils.GetMask(new string[] { LayerNames.Default, LayerNames.ObjectLayer, LayerNames.UnitLayer });
        }

        if (playerObject == null)
        {
            playerObject = _playerManager.GetPlayerGameObject();
        }

        ray = mainCam.ScreenPointToRay(Input.mousePosition);

        didHitObject = Physics.Raycast(ray, out hit, MapConstants.MaxMouseRaycastDistance, mouseLayerMask);
       
        if (didHitObject && hit.transform != null)
        {
            
            hitObject = hit.transform.gameObject;

            if (playerObject != null)
            {
                hitObjectDistance = Vector3.Distance(hit.transform.position, playerObject.transform.position    );
            }
            else
            {
                hitObjectDistance = errorDistance;
            }

            // This causes garbage in the editor, but not in the built game. 0.6k per frame.
            InteractableObject newInteractObject = hitObject.GetComponent<InteractableObject>();

            // Add this for cases where the collider is nested in the prefab and 
            // the interactable object component is added to the root object.
            if (newInteractObject == null && hitObject.transform.parent != null)
            {
                newInteractObject = _clientEntityService.FindInParents<InteractableObject>(hitObject);
            }


            if (interactObject != null && newInteractObject != interactObject)
            {
                interactObject.MouseExit();
            }
            if (newInteractObject != null && newInteractObject != interactObject)
            {
                newInteractObject.MouseEnter();
            }

            if (newInteractObject != null)
            {
                if (MouseClickNow(0))
                {
                    newInteractObject.LeftMouseClick(hitObjectDistance);
                }
                else if (MouseClickNow(1))
                {
                    newInteractObject.RightMouseClick(hitObjectDistance);
                }
            }


            interactObject = newInteractObject;
        }
        else
        {
            hitObject = null;
            hitObjectDistance = errorDistance;
            if (interactObject != null)
            {
                interactObject.MouseExit();
            }
        }

    }

    private void UpdateUIInputs()
    {
        screens = null;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            screens = _screenService.GetAllScreens();
            if (screens != null && screens.Count > 0)
            {
                _screenService.CloseAll();
                if (_playerManager.GetPlayerGameObject() != null)
                {
                    return;
                }
            }
        }

        if (!EditingText())
        {
            foreach (InputContainer kci in _checkEachFrameInputs)
            {
                if (Input.GetKeyDown(kci.Code))
                {
                    screenIsShowing = false;
                    if (screens == null)
                    {
                        screens = _screenService.GetAllScreens();
                    }
                    foreach (ActiveScreen obj in screens)
                    {
                        ActiveScreen ssi = obj as ActiveScreen;
                        if (ssi == null)
                        {
                            continue;
                        }

                        if (_screenService.GetFullScreenNameFromEnum(ssi.ScreenId) == kci.Command.KeyCommand)
                        {
                            _screenService.Close(ssi.ScreenId);
                            screenIsShowing = true;
                            break;
                        }
                    }

                    if (!screenIsShowing)
                    {
                        _screenService.StringOpen(kci.Command.KeyCommand);
                    }
                }
            }
        }
        else
        {

        }
    }

    public string[] MoveInputsToCheck()
    {
        return _moveInputsToCheck;
    }
   
    private string[] _moveInputsToCheck = new string[]
    {
        KeyComm.Forward,
        KeyComm.Backward,
        KeyComm.TurnLeft,
        KeyComm.TurnRight,
        KeyComm.StrafeLeft,
        KeyComm.StrafeRight,
        KeyComm.Jump
    };

    private void UpdateMovementInputs()
    {
        if (!_playerManager.Exists())
        {
            return;
        }
        if (EditingText())
        {
            return;
        }

        foreach (string kc in _moveInputsToCheck)
        {
            _playerManager.SetKeyPercent(kc, KeyIsDown(kc) ? 1 : 0);
        }
        if (MouseIsDown(0) && MouseIsDown(1))
        {
            _playerManager.SetKeyPercent(KeyComm.Forward, 1.0f);
        }
    }

    private void UpdateTarget(bool forceTarget)
    {
        if (KeyPressNow(KeyComm.TargetNext) || forceTarget)
        {
            _playerManager.TargetNext();
        }
    }

    private void SetupAbilityIndexes()
    { 
        _currActionIndexes = new List<int>();
    
        _actionKeysTocheck = new List<string>();

        for (int i = InputConstants.MinActionIndex; i <= InputConstants.MaxActionIndex; i++)
        {
            _actionKeysTocheck.Add(KeyComm.ActionPrefix + i);
        }
    }

    private void UpdateActionInputs()
    {

        _currActionIndexes.Clear();

        if (!_playerManager.Exists())
        {
            return;
        }

        if (_screenService.GetLayerScreen(ScreenLayers.Screens) != null)
        {
            return;
        }

        for (int k = 0; k < _actionKeysTocheck.Count; k++)
        {
            if (KeyPressNow(_actionKeysTocheck[k]))
            {
                _currActionIndexes.Add(k + 1);
            }
        }

        if (_currActionIndexes.Count > 0)
        {
            for (int i = 0; i < _currActionIndexes.Count; i++)
            {
                PerformAction(_currActionIndexes[i]);
            }
        }
    }

    private DateTime _lastActionTime = DateTime.UtcNow;
    public void PerformAction (int actionIndex)
    {
        if ((DateTime.UtcNow-_lastActionTime).TotalSeconds < 0.5f)
        {
            return;
        }
        ActionInputData actionInputs = _gs.ch.Get<ActionInputData>();
        ActionInput actionKey = actionInputs.GetInput(actionIndex);
        if (actionKey == null || actionKey.SpellId == 0)
        {
            return;
        }

        Spell spell = _gs.ch.Get<SpellData>().Get(actionKey.SpellId);

        if (spell == null)
        {
            return;
        }
       
        if (!_playerManager.TryGetUnit(out Unit playerUnit))
        {
            return;
        }

        SkillType skillType = _gameData.Get<SkillTypeSettings>(_gs.ch).Get(spell.Effects.FirstOrDefault()?.SkillTypeId ?? 0);
        if (!_objectManager.GetUnit(playerUnit.TargetId, out Unit target))
        {
            if (skillType.TargetTypeId == TargetTypes.Ally)
            {
                target = playerUnit;
            }
            else
            {
                return;
            }
        }

        if (!SpellUtils.IsValidTarget(target, playerUnit.FactionTypeId, skillType.TargetTypeId))
        {
            if (skillType.TargetTypeId == TargetTypes.Ally)
            {
                target = playerUnit;
            }
            else
            {
                return;
            }
        }
 

        if (target == null)
        {
            UpdateTarget(true);
            return;
        }
        CastSpell castSpell = new CastSpell()
        {
            SpellId = spell.IdKey,
            TargetId = target.Id,
        };
        _networkService.SendMapMessage(castSpell);
        _lastActionTime = DateTime.UtcNow;
    }
}