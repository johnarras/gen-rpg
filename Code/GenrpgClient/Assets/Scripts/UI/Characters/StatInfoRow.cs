using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Stats.Entities;

public class StatInfoRow : BaseBehaviour
{
    [SerializeField]
    private Text _statName;
    [SerializeField]
    private Text _currStat;
    [SerializeField]
    private Text _percent;
    [SerializeField]
    private Text _modifier;

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
            UIHelper.SetText(_statName, "=============");
            UIHelper.SetText(_currStat, "");
            UIHelper.SetText(_percent, "");
            UIHelper.SetText(_modifier, "");
        }

        if (unit == null)
        {
            return;
        }

        if (_statType == null)
        {
            _statType = _gs.data.GetGameData<StatSettings>().GetStatType(this._statTypeId);
        }

        if (_statType == null)
        {
            return;
        }

        UIHelper.SetText(_statName, _statType.Name);

        long curr = unit.Stats.Max(_statTypeId);

        
        UIHelper.SetText(_currStat, curr.ToString());

        float pct = 0.0f;

        if (_statTypeId <= StatConstants.PrimaryStatEnd)
        {
            UIHelper.SetText(_percent, "");
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
            UIHelper.SetText(_percent, "");
        }
        else
        {
            UIHelper.SetText(_percent, (100 * pct).ToString("F2") + "%");
        }

        if (modifier == 0)
        {
            UIHelper.SetText(_modifier, "");
        }
        else if (modifier > 0)
        {
            UIHelper.SetText(_modifier, "+" + modifier);
            UIHelper.SetColor(_modifier, Color.green);
        }
        else if (modifier < 0) // Just be explicit here
        {
            UIHelper.SetText(_modifier, "-" + modifier);
            UIHelper.SetColor(_modifier, Color.red);
        }
    }


}