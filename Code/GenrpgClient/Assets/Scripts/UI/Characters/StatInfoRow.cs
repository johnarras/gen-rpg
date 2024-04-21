using System;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;

public class StatInfoRow : BaseBehaviour
{
    public GText StatName;
    public GText CurrStat;
    public GText Percent;
    public GText Modifier;

    private StatType _statType = null;

    private long _statTypeId;

    public void Init(Unit unit, long statTypeId, long modifier = 0)
    {
        if (statTypeId > 0)
        {
            this._statTypeId = statTypeId;
        }
        else
        {
            _uIInitializable.SetText(StatName, "=============");
            _uIInitializable.SetText(CurrStat, "");
            _uIInitializable.SetText(Percent, "");
            _uIInitializable.SetText(Modifier, "");
        }

        if (unit == null)
        {
            return;
        }

        if (_statType == null)
        {
            _statType = _gameData.Get<StatSettings>(unit).Get(this._statTypeId);
        }

        if (_statType == null)
        {
            return;
        }

        _uIInitializable.SetText(StatName, _statType.Name);

        long curr = unit.Stats.Max(_statTypeId);

        
        _uIInitializable.SetText(CurrStat, curr.ToString());

        float pct = 0.0f;

        if (_statTypeId <= StatConstants.PrimaryStatEnd)
        {
            _uIInitializable.SetText(Percent, "");
        }
        else if (_statTypeId >= StatConstants.ScaleDownBegin && _statTypeId <= StatConstants.ScaleDownEnd)
        {
            pct = 1.0f-unit.Stats.ScaleDown(_statTypeId);
        }
        else if (_statTypeId >= StatConstants.RatingPercentStart)       
        {
            pct = unit.Stats.Pct(_statTypeId);   
        }
        else // The percent IS the stat (for dam/defense direct mults.
        {
            pct = unit.Stats.Curr(_statTypeId);
        }

        if (Math.Abs(pct) < 0.001f)
        {
            _uIInitializable.SetText(Percent, "");
        }
        else
        {
            _uIInitializable.SetText(Percent, (100 * pct).ToString("F2") + "%");
        }

        if (modifier == 0)
        {
            _uIInitializable.SetText(Modifier, "");
        }
        else if (modifier > 0)
        {
            _uIInitializable.SetText(Modifier, "+" + modifier);
            _uIInitializable.SetColor(Modifier, GColor.green);
        }
        else if (modifier < 0) // Just be explicit here
        {
            _uIInitializable.SetText(Modifier, "-" + modifier);
            _uIInitializable.SetColor(Modifier, GColor.red);
        }
    }


}