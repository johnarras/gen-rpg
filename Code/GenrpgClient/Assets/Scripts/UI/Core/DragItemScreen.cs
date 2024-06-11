using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using UnityEngine.UI; // FIX
using UnityEngine.EventSystems;

using System.Threading;
using UnityEngine;

public abstract class DragItemScreen<TData,TDragItem,TScreen,TInitData> : BaseScreen 
    where TData : class 
    where TDragItem : DragItem<TData,TDragItem,TScreen,TInitData>
    where TScreen : DragItemScreen<TData,TDragItem,TScreen,TInitData>
    where TInitData : DragItemInitData<TData, TDragItem, TScreen, TInitData>
{

    protected IInputService _inputService;

    public BaseTooltip ToolTip;
    public MoneyDisplay _playerMoney;

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        if (ToolTip != null)
        {
            GEntityUtils.InitializeHierarchy(_gs, ToolTip.entity());
            GEntityUtils.SetActive(ToolTip.entity(), false);
        }
        
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
            _dragItem.transform().position = GVector3.Create(_inputService.MousePosition());
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
        GEntity dragParent = _screenService.GetDragParent() as GEntity;

        if (dragParent == null)
        {
            return;
        }

        ResetCurrentDragItem();
        _origItem = icon;
        _dragItem = GEntityUtils.FullInstantiate<TDragItem>(_gs, icon);
        _dragItem.Init(icon.GetInitData(), _token);
        _dragItem.transform().SetParent(dragParent.transform());
        _dragItem.transform().localScale = icon.transform().lossyScale;
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

        GEntityUtils.Destroy(_dragItem.entity());
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
        pointerData.position = GVector3.Create(_inputService.MousePosition());

        foreach (GraphicRaycaster gr in raycasters)
        {
            List<RaycastResult> results = new List<RaycastResult>();

            gr.Raycast(pointerData, results);

            GEntity lastTargetHit = null;
            TDragItem dropTargetIcon = null;

            if (results.Count > 0)
            {
                foreach (RaycastResult res in results)
                {
                    TDragItem iconParent = GEntityUtils.FindInParents<TDragItem>(res.entity());
                    if (dropTargetIcon == null && iconParent != null && iconParent != _dragItem)
                    {
                        dropTargetIcon = iconParent;
                    }
                }
                if (results.Count > 0)
                {
                    lastTargetHit = results[results.Count - 1].entity();
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
    protected virtual void HandleDragDrop(TScreen startScreen, TDragItem dragIcon, TDragItem otherIconHit, GEntity finalObjectHit)
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
            _dragItem.transform().position = GVector3.Create(_inputService.MousePosition());
        }
    }
}