using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using ClientEvents;
using UI.Screens.Constants;
using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class BaseScreen : AnimatorBehaviour, IScreen
{
    public string Name { get; set; }
    public ScreenId ScreenId { get; set; }
    public float IntroTime;
    public float OutroTime;

    protected object _openData;

    private static List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();


    private CancellationTokenSource _screenSource = new CancellationTokenSource();
    protected CancellationToken _token;

    // Called when screen first opens.
    protected abstract UniTask OnStartOpen(object data, CancellationToken token);


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(ScreenUpdate, UpdateType.Regular);
    }

    protected virtual void OnEnable()
    {
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null)
        {
            _raycasters.Add(gr);
        }
    }

    protected virtual void OnDisable()
    {
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null)
        {
            _raycasters.Remove(gr);
        }
    }

    virtual public string GetAnalyticsName()
    {
        return ScreenId.ToString();
    }

    protected List<GraphicRaycaster> GetAllRaycasters()
    {
        return _raycasters;
    }

    public virtual async UniTask StartOpen(object data, CancellationToken token)
    {
        _screenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _screenSource.Token;
        _openData = data;
        await OnStartOpen(_openData, token);

        if (IntroTime > 0)
        {
            TriggerAnimation(AnimParams.Intro, IntroTime, OnFinishOpen);
        }
        else
        {
            OnFinishOpen();
        }
    }

    // Called as the screen finishes opening.
    protected virtual void OnFinishOpen()
    {
    }

    protected virtual void ScreenUpdate()
    {

    }

    public virtual void OnInfoChanged()
    {

    }

    public virtual void OnReset()
    {

    }

    public virtual bool BlockMouse()
    {
        return true;
    }

    public virtual void ErrorClose(string txt)
    {
        if (!string.IsNullOrEmpty(txt))
        {
            _gs.logger.Message("Error on close: " + txt);
        }

        StartClose();
    }


    public virtual void StartClose()
    {
        OnStartClose();
        if (OutroTime > 0)
        {
            TriggerAnimation(AnimParams.Outro, OutroTime, OnFinishClose);
        }
        else
        {
            OnFinishClose();
        }
    }

    // Called immediately on start close.
    protected virtual void OnStartClose()
    {

    }

    // Called after close animation ends.
    protected virtual void OnFinishClose()
    {
        _screenService.FinishClose(_gs, ScreenId);
    }
}



