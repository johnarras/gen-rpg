using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;

public interface IStatService : IInitializable
{
    float Pct(Unit unit, long statTypeId);
    void CalcStats(GameState gs, Unit unit, bool resetMutableStats);
    List<StatType> GetMutableStatTypes(GameState gs, Unit unit);
    List<StatType> GetFixedStatTypes(GameState gs, Unit unit);
    List<StatType> GetPrimaryStatTypes(GameState gs, Unit unit);
    List<StatType> GetAttackStatTypes(GameState gs, Unit unit);
    List<StatType> GetSecondaryStatTypes(GameState gs, Unit unit);

    void Add(GameState gs, Unit unit, long statTypeId, int statCategory, long value);
    void Set(GameState gs, Unit unit, long statTypeId, int statCategory, long value);

    void RegenerateTick(GameState gs, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds);
}
