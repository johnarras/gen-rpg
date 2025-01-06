
using ClientEvents;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Units.Entities;

public class ExpBar : BaseBehaviour
{
    
    public ProgressBar _progressBar;

    private long _curr = 0;
    private long _max = 0;
    private Unit _unit = null;

    public void Init(Unit unitIn)
    {

        AddListener<LevelUpEvent>(OnLevelUpdate);
        AddListener<OnAddQuantityReward>(OnAddQuantityRewardHandler);
        _unit = unitIn;

        long currLevelId = _gs.ch.Level;

        LevelInfo nextLevelData = _gameData.Get<LevelSettings>(_gs.ch).Get(_gs.ch.Level);

        if (nextLevelData == null)
        {
            return;
        }

        CurrencyData currencies = _gs.ch.Get<CurrencyData>();

        long currExp = currencies.GetQuantity(CurrencyTypes.Exp);

        if (_progressBar != null && _unit != null)
        {
            _curr = currExp;
            _max = nextLevelData.CurrExp;
            _progressBar.InitRange(0, _max, _curr);
        }
    }

    private void OnAddQuantityRewardHandler(OnAddQuantityReward data)
    {
        if (data.EntityTypeId == EntityTypes.Currency && data.EntityId == CurrencyTypes.Money)
        {
            Init(_unit);
        }
        return;
    }

    private void OnLevelUpdate(LevelUpEvent data)
    {
        Init(_unit);
        return;
    }
}