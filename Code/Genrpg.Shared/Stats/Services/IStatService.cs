using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;

public interface IStatService : IService
{
    float Pct(Unit unit, long statTypeId);
    void CalcStats(GameState gs, Unit unit, bool resetMutableStats);
    List<StatType> GetMutableStatTypes(GameState gs);
    List<StatType> GetFixedStatTypes(GameState gs);
    List<StatType> GetPrimaryStatTypes(GameState gs);
    List<StatType> GetAttackStatTypes(GameState gs);
    List<StatType> GetSecondaryStatTypes(GameState gs);

    void Add(GameState gs, Unit unit, long statTypeId, int statCategory, long value);
    void Set(GameState gs, Unit unit, long statTypeId, int statCategory, long value);

    void RegenerateTick(GameState gs, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds);
}
