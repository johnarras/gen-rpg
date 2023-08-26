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
using Entities;
using Genrpg.Shared.Utils;
using System.Threading;

public class DragItemInitData<TData,TDragItem,TScreen,TInitData> 
    where TData : class
    where TScreen : DragItemScreen<TData,TDragItem,TScreen,TInitData>
    where TDragItem : DragItem<TData,TDragItem,TScreen,TInitData>
    where TInitData : DragItemInitData<TData, TDragItem, TScreen, TInitData>
{
    public TData Data;
    public TScreen Screen;
    public int Flags;
}


public abstract class DragItem<TData,TDragItem,TScreen,TInitData> : BaseBehaviour,
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerClickHandler
    where TData : class
    where TDragItem : DragItem<TData,TDragItem,TScreen,TInitData>
    where TScreen : DragItemScreen<TData,TDragItem,TScreen,TInitData>
    where TInitData : DragItemInitData<TData,TDragItem,TScreen,TInitData>
{

    [SerializeField]
    protected Button _selfButton;

    protected TInitData _initData;
    protected CancellationToken _token;
    public virtual void Init(TInitData initData, CancellationToken token)
    {
        _initData = initData;
        _token = token;
    }

    protected bool _canDrag = true;
    public virtual bool CanDrag()
    {
        return _canDrag;
    }

    public TInitData GetInitData()
    {
        return _initData;
    }

    public TScreen GetScreen()
    {
        return _initData?.Screen;
    }

    public TData GetDataItem()
    {
        return _initData?.Data;
    }

    public void SetDataItem(TData item)
    {
        if (_initData != null)
        {
            _initData.Data = item;
        }
    }

    public void AddFlags(int flags)
    {
        if (_initData != null)
        {
            _initData.Flags |= flags;
        }
    }

    public void RemoveFlags (int flags)
    {
        if (_initData != null)
        {
            _initData.Flags &= ~flags;
        }
    }

    public bool HasFlag(int flag)
    {
        if (_initData != null)
        {
            return ((_initData.Flags & flag) != 0);
        }
        return false;
    }

    protected virtual bool ShowTooltipOnLeft()
    {
        return (_initData == null || !FlagUtils.IsSet(_initData.Flags, ItemIconFlags.ShowTooltipOnRight));
    }


    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!CanDrag() || _initData == null || _initData.Screen == null)
        {
            return;
        }

        if (InputService.Instance.MouseClickNow(1))
        {
            return;
        }

        HideTooltip();
        _initData.Screen.OnPointerDown(eventData, this as TDragItem);
        SetAsDragItem(true);
    }


    public virtual void OnEndDrag()
    {
        SetAsDragItem(false);
    }

    protected virtual void SetAsDragItem(bool isDragItem)
    {
        ScrollRect scrollRect = GameObjectUtils.FindInParents<ScrollRect>(gameObject);
        if (scrollRect != null)
        {
            scrollRect.enabled = !enabled;
        }

        List<Image> allImages = GameObjectUtils.GetComponents<Image>(gameObject);
        foreach (Image image in allImages)
        {
            if (isDragItem)
            {
                image.color = Color.gray;
            }
            else
            {
                image.color = Color.white;
            }
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {

        if (_initData == null || _initData.Screen == null)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _initData.Screen.OnRightClickIcon(_gs, this as TDragItem);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            _initData.Screen.OnLeftClickIcon(_gs, this as TDragItem);
        }
    }

    public abstract void ShowTooltip();
    public abstract void HideTooltip();

    protected virtual void UpdateTooltipPosition()
    {
        if (_initData.Screen == null || _initData.Screen.ToolTip == null)
        {
            return;
        }

        //Tooltip.transform.position = transform.position;

        RectTransform transTooltip = _initData.Screen.ToolTip.GetComponent<RectTransform>();
        RectTransform transIcon = GetComponent<RectTransform>();

        if (transTooltip == null || transIcon == null)
        {
            return;
        }

        Rect trect = transTooltip.rect;
        Rect irect = transIcon.rect;



        float dy = -trect.height / 4;

        float dx = 0;

        float xpad = irect.width / 2;

        if (ShowTooltipOnLeft()) // Move to right.
        {
            dx = -trect.width / 2 - xpad;
        }
        else
        {
            dx = trect.width / 2 + xpad;
        }

        transTooltip.position = transIcon.position;
        transTooltip.localPosition += new Vector3(dx, dy, 0);
    }

  


}
