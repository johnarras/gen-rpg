using Genrpg.Shared.Constants;
using System.Threading;

public class MapProjectile : BaseBehaviour
{
    protected IClientMapObjectManager _objectManager;
    private IInputService _inputService;

    protected FullFX _full;

    protected float _elapsedTime = 0;

    GVector3 lastPos = GVector3.zero;
    GVector3 currPos = GVector3.zero;

    GVector3 extraHeight = GVector3.up * 1.0f;

    private CancellationToken _token;
    public void Init(FullFX full, CancellationToken token)
    {
        _token = token;
        _full = full;

        AddUpdate(ProjUpdate, UpdateType.Regular);

        

        if (_full == null || _full.fromObj == null ||
            _full.fx == null || !TokenUtils.IsValid(_full.token))
        {
            GEntityUtils.Destroy(entity);
            return;
        }
        GEntityUtils.AddToParent(entity, _objectManager.GetFXParent());

       entity.transform().position = GVector3.Create(GVector3.Create(_full.fromObj.transform().position) + extraHeight);
        lastPos = GVector3.Create(entity.transform().position);
        currPos = GVector3.Create(entity.transform().position);

        if (_full.fx.Dur < 0.1f)
        {
            _full.fx.Dur = 0.1f;
        }

        GEntityUtils.SetLayer(entity, LayerUtils.NameToLayer(LayerNames.SpellLayer));

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

            GVector3 diff = GVector3.Create(_full.toObj.transform().position -entity.transform().position);

            float magnitude = diff.magnitude;

            if (distThisTick < magnitude)
            {
               entity.transform().position += GVector3.Create(diff * (distThisTick / magnitude));
            }
            else
            {
               entity.transform().position = _full.toObj.transform().position;
                GEntityUtils.Destroy(entity);
            }
        }
        else
        {
            _elapsedTime += _inputService.GetDeltaTime();

            if (_elapsedTime >= _full.fx.Dur)
            {
                GEntityUtils.Destroy(entity);
            }

            if (_full.fromObj != null && _full.toObj != null)
            {
                GVector3 newPos = (GVector3.Create(_full.fromObj.transform().position)* (1 - _elapsedTime / _full.fx.Dur) +
                    GVector3.Create(_full.toObj.transform().position) * (_elapsedTime / _full.fx.Dur)) + extraHeight;
               entity.transform().position = GVector3.Create(newPos);
                lastPos = currPos;
                currPos = GVector3.Create(entity.transform().position);

            }
            else
            {
               entity.transform().position += GVector3.Create(currPos - lastPos);
                lastPos = currPos;
                currPos = GVector3.Create(entity.transform().position);
            }
        }
    }
}
