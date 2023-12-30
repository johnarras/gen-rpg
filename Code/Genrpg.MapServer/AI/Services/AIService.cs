
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

namespace Genrpg.MapServer.AI.Services
{
    public interface IAIService : ISetupService
    {
        bool Update(GameState gs, Unit unit);
        void TargetMove(GameState gs, Unit unit, string targetUnitId);
        void EndCombat(GameState gs, Unit unit, string killedUnitId, bool clearAllAttackers);
        void BringFriends(GameState gs, Unit unit, string targetId);
#if DEBUG
        long GetCastTimes();
        long GetUpdateTimes();
#endif
    }

    public class AIService : IAIService
    {

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IPathfindingService _pathfindingService = null;
        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

#if DEBUG
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
#endif

        public bool Update(GameState gs, Unit unit)
        {
            if (unit.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }
#if DEBUG
            _updateTimes++;
#endif
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
                    ScanForTargets(gs, unit);
                    if (!unit.HasTarget())
                    {
                        if (!unit.Moving)
                        {
                            if (false) // Perform other tasks for strategic AI
                            {

                            }
                            else
                            {
                                IdleWander(gs, unit);
                            }
                        }
                        else
                        {
                            KeepMoving(gs, unit);
                        }
                    }
                }
                else // Have target
                {
                    UpdateCombat(gs, unit);
                }
            }
            else
            {
                KeepMoving(gs, unit);
            }

            UpdateAfterAIStep(gs, unit);
            return true;
        }

        protected void IdleWander(GameState gs, Unit unit)
        {
            if (!unit.Moving && !unit.HasFlag(UnitFlags.Evading) &&
                !unit.GetAddons().Any() &&
                !unit.HasTarget() &&
                gs.rand.NextDouble() < gs.data.GetGameData<AISettings>(unit).IdleWanderChance &&
                unit.Spawn != null)
            {

                unit.ClearAttackers();

                float wanderRange = 25.0f;

                float targetx = MathUtils.FloatRange(unit.Spawn.X - wanderRange, unit.Spawn.X + wanderRange, gs.rand);
                float targetz = MathUtils.FloatRange(unit.Spawn.Z - wanderRange, unit.Spawn.Z + wanderRange, gs.rand);

                LocationMove(gs, unit, targetx, targetz, MathUtils.FloatRange(0.2f, 0.3f, gs.rand));
            }
        }

        protected void UpdateCombat(GameState gs, Unit unit)
        {
            if (!_objectManager.GetUnit(unit.TargetId, out Unit target) || target.HasFlag(UnitFlags.IsDead))
            {
                SetTarget(gs, unit, "");
                EndCombat(gs, unit, unit.TargetId, false);
                return;
            }


            SpellData spellData = unit.Get<SpellData>();
            // This does not require an await for monsters
            List<Spell> spells = spellData.GetData();

            if (spells.Count < 1)
            {
                KeepMoving(gs, unit);
                return;
            }

#if DEBUG
            _castTimes++;
#endif
            Spell spell = spells.FirstOrDefault();

            CastSpell castSpell = new CastSpell()
            {
                SpellId = spell.IdKey,
                TargetId = unit.TargetId,
            };

            _messageService.SendMessage(unit, castSpell);

            KeepMoving(gs, unit);
        }

        protected void ScanForTargets(GameState gs, Unit unit)
        {
            if (unit.HasFlag(UnitFlags.Evading))
            {
                return;
            }

            List<Unit> nearbyUnits = _objectManager.GetTypedObjectsNear<Unit>(unit.X, unit.Z, unit, gs.data.GetGameData<AISettings>(unit).EnemyScanDistance,
                true);

            nearbyUnits = nearbyUnits.Where(x => x.FactionTypeId != unit.FactionTypeId && !x.HasFlag(UnitFlags.IsDead | UnitFlags.Evading)).ToList();

            if (nearbyUnits.Count > 0)
            {
                string newTargetId = nearbyUnits[gs.rand.Next() % nearbyUnits.Count].Id;
                TargetMove(gs, unit, newTargetId);
                BringFriends(gs, unit, newTargetId); // When it finds a target, it brings friends.
            }
        }

        public void BringFriends(GameState gs, Unit bringer, string targetId)
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

            _messageService.SendMessageNear(targetUnit, bringAFriend, gs.data.GetGameData<AISettings>(bringer).BringAFriendRadius, false);
        }

        public void LocationMove(GameState gs, Unit unit, float x, float z, float speedMult)
        {
            unit.Speed = unit.BaseSpeed * speedMult;
            unit.Moving = true;
            unit.FinalX = x;
            unit.FinalZ = z;

            //UpdateAfterAIStep(gs, unit);
        }

        public void TargetMove(GameState gs, Unit unit, string targetUnitId)
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

            SetTarget(gs, unit, targetUnit.Id);

            float targX = unit.X;
            float targZ = unit.Z;

            float dx = unit.X - targetUnit.X;
            float dz = unit.Z - targetUnit.Z;

            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            LocationMove(gs, unit, targX, targZ, speedMult);

            StartCombat(gs, unit, targetUnit);
        }
        public void EndCombat(GameState gs, Unit unit, string killedUnitId, bool clearAllAttackers)
        {
            string oldTargetId = unit.TargetId;
            SetTarget(gs, unit, null);
            ScanForTargets(gs, unit);
            if (!string.IsNullOrEmpty(killedUnitId))
            {
                unit.RemoveAttacker(killedUnitId);
            }

            if (clearAllAttackers)
            {
                unit.ClearAttackers();
            }

            if (!unit.HasTarget() || unit.TargetId == oldTargetId || unit.TargetId == killedUnitId)
            {
                SetTarget(gs, unit, null);

                if (!(unit is Character ch))
                {
                    LocationMove(gs, unit, unit.CombatStartX, unit.CombatStartZ, UnitConstants.EvadeSpeedMult);
                }
            }
        }

        public void SetTarget(GameState gs, Unit unit, string targetId)
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

        public void StartCombat(GameState gs, Unit attacker, Unit victim)
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

        public void KeepMoving(GameState gs, Unit unit)
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

                if (combatDist >= gs.data.GetGameData<AISettings>(unit).LeashDistance)
                {
                    unit.AddFlag(UnitFlags.Evading);
                    EndCombat(gs, unit, "", true);
                    return;
                }
            }

            float dx = unit.X - unit.FinalX;
            float dz = unit.Z - unit.FinalZ;

            float distToGo = (float)Math.Sqrt(dx * dx + dz * dz);

            if (!unit.Moving)
            {
                if (unit.HasTarget() && distToGo > AIConstants.CloseToTargetDistance)
                {
                    TargetMove(gs, unit, unit.TargetId);
                }
                return;
            }

            float distGone = unit.Speed * gs.data.GetGameData<AISettings>(unit).UpdateSeconds;

            float oldSpeed = unit.Speed;

            float pctMove = distGone / distToGo;
            if (pctMove >= 1.0f || distToGo < AIConstants.CloseToTargetDistance)
            {
                unit.X = unit.FinalX;
                unit.Z = unit.FinalZ;
                unit.Speed = 0;
                unit.Moving = false;
                if (unit.HasFlag(UnitFlags.Evading))
                {
                    unit.RemoveFlag(UnitFlags.Evading | UnitFlags.DidStartCombat);
                }
            }
            else
            {

                float nextXPos = unit.GetNextXPos();
                float nextZPos = unit.GetNextZPos();


                float oldX = unit.X;
                float oldZ = unit.Z;

                float nx = unit.X + (nextXPos - unit.X) * pctMove;
                float nz = unit.Z + (nextZPos - unit.Z) * pctMove;
                unit.X = nx;
                unit.Z = nz;

                float nextToFinalX = nextXPos - unit.FinalX;
                float nextToFinalZ = nextZPos - unit.FinalZ;

                float nextToFinalDist = MathF.Sqrt(nextToFinalX*nextToFinalX+nextToFinalZ*nextToFinalZ);

                float currToFinalDistX = unit.X - unit.FinalX;
                float currToFinalDistZ = unit.Z - unit.FinalZ;

                float currToFinalDist = MathF.Sqrt(currToFinalDistX*currToFinalDistX+currToFinalDistZ*currToFinalDistZ);

                if (currToFinalDist < nextToFinalDist+1)
                {
                    if (unit.Waypoints != null)
                    {
                        if (unit.Waypoints.Waypoints.Count > 0)
                        {
                            unit.Waypoints.Waypoints.RemoveAt(0);
                        }

                        if (unit.Waypoints.Waypoints.Count == 0)
                        {
                            unit.Waypoints = null;
                        }
                    }
                }
            }           
        }

        private void UpdateAfterAIStep(GameState gs, Unit unit)
        {
            _pathfindingService.UpdatePath(gs, unit, (int)unit.FinalX, (int)unit.FinalZ);
            UnitUtils.TurnTowardNextPosition(unit);
            _objectManager.UpdatePosition(gs, unit, 0);

        }
    }
}
