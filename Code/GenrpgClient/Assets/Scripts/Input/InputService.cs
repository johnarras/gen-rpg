
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

using ClientEvents;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Spells.Entities;
using UI.Screens.Utils;
using UI;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Genrpg.Shared.Spells.Messages;
using UnityEngine; // Needed
using Genrpg.Shared.Spells.Constants;
using UI.Screens.Constants;

internal class InputContainer
{
    public KeyCode Code;
    public KeyComm Command;
    public int MouseButton = -1;
}

public interface IInputService : ISetupService
{
    bool EnteringChat();
    void ToggleChat();
    bool MouseClickNow(int index);
}

public class InputService : BaseBehaviour, IInputService
{

    private static InputService _instance = null;
    public static InputService Instance { get { return _instance; } }

    IClientMapObjectManager _objectManager;
    IScreenService _screenService;

    public async Task Setup(GameState gs, CancellationToken token)
    {
        _instance = this;
        AddUpdate(InputUpdate, UpdateType.Regular);
        _gs.AddEvent<MapIsLoadedEvent>(this, UpdateInputs);
        await Task.CompletedTask;
    }

    private bool _editingChat = false;

    public bool EnteringChat()
    {
        return _editingChat;
    }

    public void ToggleChat()
    {
        if (_editingChat)
        {
            ChatWindow.Instance?.SendChat();
        }
        _editingChat = !_editingChat;
        ChatWindow.Instance?.SetEditing(_editingChat);
    }

    private Dictionary<string, InputContainer> _stringInputs = null;
    private Dictionary<KeyCode, InputContainer> _keyCodeInputs = null;

    private List<InputContainer> _checkEachFrameInputs = null;


    public float GetDeltaTime() { return Time.deltaTime; }



    private List<String> _actionKeysTocheck = null;

    private List<int> _currActionIndexes = null;


    private MapIsLoadedEvent UpdateInputs(UnityGameState gs, MapIsLoadedEvent worldLoaded)
    {

        SetupAbilityIndexes();
        UpdateFromInputs(gs);
        return null;
    }

    private void UpdateFromInputs (UnityGameState gs)
    {
        KeyCommData inputList = gs.ch.Get<KeyCommData>();

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
            KeyCode kc = KeyCode.None;
            if (item.KeyPress.Length == 1)
            {
                try
                {
                    kc = (KeyCode)(item.KeyPress[0]);
                }
                catch (Exception e)
                {
                    _gs.logger.Exception(e, "Bad KeyCode");
                    continue;
                }
            }
            else if (item.KeyPress == "space")
            {
                kc = KeyCode.Space;
            }
            else if (item.KeyPress == "esc")
            {
                kc = KeyCode.Escape;
            }
            else if (item.KeyPress == "tab")
            {
                kc = KeyCode.Tab;
            }
            else if (item.KeyPress.IndexOf("mouse") == 0)
            {
                string mouseButtonString = item.KeyPress.Replace("mouse", "");
                Int32.TryParse(mouseButtonString, out mouseButton);              
            }

            if (kc == KeyCode.None && mouseButton < 0)
            {
                continue;
            }

            if (mouseButton >= 0 && mouseButton < 6)
            {
                kc = (KeyCode)((int)(KeyCode.Mouse0) + mouseButton);
            }

            InputContainer kci = new InputContainer() { Code = kc, Command = item, MouseButton = mouseButton };
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
     
    public GVector3 MousePosition()
    {
        return GVector3.Create(Input.mousePosition);
    }

    public bool KeyPressNow (UnityGameState gs, string keyCommand)
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

    public bool ModifierIsActive(UnityGameState gs, string keyCommand)
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

    public bool KeyIsDown (UnityGameState gs, string keyCommand)
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
        if (_stringInputs == null || _checkEachFrameInputs == null)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gs.md == null || _gs.md.GeneratingMap)
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
    GEntity hitObject = null;
    InteractableObject interactObject = null;
    bool didHitObject = false;
    Camera mainCam = null;
    float hitObjectDistance = 0;
    GEntity playerObject = null;
    float errorDistance = 1000000;
    private void GetMapMouseHit()
    {
        if (mainCam == null)
        {
            mainCam = CameraController.Instance.GetMainCamera();
        }

        if (mouseLayerMask == 0)
        {
            mouseLayerMask = LayerUtils.GetMask(new string[] { LayerNames.Default, LayerNames.ObjectLayer, LayerNames.UnitLayer });
        }

        if (playerObject == null)
        {
            playerObject = PlayerObject.Get();
        }

        ray = mainCam.ScreenPointToRay(Input.mousePosition);

        didHitObject = GPhysics.Raycast(ray, out hit, MapConstants.MaxMouseRaycastDistance, mouseLayerMask);
       
        if (didHitObject && hit.transform() != null)
        {
            
            hitObject = hit.transform().entity();

            if (playerObject != null)
            {
                hitObjectDistance = GVector3.Distance(GVector3.Create(hit.transform().position), GVector3.Create(playerObject.transform().position));
            }
            else
            {
                hitObjectDistance = errorDistance;
            }

            // This causes garbage in the editor, but not in the built game. 0.6k per frame.
            InteractableObject newInteractObject = hitObject.GetComponent<InteractableObject>();

            // Add this for cases where the collider is nested in the prefab and 
            // the interactable object component is added to the root object.
            if (newInteractObject == null && hitObject.transform().parent != null)
            {
                newInteractObject = GEntityUtils.FindInParents<InteractableObject>(hitObject);
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
            screens = _screenService.GetAllScreens(_gs);
            if (screens != null && screens.Count > 0)
            {
                _screenService.CloseAll(_gs);
                if (PlayerObject.Get() != null)
                {
                    return;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleChat();
        }

        if (!EnteringChat())
        {
            foreach (InputContainer kci in _checkEachFrameInputs)
            {
                if (Input.GetKeyDown(kci.Code))
                {
                    screenIsShowing = false;
                    if (screens == null)
                    {
                        screens = _screenService.GetAllScreens(_gs);
                    }
                    foreach (ActiveScreen obj in screens)
                    {
                        ActiveScreen ssi = obj as ActiveScreen;
                        if (ssi == null)
                        {
                            continue;
                        }

                        if (ScreenUtils.GetPrefabName(ssi.ScreenId) == kci.Command.KeyCommand)
                        {
                            _screenService.Close(_gs, ssi.ScreenId);
                            screenIsShowing = true;
                            break;
                        }
                    }

                    if (!screenIsShowing)
                    {
                        _screenService.StringOpen(_gs, kci.Command.KeyCommand);
                    }
                }
            }
        }
        else
        {

        }
    }

    public static readonly string[] MoveInputsToCheck = 
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
        if (PlayerController.Instance == null)
        {
            return;
        }

        if (EnteringChat())
        {
            return;
        }

        foreach (string kc in MoveInputsToCheck)
        {
            PlayerController.Instance.SetKeyPercent(kc, KeyIsDown(_gs, kc) ? 1 : 0);
        }
        if (MouseIsDown(0) && MouseIsDown(1))
        {
            PlayerController.Instance.SetKeyPercent(KeyComm.Forward, 1.0f);
        }
    }

    private void UpdateTarget(bool forceTarget)
    {
        if (KeyPressNow(_gs, KeyComm.TargetNext) || forceTarget)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.TargetNext(_gs);
            }
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

        if (PlayerController.Instance == null || PlayerController.Instance.GetUnit() == null)
        {
            return;
        }

        if (_screenService.GetLayerScreen(_gs, ScreenLayers.Screens) != null)
        {
            return;
        }

        for (int k = 0; k < _actionKeysTocheck.Count; k++)
        {
            if (KeyPressNow(_gs, _actionKeysTocheck[k]))
            {
                _currActionIndexes.Add(k + 1);
            }
        }

        if (_currActionIndexes.Count > 0)
        {
            for (int i = 0; i < _currActionIndexes.Count; i++)
            {
                PerformAction(_gs, _currActionIndexes[i]);
            }
        }
    }

    private DateTime _lastActionTime = DateTime.UtcNow;
    public void PerformAction (UnityGameState gs, int actionIndex)
    {
        if ((DateTime.UtcNow-_lastActionTime).TotalSeconds < 0.5f)
        {
            return;
        }
        ActionInputData actionInputs = gs.ch.Get<ActionInputData>();
        ActionInput actionKey = actionInputs.GetInput(actionIndex);
        if (actionKey == null || actionKey.SpellId == 0)
        {
            return;
        }

        Spell spell = gs.ch.Get<SpellData>().Get(actionKey.SpellId);

        Unit MapUnit = PlayerController.Instance.GetUnit();

        if (spell == null)
        {
            return;
        }



        SkillType skillType = gs.data.GetGameData<SkillTypeSettings>(_gs.ch).GetSkillType(spell.Effects.FirstOrDefault()?.SkillTypeId ?? 0);
        if (!_objectManager.GetUnit(MapUnit.TargetId, out Unit target))
        {
            if (skillType.TargetTypeId == TargetTypes.Ally)
            {
                target = MapUnit;
            }
            else
            {
                return;
            }
        }

        if (!SpellUtils.IsValidTarget(gs, target, MapUnit.FactionTypeId, skillType.TargetTypeId))
        {
            if (skillType.TargetTypeId == TargetTypes.Ally)
            {
                target = MapUnit;
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