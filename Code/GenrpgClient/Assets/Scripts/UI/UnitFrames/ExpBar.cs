
using ClientEvents;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Units.Entities;
using UnityEngine;

public class ExpBar : BaseBehaviour
{
    [SerializeField]
    private ProgressBar _progressBar;

    private long _curr = 0;
    private long _max = 0;
    private Unit _unit = null;

    public void Init(UnityGameState gs, Unit unitIn)
    {

        _gs.AddEvent<LevelUpEvent>(this, OnLevelUpdate);
        _gs.AddEvent<OnAddCurrency>(this, OnAddCurrencyHandler);
        _unit = unitIn;

        long currLevelId = gs.ch.Level;

        LevelData nextLevelData = gs.data.GetGameData<LevelSettings>().GetLevel(gs.ch.Level);

        if (nextLevelData == null)
        {
            return;
        }

        CurrencyData currencies = gs.ch.Get<CurrencyData>();

        long currExp = currencies.GetQuantity(CurrencyType.Exp);

        if (_progressBar != null && _unit != null)
        {
            _curr = currExp;
            _max = nextLevelData.CurrExp;
            _progressBar.InitRange(gs, 0, _max, _curr);
        }
    }

    private OnAddCurrency OnAddCurrencyHandler(UnityGameState gs, OnAddCurrency data)
    {
        Init(gs, _unit);
        return null;
    }

    private LevelUpEvent OnLevelUpdate(UnityGameState gs, LevelUpEvent data)
    {
        Init(gs, _unit);
        return null;
    }
}