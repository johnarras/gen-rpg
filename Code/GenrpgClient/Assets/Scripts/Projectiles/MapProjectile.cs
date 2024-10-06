using Genrpg.Shared.Constants;
using System.Threading;
using UnityEngine;

public class MapProjectile : BaseBehaviour
{
    protected IClientMapObjectManager _objectManager;
    private IInputService _inputService;

    protected FullFX _full;

    protected float _elapsedTime = 0;

    Vector3 lastPos = Vector3.zero;
    Vector3 currPos = Vector3.zero;

    Vector3 extraHeight = Vector3.up * 1.0f;

    private CancellationToken _token;
    public void Init(FullFX full, CancellationToken token)
    {
        _token = token;
        _full = full;

        AddUpdate(ProjUpdate, UpdateType.Regular);

        

        if (_full == null || _full.fromObj == null ||
            _full.fx == null || !TokenUtils.IsValid(_full.token))
        {
            _gameObjectService.Destroy(entity);
            return;
        }
        _gameObjectService.AddToParent(entity, _objectManager.GetFXParent());

       entity.transform.position = _full.fromObj.transform.position + extraHeight;
        lastPos = entity.transform.position;
        currPos = entity.transform.position;

        if (_full.fx.Dur < 0.1f)
        {
            _full.fx.Dur = 0.1f;
        }

        _gameObjectService.SetLayer(entity, LayerUtils.NameToLayer(LayerNames.SpellLayer));

    }


    private void ProjUpdate()
    {
        if (_full == null || _full.toObj == null)
        {
            return;
        }

        if (_full.fx.Speed > 0)
        {
            float deltaTime = _inputService.GetDeltaTime();
            float distThisTick = deltaTime * _full.fx.Speed;

            Vector3 diff = _full.toObj.transform.position -entity.transform.position;

            float magnitude = diff.magnitude;

            if (distThisTick < magnitude)
            {
               entity.transform.position += diff * (distThisTick / magnitude);
            }
            else
            {
               entity.transform.position = _full.toObj.transform.position;
                _gameObjectService.Destroy(entity);
            }
        }
        else
        {
            _elapsedTime += _inputService.GetDeltaTime();

            if (_elapsedTime >= _full.fx.Dur)
            {
                _gameObjectService.Destroy(entity);
            }

            if (_full.fromObj != null && _full.toObj != null)
            {
                Vector3 newPos = (_full.fromObj.transform.position * (1 - _elapsedTime / _full.fx.Dur) +
                    _full.toObj.transform.position * (_elapsedTime / _full.fx.Dur)) + extraHeight;
               entity.transform.position = newPos;
                lastPos = currPos;
                currPos = entity.transform.position;

            }
            else
            {
               entity.transform.position += currPos - lastPos;
                lastPos = currPos;
                currPos = entity.transform.position;
            }
        }
    }
}
