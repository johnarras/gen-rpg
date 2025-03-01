﻿using System.Collections.Generic;

using System.Threading;
using UnityEngine.UI; // FIX
using UnityEngine;
using Genrpg.Shared.UI.Entities;
using System.Threading.Tasks;
using Assets.Scripts.Awaitables;

public abstract class BaseScreen : AnimatorBehaviour, IScreen
{
    public ScreenId ScreenID { get; set; }
    public string Subdirectory { get; set; }
    public float IntroTime;
    public float OutroTime;

    protected object _openData;

    private static List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();

    private CancellationTokenSource _screenSource = new CancellationTokenSource();
    protected CancellationToken _token;

    protected IAwaitableService _awaitableService;

    // Called when screen first opens.
    protected abstract Task OnStartOpen(object data, CancellationToken token);


    public override void Init()
    {
        base.Init();
        AddUpdate(ScreenUpdate, UpdateTypes.Regular);
    }

    protected virtual void OnEnable()
    {
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null)
        {
            _raycasters.Insert(0, gr);
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

    public virtual string GetName()
    {
        return name;
    }

    protected List<GraphicRaycaster> GetAllRaycasters()
    {
        return _raycasters;
    }

    public virtual async Task StartOpen(object data, CancellationToken token)
    {
        _screenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _screenSource.Token;
        _openData = data;
        await OnStartOpen(_openData, token);

        if (IntroTime > 0)
        {
            TriggerAnimation(AnimParams.Intro, IntroTime, OnFinishOpen, token);
        }
        else
        {
            OnFinishOpen(token);
        }
    }

    // Called as the screen finishes opening.
    protected virtual void OnFinishOpen(CancellationToken token)
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
            _logService.Message("Error on close: " + txt);
        }

        StartClose();
    }


    public virtual void StartClose()
    {
        OnStartClose();
        if (OutroTime > 0)
        {
            TriggerAnimation(AnimParams.Outro, OutroTime, OnFinishClose, _token);
        }
        else
        {
            OnFinishClose(_token);
        }
    }

    // Called immediately on start close.
    protected virtual void OnStartClose()
    {

    }

    // Called after close animation ends.
    protected virtual void OnFinishClose(CancellationToken token)
    {
        _screenService.FinishClose(ScreenID);
    }

}



