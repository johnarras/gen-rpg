
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using System.Threading;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spells.SpellEffectHandlers;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Targets.Messages;

namespace Genrpg.MapServer.Spells
{
    public class ServerSpellService : IServerSpellService
    {
        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IReflectionService _reflectionService = null;
        private IStatService _statService;
        protected Dictionary<long, ISpellEffectHandler> _handlers = null;
        public async Task Setup(GameState gs, CancellationToken token)
        {
            _handlers = _reflectionService.SetupDictionary<long, ISpellEffectHandler>(gs);

            foreach (ISpellEffectHandler handler in _handlers.Values)
            {
                handler.Init(gs);
            }
            await Task.CompletedTask;
        }

        public TryCastResult TryCast(GameState gs, Unit caster, long spellId, string targetId, bool endOfCast)
        {
            TryCastResult result = new TryCastResult();

            if (string.IsNullOrEmpty(targetId))
            {
                result.State = TryCastState.TargetMissing;
                return result;
            }

            if (caster.IsDeleted())
            {
                result.State = TryCastState.CasterDeleted;
                return result;
            }
            else if (caster.HasFlag(UnitFlags.IsDead))
            {
                result.State = TryCastState.CasterDead;
                return result;
            }

            if (caster.HasFlag(UnitFlags.Evading))
            {
                result.State = TryCastState.CasterEvading;
                return result;
            }

            if (!endOfCast && caster.ActionMessage != null && !caster.ActionMessage.IsCancelled())
            {
                CastingSpell castingSpell = caster.ActionMessage as CastingSpell;
                // Only mark caster as busy if the spell has not reached its end casting time...
                // potential fix for stuck spells
                if (castingSpell == null || DateTime.UtcNow < castingSpell.EndCastingTime)
                {
                    result.State = TryCastState.CasterBusy;
                    return result;
                }
                else
                {
                    castingSpell.SetCancelled(true);
                }
            }

            SpellData spellData = caster.Get<SpellData>();
            if (spellData == null)
            {
                result.State = TryCastState.NoSpellData;
                return result;
            }
            Spell spell = spellData.Get(spellId);

            if (spell == null)
            {
                result.State = TryCastState.DoNotKnowSpell;
                return result;
            }
            if (spell.HasFlag(SpellFlags.IsPassive))
            {
                result.State = TryCastState.IsPassive;
                return result;
            }

            ElementType elementType = gs.data.GetGameData<SpellSettings>().GetElementType(spell.ElementTypeId);

            if (elementType == null)
            {
                result.State = TryCastState.UnknownElement;
                return result;
            }

            SkillType skillType = gs.data.GetGameData<SpellSettings>().GetSkillType(spell.SkillTypeId);

            if (skillType == null)
            {
                result.State = TryCastState.UnknownSkill;
                return result;
            }


            if (spell.CooldownEnds > DateTime.UtcNow)
            {
                result.State = TryCastState.OnCooldown;
                return result;
            }

            if (caster.Stats.Curr(skillType.PowerStatTypeId) < spell.GetCost(gs, caster))
            {
                result.State = TryCastState.NotEnoughPower;
                return result;
            }

            TargetCastState targState = GetTargetState(gs, spell, targetId);

            if (targState.State != TryCastState.Ok)
            {
                result.State = targState.State;
                return result;
            }

            if (caster.DistanceTo(targState.Target) > spell.GetRange())
            {
                result.State = TryCastState.TargetTooFar;
                return result;
            }

            result.Spell = spell;
            result.Target = targState.Target;
            result.ElementType = elementType;
            result.SkillType = skillType;

            if (caster is Character ch)
            {
                AbilityData adata = ch.Get<AbilityData>();
                result.ElementRank = adata.GetRank(gs, AbilityCategory.Element, result.ElementType.IdKey);
                result.SkillRank = adata.GetRank(gs, AbilityCategory.Skill, result.SkillType.IdKey);
            }

            result.State = TryCastState.Ok;
            return result;
        }
        public TargetCastState GetTargetState(GameState gs, Spell spell, string unitId)
        {
            TargetCastState castState = new TargetCastState();
            if (!_objectManager.GetUnit(unitId, out Unit target))
            {
                castState.State = TryCastState.TargetMissing;
                return castState;
            }

            castState.Target = target;

            if (target.IsDeleted())
            {
                castState.State = TryCastState.TargetDeleted;
                return castState;
            }

            if (target.HasFlag(UnitFlags.IsDead))
            {
                castState.State = TryCastState.TargetDead;
                return castState;
            }

            if (target.HasFlag(UnitFlags.Evading))
            {
                castState.State = TryCastState.Evading;
                return castState;
            }

            castState.State = TryCastState.Ok;
            return castState;
        }

        protected bool GetSpellEffectHandler(long spellEffectId, out ISpellEffectHandler handler)
        {
            if (_handlers.TryGetValue(spellEffectId, out handler))
            {
                return true;
            }
            handler = null;
            return false;
        }


        public void SendStopCast(GameState gs, MapObject obj)
        {

            OnStopCast stop = obj.GetCachedMessage<OnStopCast>(true);
            stop.CasterId = obj.Id;
            _messageService.SendMessageNear(gs, obj, stop);
        }

        public void SendSpell(GameState gs, Unit caster, TryCastResult result)
        {
            // Creating and sending projectiles

            StatGroup newGroup = new StatGroup();
            newGroup._stats = new List<Stat>(caster.Stats._stats);
            SendSpell sendSpell = new SendSpell()
            {
                CasterId = caster.Id,
                CasterStats = newGroup,
                CasterLevel = caster.Level,
                CasterFactionId = caster.FactionTypeId,
                Spell = result.Spell,
                ElementType = result.ElementType,
                ElementRank = result.ElementRank,
                SkillType = result.SkillType,
                SkillRank = result.SkillRank,
            };

            SendOneSpell(gs, caster, result.Target, sendSpell, true);
        }

        public void ResendSpell(GameState gs, Unit caster, Unit target, SendSpell sendSpell)
        {
            SendOneSpell(gs, caster, target, sendSpell, false);
        }

        private void SendOneSpell(GameState gs, Unit caster, Unit target, SendSpell sendSpell, bool isFirstSend)
        {
            float distance1 = MapObjectUtils.DistanceBetween(caster, target);

            float duration1 = distance1 / SpellConstants.ProjectileSpeed;

            if (target.Moving &&
                (target.ToX != target.X ||
                target.Z != target.ToZ))
            {
                float tdx = target.ToX - target.X;
                float tdz = target.ToZ - target.Z;

                float dist = (float)Math.Sqrt(tdx * tdx + tdz * tdz);

                float distTravelled = duration1 * target.Speed;

                if (distTravelled > dist)
                {
                    distTravelled = dist;
                }

                float endx = target.X + tdx * distTravelled / dist;
                float endz = target.Z + tdz * distTravelled / dist;

                float endDX = endx - caster.X;
                float endDZ = endz - caster.Z;

                float distance2 = (float)Math.Sqrt(endDX * endDX + endDZ * endDZ);

                float duration2 = distance2 / SpellConstants.ProjectileSpeed;

                duration1 = (duration1 + duration2) / 2;

            }

            if (sendSpell.Spell.HasFlag(SpellFlags.InstantHit))
            {
                duration1 = 0;
            }
            else
            {
                duration1 = Math.Max(0.1f, duration1);
            }

            _messageService.SendMessage(gs, target, sendSpell, duration1);


            caster.ActionMessage = null;

            if (duration1 > 0)
            {
                ShowProjectile(gs, caster, target, sendSpell.Spell, FXNames.Projectile, SpellConstants.ProjectileSpeed);
            }
            else
            {
                ShowFX(gs, caster.Id, target.Id, sendSpell.Spell.ElementTypeId, FXNames.Projectile, 0);
            }
            //}
            //if (isFirstSend && caster is Character ch)
            //{
            //    ResendSpell resend = new ResendSpell()
            //    {
            //        TargetId = target.Id,
            //        ShotsLeft = 2,
            //        SpellMessage = sendSpell,
            //    };
            //    _ms.AddMessage(gs, caster, resend, SpellUtils.GetResendDelay(sendSpell.Spell.HasFlag(SpellFlags.InstantHit)));
            //}
        }

        public void ShowFX(GameState gs, string fromUnitId, string toUnitId, long elementTypeId, string fxName, float duration)
        {

            if (string.IsNullOrEmpty(fxName))
            {
                return;
            }

            FX fx = new FX()
            {
                From = fromUnitId,
                To = toUnitId,
                Dur = duration,
            };

            ElementType etype = gs.data.GetGameData<SpellSettings>().GetElementType(elementTypeId);
            if (etype != null)
            {
                fx.Art = etype.Art + fxName;
            }

            if (_objectManager.GetUnit(fromUnitId, out Unit fromUnit))
            {
                _messageService.SendMessageNear(gs, fromUnit, fx);
            }
        }


        public void ShowProjectile(GameState gs, Unit fromUnit, Unit toUnit, Spell spell, string fxName, float speed)
        {
            FX fx = new FX()
            {
                From = fromUnit.Id,
                To = toUnit.Id,
                Dur = 0,
                Speed = speed,
            };

            ElementType etype = gs.data.GetGameData<SpellSettings>().GetElementType(spell.ElementTypeId);
            if (etype != null)
            {
                fx.Art = etype.Art + fxName;
            }

            _messageService.SendMessageNear(gs, fromUnit, fx);
        }

        public void OnSendSpell(GameState gs, Unit origTarget, SendSpell sendSpell)
        {
            List<SpellHit> hits = GetTargetsHit(gs, origTarget, sendSpell);

            foreach (SpellHit hit in hits)
            {
                _messageService.SendMessage(gs, hit.Target, hit);
            }
        }

        /// <summary>
        /// This creates a list of spell hit objects for each victim hit.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="spellData"></param>
        /// <returns></returns>
        protected List<SpellHit> GetTargetsHit(GameState gs, Unit origTarget, SendSpell sendSpell)
        {
            List<SpellHit> hits = new List<SpellHit>();

            long targetTypeId = sendSpell.SkillType.TargetTypeId;
            long casterFactionId = sendSpell.CasterFactionId;
            if (targetTypeId == TargetType.None)
            {
                return hits;
            }

            List<Unit> primaryTargets = new List<Unit>();
            List<Unit> targets = new List<Unit>();


            primaryTargets.Add(origTarget);
            targets.Add(origTarget);

            if (sendSpell.Spell.ExtraTargets > 0)
            {
                List<Unit> newTargets = GetUnitsNear(gs, origTarget.X, origTarget.Z, origTarget, SpellConstants.ExtraTargetRadius,
                    casterFactionId, targetTypeId, sendSpell.Spell.ExtraTargets);

                targets.AddRange(newTargets);
                targets = targets.Distinct().ToList(); // Use as help vs units moving across cells
                primaryTargets.AddRange(newTargets);
                primaryTargets = primaryTargets.Distinct().ToList(); // Use as help vs units moving across cells
            }
            if (sendSpell.Spell.Radius > 0)
            {
                if (primaryTargets.Count > 0)
                {
                    foreach (Unit ptarg in primaryTargets)
                    {
                        List<Unit> newTargets = GetUnitsNear(gs, ptarg.X, ptarg.Z, ptarg, sendSpell.Spell.Radius, casterFactionId, targetTypeId);

                        newTargets = newTargets.Distinct().ToList();
                        targets.AddRange(newTargets.Except(primaryTargets));
                    }
                }
            }

            foreach (Unit targ in targets)
            {
                if (targ.HasFlag(UnitFlags.IsDead) || targ.IsDeleted())
                {
                    continue;
                }
                SpellHit newHit = new SpellHit()
                {
                    Id = gs.rand.NextLong(),
                    OrigTarget = origTarget,
                    Target = targ,
                    SendSpell = sendSpell,
                };

                if (primaryTargets.Contains(targ))
                {
                    newHit.PrimaryTarget = true;
                }
                hits.Add(newHit);
            }
            return hits;
        }

        protected List<Unit> GetUnitsNear(GameState gs, float x, float z, Unit origTarget, float radius,
            long casterFactionId, long targetTypeId, long maxQuantity = 0)
        {
            List<Unit> newTargets = _objectManager.GetTypedObjectsNear<Unit>(x, z, origTarget, radius, true);

            newTargets = newTargets.Where(unit => SpellUtils.IsValidTarget(gs, unit, casterFactionId, targetTypeId)).ToList();

            if (newTargets.Contains(origTarget))
            {
                newTargets.Remove(origTarget);
            }

            if (maxQuantity > 0)
            {
                newTargets.RemoveAt(gs.rand.Next() % newTargets.Count);
            }
            return newTargets;
        }

        public void OnSpellHit(GameState gs, SpellHit hit)
        {
            List<SpellEffect> effects = CalcSpellEffects(gs, hit);

            foreach (SpellEffect eff in effects)
            {
                if (!_objectManager.GetUnit(eff.TargetId, out Unit unit))
                {
                    continue;
                }
                _messageService.SendMessage(gs, unit, eff);
            }
        }

        /// <summary>
        /// Calculate what happens wrt one spell vs one target
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="castData"></param>
        /// <param name="hitData"></param>
        /// <param name="cr"></param>
        public List<SpellEffect> CalcSpellEffects(GameState gs, SpellHit hit)
        {

            List<SpellEffect> retval = new List<SpellEffect>();
            SendSpell sendSpell = hit.SendSpell;
            Spell spell = sendSpell.Spell;

            ElementType elementType = hit.SendSpell.ElementType;
            SkillType skillType = hit.SendSpell.SkillType;
            long elementRank = hit.SendSpell.ElementRank;
            long skillRank = hit.SendSpell.SkillRank;
            long level = sendSpell.CasterLevel;

            double elemSkillScalePct = elementType.GetScalePct(skillType.IdKey) / 100.0f;

            double skillBaseScalePct = skillType.BaseScalePct / 100.0f;
            int skillRankScale = skillType.RankScale;
            int elemRankScale = elementType.RankScale;

            long baseQuantity = 0;
            long defenseQuantity = 0;
            long maxQuantity = 0;
            long finalQuantity = 0;

            if (!GetSpellEffectHandler(skillType.EntityTypeId, out ISpellEffectHandler handler))
            {
                return retval;
            }

            if (handler.UseStatScaling())
            {
                long powerStatId = skillType.ScalingStatTypeId;
                if (powerStatId < StatType.Power || powerStatId >= StatType.Power + 10)
                {
                    powerStatId = StatType.Power;
                }

                long powerOffset = powerStatId - StatType.Power;

                StatGroup cStats = sendSpell.CasterStats;

                long powerStat = cStats.Max(powerStatId);
                double statPowerMult = cStats.Max(StatType.PowerMult + powerOffset);

                if (powerOffset > 0)
                {
                    powerStat += cStats.Max(StatType.Power);
                    statPowerMult += cStats.Max(StatType.PowerMult);
                }

                statPowerMult = 1 + statPowerMult / 100.0f;

                double trainedRankScale = 1 + (elementRank * elemRankScale + skillRank * skillRankScale) / 100.0f;

                double primaryScale = spell.Scale / 100.0f;

                // Base amount is your base stat times the skill and element scaling.
                baseQuantity = (long)(
                    powerStat *
                    skillBaseScalePct *
                    elemSkillScalePct *
                    trainedRankScale *
                    statPowerMult *
                    primaryScale);

                defenseQuantity = baseQuantity;

                // If there's a target and it's an enemy spell, then get the victim's defenses.
                if (hit.Target != null && skillType.TargetTypeId == TargetType.Enemy)
                {
                    StatGroup tStats = hit.Target.Stats;
                    float defenseScaleDown = tStats.ScaleDown(StatType.Defense + powerOffset);
                    long defenseMult = tStats.Max(StatType.DefenseMult + powerOffset);

                    if (powerOffset > 0)
                    {
                        defenseScaleDown *= tStats.ScaleDown(StatType.Defense);
                        defenseMult += tStats.Max(StatType.DefenseMult);
                    }


                    defenseMult = Math.Min(defenseMult, SpellConstants.MaxDefenseMult);

                    defenseQuantity = (long)(baseQuantity * defenseScaleDown * (100 - defenseMult) / 100.0f);
                }
                else
                {
                    defenseQuantity = baseQuantity;
                }

                maxQuantity = defenseQuantity;
                finalQuantity = maxQuantity;

                hit.CritChance = cStats.Pct(StatType.Crit);
                hit.CritMult = 2.0f + cStats.Pct(StatType.CritDam);


            }
            else // For buff/debuff use scaling from element * skill
            {
                // elem Rank and skillRank are self-explanatory.
                // skillRankScale = how many points this skill grants per level.
                // elemSkillScalePct = percent of effectiveness of this skill. (x100 so need div by 100)
                baseQuantity = (long)(elementRank * skillRank * skillRankScale * elemSkillScalePct / 100);
                maxQuantity = baseQuantity;
                defenseQuantity = baseQuantity;
                finalQuantity = baseQuantity;
            }
            hit.BaseQuantity = finalQuantity;
            hit.VariancePct = skillType.VariancePct;

            return handler.CreateEffects(gs, hit);

        }

        public void ShowCombatText(GameState gs, Unit unit, string txt, int combatTextColorId, bool isCrit = false)
        {
            CombatText combatText = new CombatText()
            {
                TargetId = unit.Id,
                Text = txt,
                TextColor = combatTextColorId,
                IsCrit = isCrit,
            };
            _messageService.SendMessageNear(gs, unit, combatText);
        }




        public void ApplyOneEffect(GameState gs, SpellEffect eff)
        {

            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ))
            {
                return;
            }

            if (eff.Id < 0)
            {
                eff.Id = gs.rand.NextLong();
            }

            bool isImmune = targ.IsFullImmune(gs);

            if (!isImmune && eff.Radius > 0 && eff.IsOrigTarget)
            {
                ShowFX(gs, eff.TargetId, eff.TargetId, eff.ElementTypeId, FXNames.Explosion, 2.0f);
            }

            bool allySpell = false;
            long targetTypeId = gs.data.GetGameData<SpellSettings>().GetSkillType(eff.SkillTypeId).TargetTypeId;
            if (targetTypeId == TargetType.Ally)
            {
                allySpell = true;
            }

            string fxName = FXNames.SpellHit;
            if (allySpell)
            {
                fxName = FXNames.AllyHit;
            }
            else if (eff.Range <= SkillType.MinRange)
            {
                fxName = "";
            }
            if (eff.DurationLeft < eff.Duration)
            {
                fxName = FXNames.DoT;
                fxName = "";
            }
            ShowFX(gs, eff.TargetId, eff.TargetId, eff.ElementTypeId, fxName, 2.0f);


            if (!GetSpellEffectHandler(eff.EntityTypeId, out ISpellEffectHandler handler))
            {
                return;
            }


            if (!handler.HandleEffect(gs, eff))
            {
                eff.SetCancelled(true);
                return;
            }

            // If this is the first tick, add the effect.
            if (eff.Duration > 0 && !isImmune && eff.DurationLeft == eff.Duration)
            {
                AddEffect(gs, eff);
            }
            // If it's the last tick, remove it.
            else if (eff.DurationLeft < 1 || isImmune)
            {
                RemoveEffect(gs, eff);
            }
            // Otherwise middle tick, do nothing.
            else
            {

            }

            if (!isImmune)
            {
                if (targetTypeId == TargetType.Enemy && !string.IsNullOrEmpty(fxName))
                {
                    ShowFX(gs, eff.TargetId, eff.TargetId, eff.ElementTypeId, fxName, 1);
                }
                //await ProcSpells(gs, hitData, ProcType.OnHitTarget, cr);
                //await ProcSpells(gs, hitData, ProcType.OnWasHit, cr);
            }

            if (eff.Duration > 0 && eff.DurationLeft > 0)
            {
                float tickLength = handler.GetTickLength();
                if (tickLength > eff.DurationLeft)
                {
                    tickLength = eff.DurationLeft;
                }
                eff.DurationLeft -= tickLength;
                _messageService.SendMessage(gs, targ, eff, tickLength);
            }
        }


        protected void AddEffect(GameState gs, SpellEffect eff)
        {

            if (!_objectManager.GetUnit(eff.TargetId, out Unit unit))
            {
                return;
            }

            if (unit.SpellEffects == null)
            {
                unit.SpellEffects = new List<SpellEffect>();
            }


            SpellEffect currEffectData = unit.SpellEffects.FirstOrDefault(x => x.MatchesOther(eff));


            bool canAddNew = true;
            if (currEffectData != null)
            {
                if (Math.Abs(currEffectData.Quantity) <= Math.Abs(eff.Quantity))
                {
                    RemoveEffect(gs, currEffectData);
                }
                else if (eff.DurationLeft > currEffectData.DurationLeft)
                {
                    currEffectData.Duration = eff.Duration;
                    currEffectData.DurationLeft = eff.DurationLeft;
                    OnUpdateEffect updateEff = new OnUpdateEffect()
                    {
                        Id = currEffectData.Id,
                        Duration = currEffectData.Duration,
                        DurationLeft = currEffectData.DurationLeft,
                    };
                    _messageService.SendMessageNear(gs, unit, updateEff);
                    canAddNew = false;
                }
                else
                {
                    canAddNew = false;
                }
            }

            if (canAddNew)
            {

                unit.SpellEffects.Add(eff);

                OnAddEffect addEffect = new OnAddEffect()
                {
                    Id = eff.Id,
                    Duration = eff.Duration,
                    DurationLeft = eff.DurationLeft,
                    EntityTypeId = eff.EntityTypeId,
                    EntityId = eff.EntityId,
                    Quantity = eff.Quantity,
                    Icon = eff.Icon,
                    TargetId = eff.TargetId,
                };

                _messageService.SendMessageNear(gs, unit, addEffect);
            }

            _statService.CalcStats(gs, unit, false);

        }


        protected void RemoveEffect(GameState gs, SpellEffect effData)
        {

            if (!_objectManager.GetUnit(effData.TargetId, out Unit unit))
            {
                return;
            }

            if (unit.SpellEffects == null || !unit.SpellEffects.Contains(effData))
            {
                return;
            }

            unit.SpellEffects.Remove(effData);
            effData.SetCancelled(true);

            OnRemoveEffect onRemove = new OnRemoveEffect()
            {
                TargetId = effData.TargetId,
                Id = effData.Id,
            };

            _messageService.SendMessageNear(gs, unit, onRemove);

            _statService.CalcStats(gs, unit, false);
        }

        protected void ProcSpells(GameState gs, SpellHit spellHit, int procTypeId, CastResult cr)
        {
            List<FullProc> procList = new List<FullProc>();

            string procCasterId = null;
            string procTargetId = null;

            if (procTypeId == ProcType.OnCast || procTypeId == ProcType.OnHitTarget)
            {
                procCasterId = spellHit.SendSpell.CasterId;
                procTargetId = spellHit.Target.Id;
            }
            else if (procTypeId == ProcType.OnWasHit || procTypeId == ProcType.OnDeath)
            {
                procCasterId = spellHit.Target.Id;
                procTargetId = spellHit.SendSpell.CasterId;
            }

            //procList.AddRange(AddProcsFromList(gs, castData, castData.Element.Procs, procTypeId, true));
            //procList.AddRange(AddProcsFromList(gs, castData, castData.Spell.Procs, procTypeId, false));
            //if (procCaster != null)
            //{
            //    procList.AddRange(AddProcsFromList(gs, castData, procCaster.Procs, procTypeId, true));
            //}



            foreach (FullProc proc in procList)
            {
                Spell procSpell = gs.data.GetGameData<SpellSettings>().GetSpell(proc.Proc.SpellId);
                if (procSpell == null)
                {
                    continue;
                }

                SkillType procSkill = gs.data.GetGameData<SpellSettings>().GetSkillType(procSpell.SkillTypeId);
                if (procSkill == null)
                {
                    continue;
                }

                if (procSkill.TargetTypeId == TargetType.Ally)
                {
                    procTargetId = procCasterId;
                }
                else if (procSkill.TargetTypeId == TargetType.Enemy &&
                    procTargetId == procCasterId)
                {
                    continue;
                }

                proc.Current.CooldownEnds = DateTime.UtcNow.AddSeconds(proc.Proc.Cooldown);

                //ProcOneSpell(gs, procSpell, procCaster, procTarget, proc.Proc.Scale, spellHit.ProcDepth + 1, cr);

            }

            return;
        }

        protected List<FullProc> AddProcsFromList(GameState gs, SpellHit hitData, List<SpellProc> listToCheck,
            int procTypeId, bool requireElementSkillMatch)
        {
            List<FullProc> retval = new List<FullProc>();
            if (listToCheck == null)
            {
                return retval;
            }

            foreach (SpellProc proc in listToCheck)
            {
                if (proc.ProcTypeId != procTypeId || gs.rand.Next() % 100 >= proc.Chance)
                {
                    continue;
                }
                if (requireElementSkillMatch)
                {
                    if (proc.FromElementTypeId > 0 && hitData.SendSpell.Spell.ElementTypeId != proc.FromElementTypeId ||
                    proc.FromSkillTypeId > 0 && hitData.SendSpell.Spell.SkillTypeId != proc.FromSkillTypeId)
                    {
                        continue;
                    }
                }

                CurrentProc currentProc = hitData.Target.GetCurrentProc(proc.SpellId);

                if (proc.Cooldown > 0)
                {
                    if (DateTime.UtcNow < currentProc.CooldownEnds)
                    {
                        continue;
                    }
                }

                FullProc fullProc = new FullProc()
                {
                    SpellHit = hitData,
                    Proc = proc,
                    Current = currentProc,
                };

                retval.Add(fullProc);
            }

            return retval;
        }

        protected void ProcOneSpell(GameState gs, Spell spell, Unit caster, Unit target, int procScale, int procDepth, CastResult cr)
        {
            SkillType skillType = gs.data.GetGameData<SpellSettings>().GetSkillType(spell.SkillTypeId);

            if (skillType.TargetTypeId != TargetType.Enemy)
            {
                target = caster;
            }
            // On proc immediately cast for free.
            // await FullCastSpell(gs, spell, caster, target, position, procScale, procDepth + 1, cr);
        }

        public bool FullTryStartCast(GameState gs, Unit caster, long spellId, string targetId)
        {
            TryCastResult result = TryCast(gs, caster, spellId, targetId, false);

            if (result.State != TryCastState.Ok)
            {
                caster.AddMessage(new ErrorMessage(StrUtils.SplitAlongCapitalLetters(result.State.ToString())));
                if (result.State == TryCastState.TargetDead)
                {
                    caster.AddMessage(new OnTargetIsDead() { UnitId = targetId });
                }
                return false;
            }

            CastingSpell casting = new CastingSpell()
            {
                Spell = result.Spell,
                TargetId = targetId,
                CastingTime = result.Spell.CastingTime / 1000,
                EndCastingTime = DateTime.UtcNow.AddSeconds(1.0 * result.Spell.CastingTime / 1000),
            };

            caster.ActionMessage = casting;

            _messageService.SendMessage(gs, caster, casting, result.Spell.CastingTime);

            OnStartCast onStartCast = caster.GetCachedMessage<OnStartCast>(true);
            onStartCast.CasterId = caster.Id;
            onStartCast.CastingName = result.Spell.Name;
            onStartCast.CastSeconds = result.Spell.CastingTime;
            

            ElementType etype = gs.data.GetGameData<SpellSettings>().GetElementType(result.Spell.ElementTypeId);

            if (etype != null)
            {
                onStartCast.AnimName = etype.CastAnim;
            }
            _messageService.SendMessageNear(gs, caster, onStartCast);

            return true;
        }
    }
}
