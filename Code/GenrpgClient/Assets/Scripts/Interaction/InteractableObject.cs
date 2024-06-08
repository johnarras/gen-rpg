using System;
using GEntity = UnityEngine.GameObject;
using UnityEngine.EventSystems;

using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Threading;
using UnityEngine; // Needed

public class InteractableObject : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    public const int NotUsed = 0;
    public const int InUse = 1;
    public const int DidUse = 2;

    protected MapObject _mapObj;
    protected GEntity _entity;
    public const string InteractGlow = "InteractGlow";
    protected GEntity GlowItem = null;
    CancellationToken _token;
    protected IPlayerManager _playerManager;

    public virtual void Init(MapObject worldObj, GEntity go, CancellationToken token)
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
        _updateService.AddDelayedUpdate(entity, (_token) => { DelayHideGlow(destroyAtEnd); }, _token, delay);
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
            GEntityUtils.Destroy(GlowItem);
            GlowItem = null;
        }

        if (destroyAtEnd)
        {
            GEntityUtils.Destroy(entity);
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
        _updateService.AddDelayedUpdate(entity, DelayShowGlow, _token, delay);
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
        GEntity glow = obj as GEntity;
        if (glow == null)
        {
            return;
        }

        if (glow.transform.parent == null)
        {
            GEntityUtils.Destroy(glow);
            return;
        }

        glow.transform().localPosition = GVector3.Create(0, 1, 0);
        GlowItem = glow;
    }


    public virtual bool CanInteract()
    {
        GEntity go = _playerManager.GetEntity();
        if (go == null)
        {
            return false;
        }
        
        if (_mapObj == null || _mapObj.IsDeleted())
        {
            return false;
        }
        

        float dist = GVector3.Distance(GVector3.Create(go.transform().position), GVector3.Create(entity.transform().position));
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
        Cursors.SetCursor(Cursors.Interact);
    }

    protected virtual void _OnPointerExit()
    {
        Cursors.SetCursor(Cursors.Default);
    }

    protected virtual void _LeftClick(float distance) { }
    protected virtual void _MiddleClick(float distance) { }
    protected virtual void _RightClick(float distance) { }

    protected virtual void _LeftDown(float distance) { }
    protected virtual void _MiddleDown(float distance) { }
    protected virtual void _RightDown(float distance) { }


}