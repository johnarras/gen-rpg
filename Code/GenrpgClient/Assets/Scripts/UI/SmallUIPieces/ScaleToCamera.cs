﻿
using UnityEngine;

public class ScaleToCamera : BaseBehaviour
{
    private ICameraController _cameraController;
    public bool _remainVertical = false;
    
    public float _defaultScale;
    
    public float _minDistToCamera;
    
    public GameObject _scaledGameObject;
    private Camera _mainCam = null;


    public override void Init()
    {
        base.Init();
        LateScaleUpdate();
        AddUpdate(LateScaleUpdate, UpdateTypes.Late);
    }


    private void LateScaleUpdate()
    {
        if (_mainCam == null)
        {
            _mainCam = _cameraController.GetMainCamera();
        }

        if (_mainCam == null)
        {
            return;
        }

        if (_defaultScale > 0 && _scaledGameObject != null)
        {

            _scaledGameObject.transform.LookAt(_mainCam.transform);

            if (_remainVertical)
            {
                _scaledGameObject.transform.eulerAngles = new Vector3(0, _scaledGameObject.transform.eulerAngles.y + 180, 0);
            }


            float currScale = _defaultScale;

            if (_minDistToCamera > 0)
            {
                float dist = Vector3.Distance(entity.transform.position, _mainCam.transform.position);

                if (dist > _minDistToCamera)
                {

                    float mult = dist / _minDistToCamera;

                    currScale *= mult;
                }
            }
            _scaledGameObject.transform.localScale = Vector3.one * currScale;
        }
    }
}
