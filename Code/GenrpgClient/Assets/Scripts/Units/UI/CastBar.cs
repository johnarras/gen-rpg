
using UnityEngine;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spells.Messages;

public class CastBar : BaseBehaviour
{
    
    public GameObject _contentParent;
    
    public ProgressBar _progressBar;

    private Unit _unit;
    private string _spellName;
    private bool _isCasting = false;


    public override void Init()
    {
        base.Init();
        AddUpdate(CastUpdate, UpdateTypes.Regular);
        AddListener<OnStartCast>(OnStartCastHandler);
        AddListener<OnStopCast>(OnStopCastHandler);
    }
    
    public void Init(Unit unit)
    {
        _unit = unit;
        HideCast();
    }

    private void OnStartCastHandler(OnStartCast onStartCast)
    {

        if (_unit == null || 
            onStartCast == null || 
            onStartCast.CasterId != _unit.Id)
        {
            return;
        }

        ShowCast((int)(onStartCast.CastSeconds * 1000), onStartCast.CastingName);

        return;
    }


    private void OnStopCastHandler(OnStopCast onStopCast)
    {

        if (onStopCast.CasterId != _unit.Id)
        {
            return;
        }
        HideCast();

        return;
    }

    public void ShowCast (int castTimeMS, string spellName)
    {
        if (_contentParent == null || _progressBar == null)
        {
            return;
        }

        _spellName = spellName;
        _isCasting = true;
        _clientEntityService.SetActive(_contentParent, true);
        _progressBar.InitRange(0, castTimeMS, 0);
        _progressBar.SetValue(0, spellName);

    }

    public void HideCast()
    {
        _clientEntityService.SetActive(_contentParent, false);
        _isCasting = false;
    }

    private void CastUpdate()
    {
        if (!_isCasting || _progressBar == null)
        {
            return;
        }

        int deltaTicks = (int)(1000 * Time.deltaTime);
        long currVal = _progressBar.GetCurrValue();
        long maxVal = _progressBar.GetMaxValue();
        currVal += deltaTicks;
        if (currVal > maxVal)
        {
            currVal = maxVal;
            HideCast();
        }

        _progressBar.SetValue(currVal, _spellName);
    }


}