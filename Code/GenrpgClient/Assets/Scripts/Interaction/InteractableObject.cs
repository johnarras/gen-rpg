using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Threading;
using Cysharp.Threading.Tasks;

public class InteractableObject : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    public const int NotUsed = 0;
    public const int InUse = 1;
    public const int DidUse = 2;

    protected MapObject _mapObj;
    protected GameObject _gameObject;
    public const string InteractGlow = "InteractGlow";
    protected GameObject GlowItem = null;
    CancellationToken _token;
    private void OnEnable()
    {
        InnerOnEnable();
    }

    public virtual void Init(MapObject worldObj, GameObject go, CancellationToken token)
    {
        _mapObj = worldObj;
        _gameObject = go;
        _token = token;
    }

    protected virtual void InnerOnEnable()
    {
        ShowGlow(0);
    }

    public void HideGlow(float delay, bool destroyAtEnd)
    {
        DelayHideGlow(delay, destroyAtEnd, _token).Forget();
    }

    private async UniTask DelayHideGlow(float delay, bool destroyAtEnd, CancellationToken token)
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

        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

        if (GlowItem != null)
        {
            GameObject.Destroy(GlowItem);
        }

        if (destroyAtEnd)
        {
            GameObject.Destroy(gameObject);
        }
    }

    public void ShowGlow(float delay)
    {
        DelayShowGlow(delay, _token).Forget();
    }

    private async UniTask DelayShowGlow(float delay, CancellationToken token)
    {
        if (GlowItem != null)
        {
            return;
        }
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
        _assetService.LoadAssetInto(_gs, gameObject, AssetCategory.UI, InteractGlow, OnLoadGlow, null, token);
    }

    private void OnLoadGlow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject glow = obj as GameObject;
        if (glow == null)
        {
            return;
        }

        glow.transform.localPosition = new Vector3(0, 1, 0);
        GlowItem = glow;
    }


    public virtual bool CanInteract()
    {
        GameObject go = PlayerObject.Get();
        if (go == null)
        {
            return false;
        }
        
        if (_mapObj == null || _mapObj.IsDeleted())
        {
            return false;
        }
        

        float dist = Vector3.Distance(go.transform.position, transform.position);
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
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _LeftClick(0);
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            _MiddleClick(0);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _RightClick(0);
        }
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