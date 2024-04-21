
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spells.Messages;
using UnityEngine;

public class CastBar : BaseBehaviour
{
    
    public GEntity _contentParent;
    
    public ProgressBar _progressBar;

    private Unit _unit;
    private string _spellName;
    private bool _isCasting = false;


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(CastUpdate, UpdateType.Regular);
        _dispatcher.AddEvent<OnStartCast>(this, OnStartCastHandler);
        _dispatcher.AddEvent<OnStopCast>(this, OnStopCastHandler);
    }
    
    public void Init(UnityGameState gs, Unit unit)
    {
        _unit = unit;
        HideCast(gs);
    }

    private OnStartCast OnStartCastHandler (UnityGameState gs, OnStartCast onStartCast)
    {

        if (_unit == null || 
            onStartCast == null || 
            onStartCast.CasterId != _unit.Id)
        {
            return null;
        }

        ShowCast(gs, (int)(onStartCast.CastSeconds * 1000), onStartCast.CastingName);

        return null;
    }


    private OnStopCast OnStopCastHandler(UnityGameState gs, OnStopCast onStopCast)
    {

        if (onStopCast.CasterId != _unit.Id)
        {
            return null;
        }
        HideCast(gs);

        return null;
    }

    public void ShowCast (UnityGameState gs, int castTimeMS, string spellName)
    {
        if (_contentParent == null || _progressBar == null)
        {
            return;
        }

        _spellName = spellName;
        _isCasting = true;
        GEntityUtils.SetActive(_contentParent, true);
        _progressBar.InitRange(gs, 0, castTimeMS, 0);
        _progressBar.SetValue(gs, 0, spellName);

    }

    public void HideCast(UnityGameState gs)
    {
        GEntityUtils.SetActive(_contentParent, false);
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
            HideCast(_gs);
        }

        _progressBar.SetValue(_gs, currVal, _spellName);
    }


}