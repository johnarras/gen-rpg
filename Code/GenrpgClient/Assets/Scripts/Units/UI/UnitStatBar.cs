
using UnityEngine;
using ClientEvents;
using Genrpg.Shared.Units.Entities;
using System.Linq;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Stats.Messages;

public class UnitStatBar : BaseBehaviour
{

    Unit _unit = null;

    
    public ProgressBar _progressBar;

    
    public int _statTypeId;

    long _curr = 0;
    long _max = 0;

    public void Init(Unit unitIn)
    {
        _unit = unitIn;
        if (_progressBar != null && _unit != null)
        {
            _curr = _unit.Stats.Curr(_statTypeId);
            _max = _unit.Stats.Max(_statTypeId);
            _progressBar.InitRange(0, _max, _curr);
        }
        AddListener<LevelUpEvent>(OnLevelUpdate);
        AddListener<StatUpd>(OnStatUpdate);
    }
    private void OnStatUpdate(StatUpd sdata)
    {
        if (_progressBar == null || sdata == null || _unit == null || _unit.Id != sdata.UnitId)
        {
            return;
        }

        FullStat myUpdate = sdata.Dat.FirstOrDefault(x => x.GetStatId() == _statTypeId);

        if (myUpdate == null)
        {
            return;
        }


        long curr = myUpdate.GetCurr();
        long max = myUpdate.GetMax();

        if (_max != max)
        {
            _max = max;
            _progressBar.InitRange(0, _max, curr);
        }
        else
        {
            curr = MathUtils.Clamp(0, curr, _max);
            _progressBar.SetValue(curr);
        }
        _curr = curr;


        return;
        
    }

    private void OnLevelUpdate(LevelUpEvent data)
    {
        Init(_unit);
        return;
    }
}