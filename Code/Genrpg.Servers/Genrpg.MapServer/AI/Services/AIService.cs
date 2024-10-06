
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.MapServer.Maps;
using System.Threading;
using Genrpg.MapServer.Spells;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Targets.Messages;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.AI.Constants;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Pathfinding.Entities;

namespace Genrpg.MapServer.AI.Services
{
    public interface IAIService : IInjectable
    {
        bool Update(IRandom rand, Unit unit);
        void TargetMove(IRandom rand, Unit unit, string targetUnitId);
        void EndCombat(IRandom rand, Unit unit, string killedUnitId, bool clearAllAttackers);
        void BringFriends(IRandom rand, Unit unit, string targetId);
        long GetCastTimes();
        long GetUpdateTimes();
    }

    public class AIService : IAIService
    {

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IPathfindingService _pathfindingService = null;
        private IGameData _gameData = null;
        private ILogService _logService = null;

        public long _updateTimes = 0;
        public long _castTimes = 0;

        public long GetUpdateTimes()
        {
            return _updateTimes;
        }

        public long GetCastTimes()
        {
            return _castTimes;
        }

        public bool Update(IRandom rand, Unit unit)
        {
            if (unit.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }
            _updateTimes++;
            float ux = unit.X;
            float uz = unit.Z;
            float fx = unit.FinalX;
            float fz = unit.FinalZ;
            float spd = unit.Speed;
            float rot = unit.Rot;

            if (!unit.HasFlag(UnitFlags.Evading))
            {
                if (!unit.HasTarget())
                {
                    ScanForTargets(rand, unit);
                    if (!unit.HasTarget())
                    {
                        if (!unit.Moving)
                        {
                            if (false) // Perform other tasks for strategic AI
                            {

                            }
                            else
                            {
                                IdleWander(rand, unit);
                            }
                        }
                        else
                        {
                            KeepMoving(rand, unit);
                        }
                    }
                }
                else // Have target
                {
                    UpdateCombat(rand, unit);
                }
            }
            else
            {
                KeepMoving(rand, unit);
            }

            UpdateAfterAIStep(rand, unit);
            return true;
        }

        protected void IdleWander(IRandom rand, Unit unit)
        {
            if (!unit.Moving && !unit.HasFlag(UnitFlags.Evading) &&
                !unit.GetAddons().Any() &&
                !unit.HasTarget() &&
                rand.NextDouble() < _gameData.Get<AISettings>(unit).IdleWanderChance &&
                unit.Spawn != null)
            {

                unit.ClearAttackers(_logService);

                float wanderRange = 25.0f;

                float targetx = MathUtils.FloatRange(unit.Spawn.X - wanderRange, unit.Spawn.X + wanderRange, rand);
                float targetz = MathUtils.FloatRange(unit.Spawn.Z - wanderRange, unit.Spawn.Z + wanderRange, rand);

                LocationMove(rand, unit, targetx, targetz, MathUtils.FloatRange(0.2f, 0.3f, rand));
            }
        }

        protected void UpdateCombat(IRandom rand, Unit unit)
        {
            if (!_objectManager.GetUnit(unit.TargetId, out Unit target) || target.HasFlag(UnitFlags.IsDead))
            {
                SetTarget(rand, unit, "");
                EndCombat(rand, unit, unit.TargetId, false);
                return;
            }


            SpellData spellData = unit.Get<SpellData>();
            // This does not require an await for monsters
            IReadOnlyList<Spell> spells = spellData.GetData();

            if (spells.Count < 1)
            {
                KeepMoving(rand, unit);
                return;
            }

            _castTimes++;
            Spell spell = spells.FirstOrDefault();

            CastSpell castSpell = new CastSpell()
            {
                SpellId = spell.IdKey,
                TargetId = unit.TargetId,
            };

            _messageService.SendMessage(unit, castSpell);

            KeepMoving(rand, unit);
        }

        protected void ScanForTargets(IRandom rand, Unit unit)
        {
            if (unit.HasFlag(UnitFlags.Evading))
            {
                return;
            }

            List<Unit> nearbyUnits = _objectManager.GetTypedObjectsNear<Unit>(unit.X, unit.Z, unit, _gameData.Get<AISettings>(unit).EnemyScanDistance,
                true);

            nearbyUnits = nearbyUnits.Where(x => x.FactionTypeId != unit.FactionTypeId && !x.HasFlag(UnitFlags.IsDead | UnitFlags.Evading)).ToList();

            if (nearbyUnits.Count > 0)
            {
                string newTargetId = nearbyUnits[rand.Next() % nearbyUnits.Count].Id;
                TargetMove(rand, unit, newTargetId);
                BringFriends(rand, unit, newTargetId); // When it finds a target, it brings friends.
            }
        }

        public void BringFriends(IRandom rand, Unit bringer, string targetId)
        {
            if (!_objectManager.GetUnit(targetId, out Unit targetUnit) || targetUnit.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
            {
                return;
            }

            BringFriends bringAFriend = new BringFriends()
            {
                BringerFactionId = bringer.FactionTypeId,
                BringerId = bringer.Id,
                TargetFactionId = targetUnit.FactionTypeId,
                TargetId = targetUnit.Id,
            };

            _messageService.SendMessageNear(targetUnit, bringAFriend, _gameData.Get<AISettings>(bringer).BringAFriendRadius, false);
        }

        public void LocationMove(IRandom rand, Unit unit, float x, float z, float speedMult)
        {
            unit.Speed = unit.BaseSpeed * speedMult;
            unit.Moving = true;
            unit.FinalX = x;
            unit.FinalZ = z;
        }

        public void TargetMove(IRandom rand, Unit unit, string targetUnitId)
        {
            if (unit.HasFlag(UnitFlags.Evading))
            {
                return;
            }

            if (!_objectManager.GetUnit(targetUnitId, out Unit targetUnit))
            {
                return;
            }

            float speedMult = 1.0f;
            if (unit.FactionTypeId != targetUnit.FactionTypeId)
            {
                speedMult = UnitConstants.CombatSpeedMult;
            }

            SetTarget(rand, unit, targetUnit.Id);

            float targX = unit.X;
            float targZ = unit.Z;

            float dx = unit.X - targetUnit.X;
            float dz = unit.Z - targetUnit.Z;

            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            LocationMove(rand, unit, targX, targZ, speedMult);

            StartCombat(rand, unit, targetUnit);
        }
        public void EndCombat(IRandom rand, Unit unit, string killedUnitId, bool isLeashing)
        {
            string oldTargetId = unit.TargetId;
            SetTarget(rand, unit, null);
            if (!string.IsNullOrEmpty(killedUnitId))
            {
                unit.RemoveAttacker(killedUnitId);
            }

            if (isLeashing)
            {
                unit.AddFlag(UnitFlags.Evading);
                unit.ClearAttackers(_logService);
                LocationMove(rand, unit, unit.CombatStartX, unit.CombatStartZ, UnitConstants.EvadeSpeedMult);
                return;
            }

            ScanForTargets(rand, unit);
            if (!unit.HasTarget() || unit.TargetId == oldTargetId || unit.TargetId == killedUnitId)
            {
                SetTarget(rand, unit, null);

                if (!(unit is Character ch))
                {
                    unit.AddFlag(UnitFlags.Evading);
                    LocationMove(rand, unit, unit.CombatStartX, unit.CombatStartZ, UnitConstants.EvadeSpeedMult);
                    return;
                }
            }
        }

        public void SetTarget(IRandom rand, Unit unit, string targetId)
        {
            if (unit.TargetId == targetId)
            {
                return;
            }

            if (targetId != null)
            {
                if (unit.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
                {
                    return;
                }
                if (!_objectManager.GetUnit(targetId, out Unit target))
                {
                    return;
                }

                if (target.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
                {
                    return;
                }
            }

            unit.TargetId = targetId;

            OnSetTarget onSet = unit.GetCachedMessage<OnSetTarget>(true);
            onSet.CasterId = unit.Id;
            onSet.TargetId = targetId;

            _messageService.SendMessageNear(unit, onSet);
        }

        public void StartCombat(IRandom rand, Unit attacker, Unit victim)
        {
            if (!attacker.HasFlag(UnitFlags.DidStartCombat))
            {
                attacker.CombatStartX = attacker.X;
                attacker.CombatStartZ = attacker.Z;
                attacker.CombatStartRot = attacker.Rot;
                attacker.AddFlag(UnitFlags.DidStartCombat);
            }
            if (attacker.HasFlag(UnitFlags.Evading))
            {
                attacker.RemoveFlag(UnitFlags.Evading);
            }
        }

        public void KeepMoving(IRandom rand, Unit unit)
        {
            if (!unit.Moving || unit.Speed < 0.01f)
            {
                if (unit.HasFlag(UnitFlags.Evading))
                {
                    unit.RemoveFlag(UnitFlags.Evading | UnitFlags.DidStartCombat);
                }
                if (!unit.HasTarget())
                {
                    return;
                }
            }
            if (unit.HasTarget() && !unit.HasFlag(UnitFlags.Evading))
            {
                if (_objectManager.GetUnit(unit.TargetId, out Unit target))
                {
                    unit.FinalX = target.X;
                    unit.FinalZ = target.Z;
                }

                float ddx = unit.FinalX - unit.CombatStartX;
                float ddz = unit.FinalZ - unit.CombatStartZ;

                double combatDist = Math.Sqrt(ddx * ddx + ddz * ddz);

                if (combatDist >= _gameData.Get<AISettings>(unit).LeashDistance)
                {
                    EndCombat(rand, unit, "", true);
                    return;
                }
            }

            float finalDx = unit.X - unit.FinalX;
            float finalDz = unit.Z - unit.FinalZ;

            float distToGo = (float)Math.Sqrt(finalDx * finalDx + finalDz * finalDz);

            if (!unit.Moving)
            {
                unit.RemoveFlag(UnitFlags.Evading);
                if (unit.HasTarget() && distToGo > AIConstants.CloseToTargetDistance)
                {
                    TargetMove(rand, unit, unit.TargetId);
                }
                return;
            }

            unit.Speed = Math.Max(unit.Speed, 0.1f);

            float distGone = unit.Speed * _gameData.Get<AISettings>(unit).UpdateSeconds;

            float oldSpeed = unit.Speed;

            float pctMove = distGone / distToGo;
            if (pctMove >= 1.0f || distToGo < AIConstants.CloseToTargetDistance)
            {
                SetUnitAtFinalLocation(unit);
            }
            else
            {
                int nextWpIndex = -1;
                int closestWpIndex = -1;
                float closestWpDist = 10000;
                for (int index = 0; index < unit.Waypoints.Waypoints.Count; index++)
                {
                    Waypoint wp = unit.Waypoints.Waypoints[index];
                    float dx = wp.Z - unit.X;
                    float dz = wp.Z - unit.Z;

                    double distToNext = Math.Sqrt(dx * dx + dz * dz);

                    if (distToNext < closestWpDist)
                    {
                        distToNext = closestWpDist;
                        closestWpIndex = index;
                    }
                    else if (closestWpIndex >= 0) // Found closest index
                    {
                        nextWpIndex = index;
                        break;
                    }
                }

                if (nextWpIndex < 0 || nextWpIndex >= unit.Waypoints.Waypoints.Count - 1)
                {
                    nextWpIndex = unit.Waypoints.Waypoints.Count - 1;
                }

                for (int i = 0; i < nextWpIndex; i++)
                {
                    unit.Waypoints.RemoveWaypointAt(0);
                }

                float nextXPos = unit.GetNextXPos();
                float nextZPos = unit.GetNextZPos();

                float oldX = unit.X;
                float oldZ = unit.Z;

                float nx = unit.X + (nextXPos - unit.X) * pctMove;
                float nz = unit.Z + (nextZPos - unit.Z) * pctMove;
                unit.X = nx;
                unit.Z = nz;

                float finaldx = unit.X - unit.FinalX;
                float finaldz = unit.Z - unit.FinalZ;

                double finalDist = Math.Sqrt(finaldx * finaldx + finaldz * finaldz);

                if (finalDist > distToGo || finalDist < AIConstants.CloseToTargetDistance)
                {
                    SetUnitAtFinalLocation(unit);        
                }
            }
        }

        private void SetUnitAtFinalLocation(Unit unit)
        {
            unit.X = unit.FinalX;
            unit.Z = unit.FinalZ;
            unit.Speed = 0;
            unit.Moving = false;
            unit.Waypoints.Clear();
            if (unit.HasFlag(UnitFlags.Evading))
            {
                unit.RemoveFlag(UnitFlags.Evading | UnitFlags.DidStartCombat);
                unit.CombatStartX = unit.X;
                unit.CombatStartZ = unit.Z;
            }
        }

        private void UpdateAfterAIStep(IRandom rand, Unit unit)
        {
            _pathfindingService.UpdatePath(unit, rand, (int)unit.FinalX, (int)unit.FinalZ, OnUpdatePath);
        }

        private void OnUpdatePath(IRandom rand, Unit unit)
        {
            UnitUtils.TurnTowardNextPosition(unit);
            _objectManager.UpdatePosition(rand, unit, 0);
        }
    }
}
