using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

public interface IStatService : IInitializable
{
    float Pct(Unit unit, long statTypeId);
    void CalcStats(Unit unit, bool resetMutableStats);
    List<StatType> GetMutableStatTypes(Unit unit);
    List<StatType> GetFixedStatTypes(Unit unit);
    List<StatType> GetPrimaryStatTypes(Unit unit);
    List<StatType> GetAttackStatTypes(Unit unit);
    List<StatType> GetSecondaryStatTypes(Unit unit);

    void Add(Unit unit, long statTypeId, int statCategory, long value);
    void Set(Unit unit, long statTypeId, int statCategory, long value);

    void RegenerateTick(IRandom rand, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds);
}
