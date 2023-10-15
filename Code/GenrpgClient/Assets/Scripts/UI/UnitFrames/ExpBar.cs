
using ClientEvents;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Units.Entities;
using GEntity = UnityEngine.GameObject;

public class ExpBar : BaseBehaviour
{
    
    public ProgressBar _progressBar;

    private long _curr = 0;
    private long _max = 0;
    private Unit _unit = null;

    public void Init(UnityGameState gs, Unit unitIn)
    {

        _gs.AddEvent<LevelUpEvent>(this, OnLevelUpdate);
        _gs.AddEvent<OnAddCurrency>(this, OnAddCurrencyHandler);
        _unit = unitIn;

        long currLevelId = gs.ch.Level;

        LevelInfo nextLevelData = gs.data.GetGameData<LevelSettings>(gs.ch).GetLevel(gs.ch.Level);

        if (nextLevelData == null)
        {
            return;
        }

        CurrencyData currencies = gs.ch.Get<CurrencyData>();

        long currExp = currencies.GetQuantity(CurrencyTypes.Exp);

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