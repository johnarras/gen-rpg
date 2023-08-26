using UnityEngine;
using System.Collections;
using System;

using Genrpg.Shared.Core.Entities;

using Services;
using Assets.Scripts.Interfaces;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.Model;
using Scripts.Assets.Audio.Constants;
using System.Threading;

public class BaseBehaviour : MonoBehaviour
{
	public static string ErrorString = "";

    protected IAudioService _audioService;
    protected IUnityUpdateService _updateService;
    protected IScreenService _screenService;
    protected INetworkService _networkService;
    protected IAssetService _assetService;

    public UnityGameState _gs { get; set; }
    
    public virtual void Initialize(UnityGameState gs)
    {
        _gs = gs;
        _gs.loc.Resolve(this);
    }

    public bool CanClick(string name)
    {
        if (!canClick(name))
        {
            _audioService.PlaySound(_gs, AudioList.ErrorClick);
            return false;
        }

        _audioService.PlaySound(_gs, AudioList.ButtonClick);
        return true;
    }

	

	private bool canClick(string name)
	{
        return true;		
	}


    private bool _addedUpdate = false;
    protected void AddUpdate(Action func, int index)
    {
        _addedUpdate = true;
        _updateService?.AddUpdate(this, func, index);
    }

    protected void AddTokenUpdate(Action<CancellationToken> func, int index)
    {
        _addedUpdate = true;
        _updateService?.AddTokenUpdate(this, func, index);
    }
	
    protected virtual void OnDestroy()
    {
        if (_addedUpdate)
        {
            _updateService.RemoveUpdates(this);
        }
    }
}

