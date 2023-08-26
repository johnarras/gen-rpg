using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class DragItemScreen<TData,TDragItem,TScreen,TInitData> : BaseScreen 
    where TData : class 
    where TDragItem : DragItem<TData,TDragItem,TScreen,TInitData>
    where TScreen : DragItemScreen<TData,TDragItem,TScreen,TInitData>
    where TInitData : DragItemInitData<TData, TDragItem, TScreen, TInitData>
{

    public BaseTooltip ToolTip;
    [SerializeField]
    private MoneyDisplay _playerMoney;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        if (ToolTip != null)
        {
            GameObjectUtils.InitializeHierarchy(_gs, ToolTip.gameObject);
        }
        await UniTask.CompletedTask;
    }

    protected override void OnDisable()
    {
        ResetCurrentDragItem();
        base.OnDisable();
    }


    public virtual void OnRightClickIcon(UnityGameState gs, TDragItem icon) { }
    public virtual void OnLeftClickIcon(UnityGameState gs, TDragItem icon) { }

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
            _dragItem.transform.position = InputService.Instance.MousePosition();
            if (!InputService.Instance.MouseIsDown(0))
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
        _dragItem = GameObjectUtils.FullInstantiate<TDragItem>(_gs, icon);
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

        GameObject.Destroy(_dragItem.gameObject);
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
        pointerData.position = InputService.Instance.MousePosition();

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
                    TDragItem iconParent = GameObjectUtils.FindInParents<TDragItem>(res.gameObject);
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
            _dragItem.transform.position = InputService.Instance.MousePosition();
        }
    }

}