﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.MapServer.Maps;
using System.Threading;
using Genrpg.MapServer.Spells;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Targets.Messages;

namespace Genrpg.MapServer.AI.Services
{
    public interface IAIService : ISetupService
    {
        bool Update(GameState gs, Unit unit);
        void ScanForTargets(GameState gs, Unit unit);
        void LocationMove(GameState gs, Unit unit, float x, float z, float speedMult);
        void TargetMove(GameState gs, Unit unit, string targetUnitId);
        void StartCombat(GameState gs, Unit attacker, Unit victim);
        void EndCombat(GameState gs, Unit unit, string killedUnitId, bool clearAllAttackers);
#if DEBUG
        long GetCastTimes();
        long GetUpdateTimes();
#endif
    }

    public class AIService : IAIService
    {

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IServerSpellService _spellService = null;
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
            return true;
        }

        protected void IdleWander(GameState gs, Unit unit)
        {
            if (!unit.Moving && !unit.HasFlag(UnitFlags.Evading) &&
                unit.NPCTypeId < 1 &&
                !unit.HasTarget() &&
                gs.rand.NextDouble() < gs.data.GetGameData<AISettings>().UpdateSeconds &&
                unit.Spawn != null)
            {
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
            List<Spell> spells = spellData.GetAll();

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

            _messageService.SendMessage(gs, unit, castSpell);

            KeepMoving(gs, unit);
        }

        public void ScanForTargets(GameState gs, Unit unit)
        {
            if (unit.HasFlag(UnitFlags.Evading))
            {
                return;
            }

            List<Unit> nearbyUnits = _objectManager.GetTypedObjectsNear<Unit>(unit.X, unit.Z, unit, gs.data.GetGameData<AISettings>().EnemyScanDistance,
                true);

            nearbyUnits = nearbyUnits.Where(x => x.FactionTypeId != unit.FactionTypeId && !x.HasFlag(UnitFlags.IsDead | UnitFlags.Evading)).ToList();

            if (nearbyUnits.Count > 0)
            {
                TargetMove(gs, unit, nearbyUnits[gs.rand.Next()%nearbyUnits.Count].Id);
            }
        }

        public void LocationMove(GameState gs, Unit unit, float x, float z, float speedMult)
        {

            float dx = x - unit.X;
            float dz = z - unit.Z;


            unit.ToX = x;
            unit.ToZ = z;
            unit.Speed = unit.BaseSpeed * speedMult;
            unit.Moving = true;

            UnitUtils.TurnTowardPosition(unit, x, z);

            OnUpdatePos posMessage = unit.GetCachedMessage<OnUpdatePos>(true);

            posMessage.ObjId = unit.Id;
            posMessage.SetX(x);
            posMessage.SetY(unit.Y);
            posMessage.SetZ(z);
            posMessage.SetRot(unit.Rot);
            posMessage.SetSpeed(unit.Speed);


            _messageService.SendMessageNear(gs, unit, posMessage);
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

            //if (dist > AIConstants.TargetStopShortDistance)
            //{
            //    float distScale = AIConstants.TargetStopShortDistance / dist;
            //    targX = unit.X + (targetUnit.X - unit.X) * distScale;
            //    targZ = unit.Z + (targetUnit.Z - unit.Z) * distScale;
            //}

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

            _messageService.SendMessageNear(gs, unit, onSet);
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
                    unit.ToX = target.X;
                    unit.ToZ = target.Z;
                }

                float ddx = unit.ToX - unit.CombatStartX;
                float ddz = unit.ToZ - unit.CombatStartZ;

                double combatDist = Math.Sqrt(ddx * ddx + ddz * ddz);

                if (combatDist >= gs.data.GetGameData<AISettings>().LeashDistance)
                {
                    unit.AddFlag(UnitFlags.Evading);
                    EndCombat(gs, unit, "", true);
                    return;
                }
            }

            float dx = unit.X - unit.ToX;
            float dz = unit.Z - unit.ToZ;

            float distToGo = (float)Math.Sqrt(dx * dx + dz * dz);

            if (!unit.Moving)
            {
                if (unit.HasTarget() && distToGo > 1)
                {
                    TargetMove(gs, unit, unit.TargetId);
                }
                return;
            }

            float distGone = unit.Speed * gs.data.GetGameData<AISettings>().UpdateSeconds;

            float oldSpeed = unit.Speed;

            float pctMove = distGone / distToGo;
            if (pctMove >= 1.0f || distToGo < 0.1f)
            {
                unit.X = unit.ToX;
                unit.Z = unit.ToZ;
                unit.Speed = 0;
                unit.Moving = false;
                if (unit.HasFlag(UnitFlags.Evading))
                {
                    unit.RemoveFlag(UnitFlags.Evading | UnitFlags.DidStartCombat);
                }
            }
            else
            {
                float nx = unit.X + (unit.ToX - unit.X) * pctMove;
                float nz = unit.Z + (unit.ToZ - unit.Z) * pctMove;
                unit.X = nx;
                unit.Z = nz;
                UnitUtils.TurnTowardPosition(unit, unit.ToX, unit.ToZ);
            }

            _objectManager.UpdatePosition(unit);

            float closeToTargetSpeedScale = 1.0f;
            if (pctMove < 1 && pctMove > 0.5f)
            {
                closeToTargetSpeedScale = (1 - pctMove) / pctMove;
            }

            OnUpdatePos posMessage = unit.GetCachedMessage<OnUpdatePos>(true);
            posMessage.ObjId = unit.Id;
            posMessage.SetX((float)Math.Round(unit.X, 1));
            posMessage.SetY((float)Math.Round(unit.Y, 1));
            posMessage.SetZ((float)Math.Round(unit.Z, 1));
            posMessage.SetRot(unit.Rot);
            posMessage.SetSpeed(oldSpeed * closeToTargetSpeedScale);

            _messageService.SendMessageNear(gs, unit, posMessage, MessageConstants.DefaultGridDistance, false);

        }
    }
}