using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using System.Threading;

public class MapProjectile : BaseBehaviour
{
    protected IClientMapObjectManager _objectManager;

    protected FullFX _full;

    protected float _elapsedTime = 0;

    Vector3 lastPos = Vector3.zero;
    Vector3 currPos = Vector3.zero;

    Vector3 extraHeight = new Vector3(0, 1, 0);

    private CancellationToken _token;
    public void Init(FullFX full, CancellationToken token)
    {
        _token = token;
        _full = full;

        AddUpdate(ProjUpdate, UpdateType.Regular);

        

        if (_full == null || _full.fromObj == null ||
            _full.fx == null || !TokenUtils.IsValid(_full.token))
        {
            Destroy(gameObject);
            return;
        }
        GameObjectUtils.AddToParent(gameObject, _objectManager.GetFXParent());

        transform.position = _full.fromObj.transform.position + extraHeight;
        lastPos = transform.position;
        currPos = transform.position;

        if (_full.fx.Dur < 0.1f)
        {
            _full.fx.Dur = 0.1f;
        }

        GameObjectUtils.SetLayer(gameObject, LayerMask.NameToLayer(LayerNames.SpellLayer));

    }


    private void ProjUpdate()
    {
        if (_full == null || _full.toObj == null)
        {
            return;
        }

        if (_full.fx.Speed > 0)
        {
            float deltaTime = InputService.Instance.GetDeltaTime();
            float distThisTick = deltaTime * _full.fx.Speed;

            Vector3 diff = _full.toObj.transform.position - transform.position;

            float magnitude = diff.magnitude;

            if (distThisTick < magnitude)
            {
                transform.position += diff * (distThisTick / magnitude);
            }
            else
            {
                transform.position = _full.toObj.transform.position;
                GameObject.Destroy(gameObject);
            }
        }
        else
        {
            _elapsedTime += InputService.Instance.GetDeltaTime();

            if (_elapsedTime >= _full.fx.Dur)
            {
                GameObject.Destroy(gameObject);
            }

            if (_full.fromObj != null && _full.toObj != null)
            {
                Vector3 newPos = (_full.fromObj.transform.position * (1 - _elapsedTime / _full.fx.Dur) +
                    _full.toObj.transform.position * (_elapsedTime / _full.fx.Dur)) + extraHeight;
                transform.position = newPos;
                lastPos = currPos;
                currPos = transform.position;

            }
            else
            {
                transform.position += (currPos - lastPos);
                lastPos = currPos;
                currPos = transform.position;
            }
        }
    }
}
