using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Genrpg.Shared.MapObjects.Entities;
using System.Threading;
using Genrpg.Shared.Client.Assets.Constants;

public class InteractableObject : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    public const int NotUsed = 0;
    public const int InUse = 1;
    public const int DidUse = 2;

    protected MapObject _mapObj;
    protected GameObject _entity;
    public const string InteractGlow = "InteractGlow";
    protected GameObject GlowItem = null;
    CancellationToken _token;
    protected IPlayerManager _playerManager;
    protected ICursorService _cursorService;

    public virtual void Init(MapObject worldObj, GameObject go, CancellationToken token)
    {
        _mapObj = worldObj;
        _entity = go;
        _token = token;
        OnInit();
    }

    virtual protected void OnInit()
    {
        ShowGlow(0);
    }

    public void HideGlow(float delay, bool destroyAtEnd)
    {
        AddDelayedUpdate((_token) => { DelayHideGlow(destroyAtEnd); }, delay);
    }

    private void DelayHideGlow(bool destroyAtEnd)
    {
        if (GlowItem != null)
        {
            ParticleSystem pc = GlowItem.GetComponent<ParticleSystem>();

            if (pc != null)
            {
                ParticleSystem.EmissionModule emissionModule = pc.emission;
                emissionModule.rateOverTime = 0;
            }
        }

        if (GlowItem != null)
        {
            _clientEntityService.Destroy(GlowItem);
            GlowItem = null;
        }

        if (destroyAtEnd)
        {
            _clientEntityService.Destroy(entity);
        }
        _showingGlow = false;
    }

    private bool _showingGlow = false;
    public void ShowGlow(float delay)
    {
        if (_showingGlow)
        {
            return;
        }
        _showingGlow = true;
        AddDelayedUpdate(DelayShowGlow, delay);
    }

    private void DelayShowGlow(CancellationToken token)
    {
        if (GlowItem != null)
        {
            return;
        }
        _assetService.LoadAssetInto(entity, AssetCategoryNames.UI,
            InteractGlow, OnLoadGlow, null, token, "Core");
    }

    private void OnLoadGlow(object obj, object data, CancellationToken token)
    {
        GameObject glow = obj as GameObject;
        if (glow == null)
        {
            return;
        }

        if (glow.transform.parent == null)
        {
            _clientEntityService.Destroy(glow);
            return;
        }

        glow.transform.localPosition = new Vector3(0, 1, 0);
        GlowItem = glow;
    }


    public virtual bool CanInteract()
    {
        GameObject go = _playerManager.GetPlayerGameObject();
        if (go == null)
        {
            return false;
        }
        
        if (_mapObj == null || _mapObj.IsDeleted())
        {
            return false;
        }
        

        float dist = Vector3.Distance(go.transform.position, entity.transform.position);
        return dist < MapConstants.MaxInteractDistance;
    }

    public void MouseEnter()
    {
        _OnPointerEnter();    
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _OnPointerEnter();
    }

    public void MouseExit()
    {
        _OnPointerExit();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _OnPointerExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _LeftDown(0);
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            _MiddleDown(0);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _RightDown(0);
        }
    }

    public void LeftMouseClick(float distance)
    {
        _LeftClick(distance);
    }

    public void RightMouseClick(float distance)
    {
        _RightClick(distance);
    }

    public void OnPointerClick(PointerEventData eventData)
    { 
    }

    protected virtual void _OnPointerEnter()
    {
        _cursorService.SetCursor(CursorNames.Interact);
    }

    protected virtual void _OnPointerExit()
    {
        _cursorService.SetCursor(CursorNames.Default);
    }

    protected virtual void _LeftClick(float distance) { }
    protected virtual void _MiddleClick(float distance) { }
    protected virtual void _RightClick(float distance) { }

    protected virtual void _LeftDown(float distance) { }
    protected virtual void _MiddleDown(float distance) { }
    protected virtual void _RightDown(float distance) { }


}