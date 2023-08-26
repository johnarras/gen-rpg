using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ScaleToCamera : BaseBehaviour
{
    [SerializeField]
    private bool _remainVertical = false;
    [SerializeField]
    private float _defaultScale;
    [SerializeField]
    private float _minDistToCamera;
    [SerializeField]
    private GameObject _scaledGameObject;
    private Camera _mainCam = null;


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(LateScaleUpdate, UpdateType.Late);
    }


    private void LateScaleUpdate()
    {
        if (_mainCam == null)
        {
            _mainCam = CameraController.Instance.GetMainCamera();
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
                float dist = Vector3.Distance(transform.position, _mainCam.transform.position);

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
