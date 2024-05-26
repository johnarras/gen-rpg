
using GEntity = UnityEngine.GameObject;
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

    public void Init(UnityGameState gs, Unit unitIn)
    {
        _unit = unitIn;
        if (_progressBar != null && _unit != null)
        {
            _curr = _unit.Stats.Curr(_statTypeId);
            _max = _unit.Stats.Max(_statTypeId);
            _progressBar.InitRange(gs, 0, _max, _curr);
        }
        _dispatcher.AddEvent<LevelUpEvent>(this, OnLevelUpdate);
        _dispatcher.AddEvent<StatUpd>(this, OnStatUpdate);
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
            _progressBar.InitRange(_gs, 0, _max, curr);
        }
        else
        {
            curr = MathUtils.Clamp(0, curr, _max);
            _progressBar.SetValue(_gs, curr);
        }
        _curr = curr;


        return;
        
    }

    private void OnLevelUpdate(LevelUpEvent data)
    {
        Init(_gs,_unit);
        return;
    }
}