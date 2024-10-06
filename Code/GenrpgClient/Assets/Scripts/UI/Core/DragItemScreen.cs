using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;

using System.Threading;
using System.Threading.Tasks;

public abstract class DragItemScreen<TData,TDragItem,TScreen,TInitData> : BaseScreen 
    where TData : class 
    where TDragItem : DragItem<TData,TDragItem,TScreen,TInitData>
    where TScreen : DragItemScreen<TData,TDragItem,TScreen,TInitData>
    where TInitData : DragItemInitData<TData, TDragItem, TScreen, TInitData>
{

    protected IInputService _inputService;

    public BaseTooltip ToolTip;
    public MoneyDisplay _playerMoney;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        if (ToolTip != null)
        {
            _gameObjectService.InitializeHierarchy(ToolTip.gameObject);
            _gameObjectService.SetActive(ToolTip.gameObject, false);
        }

        await Task.CompletedTask;
    }

    protected override void OnDisable()
    {
        ResetCurrentDragItem();
        base.OnDisable();
    }


    public virtual void OnRightClickIcon(TDragItem icon) { }
    public virtual void OnLeftClickIcon(TDragItem icon) { }

    protected TDragItem _origItem;
    protected TDragItem _dragItem;
    public TDragItem GetDragItem()
    {
        return _dragItem;
    }

    protected override void ScreenUpdate()
    {
        OnUpdate();
        base.ScreenUpdate();
    }

    protected virtual void OnUpdate()
    {

        if (_dragItem != null)
        {
            _dragItem.transform.position = _inputService.MousePosition();
            if (!_inputService.MouseIsDown(0))
            {
                OnPointerUp();
            }
        }

    }  

    public virtual void OnPointerDown(PointerEventData pointerData, TDragItem icon)
    {
        if (icon == null)
        {
            return;
        }

        if (icon == _dragItem)
        {
            return;
        }
        GameObject dragParent = _screenService.GetDragParent() as GameObject;

        if (dragParent == null)
        {
            return;
        }

        ResetCurrentDragItem();
        _origItem = icon;
        _dragItem = _gameObjectService.FullInstantiate<TDragItem>(icon);
        _dragItem.Init(icon.GetInitData(), _token);
        _dragItem.transform.SetParent(dragParent.transform);
        _dragItem.transform.localScale = icon.transform.lossyScale;
        UpdateDragIconPosition();
        ShowDragTargetIconsGlow(true);

    }

    protected void ResetCurrentDragItem()
    {
        if (_origItem != null)
        {
            _origItem.OnEndDrag();
        }

        if (_dragItem == null)
        {
            return;
        }

        _gameObjectService.Destroy(_dragItem.gameObject);
        _dragItem = null;
        _origItem = null;
        ShowDragTargetIconsGlow(false);
    }



    public virtual void OnPointerUp()
    {
        if (_dragItem == null || _origItem == null)
        {
            return;
        }

        List<GraphicRaycaster> raycasters = GetAllRaycasters();

        bool didHitItem = false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = _inputService.MousePosition();

        foreach (GraphicRaycaster gr in raycasters)
        {
            List<RaycastResult> results = new List<RaycastResult>();

            gr.Raycast(pointerData, results);

            GameObject lastTargetHit = null;
            TDragItem dropTargetIcon = null;

            if (results.Count > 0)
            {
                foreach (RaycastResult res in results)
                {
                    TDragItem iconParent = _gameObjectService.FindInParents<TDragItem>(res.gameObject);
                    if (dropTargetIcon == null && iconParent != null && iconParent != _dragItem)
                    {
                        dropTargetIcon = iconParent;
                    }
                }
                if (results.Count > 0)
                {
                    lastTargetHit = results[results.Count - 1].gameObject;
                }

                if (dropTargetIcon != null && dropTargetIcon.GetScreen() != null)
                {
                    dropTargetIcon.GetScreen().HandleDragDrop(this as TScreen, _dragItem, dropTargetIcon, lastTargetHit);
                }
                else
                {
                    HandleDragDrop(this as TScreen, _dragItem, dropTargetIcon, lastTargetHit);
                }
                didHitItem = true;
            }

            if (didHitItem)
            {
                break;
            }
        }

        if (!didHitItem)
        {
            HandleDragDrop(this as TScreen, _dragItem, null, null);
        }


        ResetCurrentDragItem();
    }
    protected virtual void HandleDragDrop(TScreen startScreen, TDragItem dragIcon, TDragItem otherIconHit, GameObject finalObjectHit)
    {
        // _dragIcon and _origIcon should already exist here!
    }

    protected virtual void ShowDragTargetIconsGlow(bool visible)
    {

    }

    protected virtual void UpdateDragIconPosition()
    {
        if (_dragItem != null)
        {
            _dragItem.transform.position = _inputService.MousePosition();
        }
    }
}