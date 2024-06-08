
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.Threading;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spells.SpellEffectHandlers;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Targets.Messages;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.Utils;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Skills;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spells.PlayerData;

namespace Genrpg.MapServer.Spells.Services
{
    public class ServerSpellService : IServerSpellService
    {
        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IStatService _statService = null;
        private IAIService _aiService = null;
        private IGameData _gameData = null;
        protected Dictionary<long, ISpellEffectHandler> _handlers = null;


        protected Dictionary<TryCastState, string> _tryCastText;
        public async Task Initialize(IGameState gs, CancellationToken token)
        {
            _handlers = ReflectionUtils.SetupDictionary<long, ISpellEffectHandler>(gs);

            foreach (ISpellEffectHandler handler in _handlers.Values)
            {
                handler.Init();
            }

            _tryCastText = new Dictionary<TryCastState, string>();
            foreach (TryCastState state in Enum.GetValues<TryCastState>())
            {
                _tryCastText[state] = StrUtils.SplitOnCapitalLetters(state.ToString());
            }

            await Task.CompletedTask;
        }

        private void SetResultState(TryCastResult result, TryCastState state)
        {
            result.State = state;
            result.StateText = _tryCastText[state];
        }

        public TryCastResult TryCast(IRandom rand, Unit caster, long spellId, string targetId, bool endOfCast)
        {
            TryCastResult result = new TryCastResult();

            if (string.IsNullOrEmpty(targetId))
            {
                SetResultState(result, TryCastState.TargetMissing);
                return result;
            }

            if (caster.IsDeleted())
            {
                SetResultState(result, TryCastState.CasterDeleted);
                return result;
            }
            else if (caster.HasFlag(UnitFlags.IsDead))
            {
                SetResultState(result, TryCastState.CasterDead);
                return result;
            }

            if (caster.HasFlag(UnitFlags.Evading))
            {
                SetResultState(result, TryCastState.CasterEvading);
                return result;
            }

            if (!endOfCast && caster.ActionMessage != null && !caster.ActionMessage.IsCancelled())
            {
                CastingSpell castingSpell = caster.ActionMessage as CastingSpell;
                // Only mark caster as busy if the spell has not reached its end casting time...
                // potential fix for stuck spells
                if (castingSpell == null || DateTime.UtcNow < castingSpell.EndCastingTime)
                {
                    SetResultState(result, TryCastState.CasterBusy);
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
                SetResultState(result, TryCastState.NoSpellData);
                return result;
            }
            Spell spell = spellData.Get(spellId);

            if (spell == null)
            {
                SetResultState(result, TryCastState.DoNotKnowSpell);
                return result;
            }
            if (spell.HasFlag(SpellFlags.IsPassive))
            {
                SetResultState(result, TryCastState.IsPassive);
                return result;
            }

            ElementType elementType = _gameData.Get<ElementTypeSettings>(caster).Get(spell.ElementTypeId);

            if (elementType == null)
            {
                SetResultState(result, TryCastState.UnknownElement);
                return result;
            }

            if (spell.CooldownEnds > DateTime.UtcNow)
            {
                SetResultState(result, TryCastState.OnCooldown);
                return result;
            }


            foreach (SpellEffect effect in spell.Effects)
            {

                SkillType skillType = _gameData.Get<SkillTypeSettings>(caster).Get(effect.SkillTypeId);

                if (skillType == null)
                {
                    SetResultState(result, TryCastState.UnknownSkill);
                    return result;
                }
            }

            if (caster.Stats.Curr(spell.PowerStatTypeId) < spell.GetCost(caster))
            {
                SetResultState(result, TryCastState.NotEnoughPower);
                return result;
            }

            TargetCastState targState = GetTargetState(rand, spell, targetId);

            if (targState.State != TryCastState.Ok)
            {

                SetResultState(result, targState.State);
                return result;
            }

            if (caster.DistanceTo(targState.Target) > spell.GetRange())
            {
                SetResultState(result, TryCastState.TargetTooFar);
                return result;
            }

            result.Spell = spell;
            result.Target = targState.Target;
            result.ElementType = elementType;

            if (caster is Character ch)
            {
                AbilityData adata = ch.Get<AbilityData>();
            }

            SetResultState(result, TryCastState.Ok);
            return result;
        }
        public TargetCastState GetTargetState(IRandom rand, Spell spell, string unitId)
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


        public void SendStopCast(IRandom rand, MapObject obj)
        {

            OnStopCast stop = obj.GetCachedMessage<OnStopCast>(true);
            stop.CasterId = obj.Id;
            _messageService.SendMessageNear(obj, stop);
        }

        public void SendSpell(IRandom rand, Unit caster, TryCastResult result)
        {
            // Creating and sending projectiles

            SendSpell sendSpell = new SendSpell()
            {
                CasterId = caster.Id,
                CasterGroupId = caster.GetGroupId(),
                CasterStats = caster.Stats,
                CasterLevel = caster.Level,
                CasterFactionId = caster.FactionTypeId,
                Spell = result.Spell,
                ElementType = result.ElementType,
            };

            SendOneSpell(rand, caster, result.Target, sendSpell, true);
        }

        public void ResendSpell(IRandom rand, Unit caster, Unit target, SendSpell sendSpell)
        {
            SendOneSpell(rand, caster, target, sendSpell, false);
        }

        private void SendOneSpell(IRandom rand, Unit caster, Unit target, SendSpell sendSpell, bool isFirstSend)
        {
            float distance1 = MapObjectUtils.DistanceBetween(caster, target);

            float duration1 = distance1 / SpellConstants.ProjectileSpeed;

            if (target.Moving &&
                (target.FinalX != target.X ||
                target.Z != target.FinalZ))
            {
                float tdx = target.FinalX - target.X;
                float tdz = target.FinalZ - target.Z;

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

            _messageService.SendMessage(target, sendSpell, duration1);


            caster.ActionMessage = null;

            if (duration1 > 0)
            {
                ShowProjectile(rand, caster, target, sendSpell.Spell, FXNames.Projectile, SpellConstants.ProjectileSpeed);
            }
            else
            {
                ShowFX(rand, caster.Id, target.Id, sendSpell.Spell.ElementTypeId, FXNames.Projectile, 0);
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
            //    _ms.AddMessage(rand, caster, resend, SpellUtils.GetResendDelay(sendSpell.Spell.HasFlag(SpellFlags.InstantHit)));
            //}
        }

        public void ShowFX(IRandom rand, string fromUnitId, string toUnitId, long elementTypeId, string fxName, float duration)
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

            ElementType etype = _gameData.Get<ElementTypeSettings>(null).Get(elementTypeId);
            if (etype != null)
            {
                fx.Art = etype.Art + fxName;
            }

            if (_objectManager.GetUnit(fromUnitId, out Unit fromUnit))
            {
                _messageService.SendMessageNear(fromUnit, fx);
            }
        }


        public void ShowProjectile(IRandom rand, Unit fromUnit, Unit toUnit, Spell spell, string fxName, float speed)
        {
            FX fx = new FX()
            {
                From = fromUnit.Id,
                To = toUnit.Id,
                Dur = 0,
                Speed = speed,
            };

            ElementType etype = _gameData.Get<ElementTypeSettings>(fromUnit).Get(spell.ElementTypeId);
            if (etype != null)
            {
                fx.Art = etype.Art + fxName;
            }

            _messageService.SendMessageNear(fromUnit, fx);
        }

        public void OnSendSpell(IRandom rand, Unit origTarget, SendSpell sendSpell)
        {

            foreach (SpellEffect effect in sendSpell.Spell.Effects)
            {
                List<SpellHit> hits = GetTargetsHit(rand, origTarget, sendSpell, effect);

                foreach (SpellHit hit in hits)
                {
                    _messageService.SendMessage(hit.Target, hit);
                }
            }
        }

        /// <summary>
        /// This creates a list of spell hit objects for each victim hit.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="spellData"></param>
        /// <returns></returns>
        protected List<SpellHit> GetTargetsHit(IRandom rand, Unit origTarget, SendSpell sendSpell, SpellEffect effect)
        {
            List<SpellHit> hits = new List<SpellHit>();

            if (!_objectManager.GetObject(sendSpell.CasterId, out MapObject caster))
            {
                return hits;
            }

            SkillType skillType = _gameData.Get<SkillTypeSettings>(caster).Get(effect.SkillTypeId);
            ElementType elementType = _gameData.Get<ElementTypeSettings>(caster).Get(sendSpell.Spell.ElementTypeId);

            long targetTypeId = skillType.TargetTypeId;
            long casterFactionId = sendSpell.CasterFactionId;
            if (targetTypeId == TargetTypes.None)
            {
                return hits;
            }

            List<Unit> primaryTargets = new List<Unit>();
            List<Unit> targets = new List<Unit>();

            primaryTargets.Add(origTarget);
            targets.Add(origTarget);

            if (effect.ExtraTargets > 0)
            {
                List<Unit> newTargets = GetTargetUnitsNear(rand, origTarget.X, origTarget.Z, origTarget, SpellConstants.ExtraTargetRadius,
                    casterFactionId, targetTypeId);

                // Targets for spells are first distinct (since lockless read multithreaded movement allows for
                // dupe messages a small amount of the time, and then not dupe the primary targets, 
                // then randomize, then take effect.ExtraTargets of them. and add them to the 

                newTargets = newTargets
                    .Except(primaryTargets)
                    .Distinct()
                    .OrderBy(x => Guid.NewGuid())
                    .Take(effect.ExtraTargets)
                    .ToList();

                primaryTargets.AddRange(newTargets);
                targets.AddRange(newTargets);
            }

            // Maybe someday we expand this to be TargetShape.
            if (effect.Radius > 0 && primaryTargets.Count > 0)
            {
                foreach (Unit ptarg in primaryTargets)
                {
                    List<Unit> newTargets = GetTargetUnitsNear(rand, ptarg.X, ptarg.Z, ptarg, effect.Radius,
                        casterFactionId, targetTypeId);

                    newTargets = newTargets.Distinct().ToList();
                    newTargets.Remove(ptarg);
                    targets.AddRange(newTargets);
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
                    Id = rand.NextLong(),
                    OrigTarget = origTarget,
                    Target = targ,
                    SendSpell = sendSpell,
                    Effect = effect,
                    ElementType = elementType,
                    SkillType = skillType,
                };

                if (primaryTargets.Contains(targ))
                {
                    newHit.PrimaryTarget = true;
                }
                hits.Add(newHit);
            }
            return hits;
        }

        protected List<Unit> GetTargetUnitsNear(IRandom rand, float x, float z, Unit origTarget, float radius,
            long casterFactionId, long targetTypeId)
        {
            List<Unit> newTargets = _objectManager.GetTypedObjectsNear<Unit>(x, z, origTarget, radius, true);

            newTargets = newTargets.Where(unit => SpellUtils.IsValidTarget(unit, casterFactionId, targetTypeId)).ToList();

            if (newTargets.Contains(origTarget))
            {
                newTargets.Remove(origTarget);
            }

            return newTargets;
        }

        public void OnSpellHit(IRandom rand, SpellHit hit)
        {
            List<ActiveSpellEffect> effects = CalcSpellEffects(rand, hit);

            foreach (ActiveSpellEffect eff in effects)
            {
                if (!_objectManager.GetUnit(eff.TargetId, out Unit unit))
                {
                    continue;
                }
                _messageService.SendMessage(unit, eff);
            }
        }

        /// <summary>
        /// Calculate what happens wrt one spell vs one target
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="castData"></param>
        /// <param name="hitData"></param>
        /// <param name="cr"></param>
        public List<ActiveSpellEffect> CalcSpellEffects(IRandom rand, SpellHit hit)
        {
            List<ActiveSpellEffect> retval = new List<ActiveSpellEffect>();
            SendSpell sendSpell = hit.SendSpell;
            Spell spell = sendSpell.Spell;
            SpellEffect effect = hit.Effect;

            ElementType elementType = hit.ElementType;
            SkillType skillType = hit.SkillType;
            long elementRank = 0;
            long skillRank = 0;
            long level = sendSpell.CasterLevel;

            double elemSkillScalePct = elementType.GetScalePct(skillType.IdKey) / 100.0f;

            double skillBaseScalePct = skillType.StatScalePercent / 100.0f;
            int skillRankScale = 1;
            int elemRankScale = 1;

            long baseQuantity = 0;
            long defenseQuantity = 0;
            long maxQuantity = 0;
            long finalQuantity = 0;

            if (!GetSpellEffectHandler(skillType.EffectEntityTypeId, out ISpellEffectHandler handler))
            {
                return retval;
            }

            if (handler.UseStatScaling())
            {
                long scalingStatId = skillType.ScalingStatTypeId;
                if (scalingStatId < StatTypes.DamagePower || scalingStatId >= StatTypes.DamagePower + 10)
                {
                    scalingStatId = StatTypes.DamagePower;
                }

                long powerOffset = scalingStatId - StatTypes.DamagePower;

                StatGroup cStats = sendSpell.CasterStats;

                long powerStat = cStats.Max(scalingStatId);

                double statPowerMult = cStats.Max(StatTypes.PowerMult + powerOffset);

                if (powerOffset > 0)
                {
                    powerStat += cStats.Max(StatTypes.DamagePower);
                    statPowerMult += cStats.Max(StatTypes.PowerMult);
                }

                statPowerMult = 1 + statPowerMult / 100.0f;

                double trainedRankScale = 1 + (elementRank * elemRankScale + skillRank * skillRankScale) / 100.0f;

                double primaryScale = effect.Scale / 100.0f;

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
                if (hit.Target != null && skillType.TargetTypeId == TargetTypes.Enemy)
                {
                    StatGroup tStats = hit.Target.Stats;
                    float defenseScaleDown = tStats.ScaleDown(StatTypes.Defense + powerOffset);
                    long defenseMult = tStats.Max(StatTypes.DefenseMult + powerOffset);

                    if (powerOffset > 0)
                    {
                        defenseScaleDown *= tStats.ScaleDown(StatTypes.Defense);
                        defenseMult += tStats.Max(StatTypes.DefenseMult);
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

                hit.CritChance = cStats.Pct(StatTypes.Crit);
                hit.CritMult = 2.0f + cStats.Pct(StatTypes.CritDam);


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

            return handler.CreateEffects(rand, hit);

        }

        public void ShowCombatText(Unit unit, string txt, int combatTextColorId, bool isCrit = false)
        {
            CombatText combatText = new CombatText()
            {
                TargetId = unit.Id,
                Text = txt,
                TextColor = combatTextColorId,
                IsCrit = isCrit,
            };
            _messageService.SendMessageNear(unit, combatText);
        }




        public void ApplyOneEffect(IRandom rand, ActiveSpellEffect eff)
        {

            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ))
            {
                return;
            }

            if (eff.Id < 0)
            {
                eff.Id = rand.NextLong();
            }

            bool isImmune = targ.IsFullImmune();

            if (!isImmune && eff.Radius > 0 && eff.IsOrigTarget)
            {
                ShowFX(rand, eff.TargetId, eff.TargetId, eff.ElementTypeId, FXNames.Explosion, 2.0f);
            }

            bool allySpell = false;
            long targetTypeId = _gameData.Get<SkillTypeSettings>(null).Get(eff.SkillTypeId).TargetTypeId;
            if (targetTypeId == TargetTypes.Ally)
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
            if (eff.DurationLeft < eff.MaxDuration)
            {
                fxName = FXNames.DoT;
                fxName = "";
            }
            ShowFX(rand, eff.TargetId, eff.TargetId, eff.ElementTypeId, fxName, 2.0f);

            if (!GetSpellEffectHandler(eff.EntityTypeId, out ISpellEffectHandler handler))
            {
                return;
            }

            if (!handler.HandleEffect(rand, eff))
            {
                eff.SetCancelled(true);
                return;
            }

            if (targetTypeId == TargetTypes.Enemy)
            {
                if (!targ.HasTarget())
                {
                    _aiService.TargetMove(rand, targ, eff.CasterId);
                    _aiService.BringFriends(rand, targ, eff.CasterId); // When a target is attacked, it brings friends
                }
                if (!targ.HasFlag(UnitFlags.IsDead) && _objectManager.GetChar(eff.CasterId, out Character chCaster))
                {
                    targ.AddAttacker(eff.CasterId, eff.CasterGroupId);
                }
            }

            // If this is the first tick, add the effect.
            if (eff.MaxDuration > 0 && !isImmune && eff.DurationLeft == eff.MaxDuration)
            {
                AddEffect(rand, eff);
            }
            // If it's the last tick, remove it.
            else if (eff.DurationLeft < 1 || isImmune)
            {
                RemoveEffect(rand, eff);
            }
            // Otherwise middle tick, do nothing.
            else
            {

            }

            if (!isImmune)
            {
                if (targetTypeId == TargetTypes.Enemy && !string.IsNullOrEmpty(fxName))
                {
                    ShowFX(rand, eff.TargetId, eff.TargetId, eff.ElementTypeId, fxName, 1);
                }
            }

            if (eff.MaxDuration > 0 && eff.DurationLeft > 0)
            {
                float tickLength = handler.GetTickLength();
                if (tickLength > eff.DurationLeft)
                {
                    tickLength = eff.DurationLeft;
                }
                eff.DurationLeft -= tickLength;
                _messageService.SendMessage(targ, eff, tickLength);
            }
        }


        protected void AddEffect(IRandom rand, ActiveSpellEffect eff)
        {

            if (!_objectManager.GetUnit(eff.TargetId, out Unit unit))
            {
                return;
            }

            if (unit.Effects == null)
            {
                unit.Effects = new List<IDisplayEffect>();
            }


            IDisplayEffect currEffectData = unit.Effects.FirstOrDefault(x => x.MatchesOther(eff));


            bool canAddNew = true;
            if (currEffectData != null)
            {
                if (Math.Abs(currEffectData.Quantity) <= Math.Abs(eff.Quantity))
                {
                    RemoveEffect(rand, currEffectData);
                }
                else if (eff.DurationLeft > currEffectData.DurationLeft)
                {
                    currEffectData.MaxDuration = eff.MaxDuration;
                    currEffectData.DurationLeft = eff.DurationLeft;
                    OnUpdateEffect updateEff = new OnUpdateEffect()
                    {
                        Id = currEffectData.Id,
                        Duration = currEffectData.MaxDuration,
                        DurationLeft = currEffectData.DurationLeft,
                    };
                    _messageService.SendMessageNear(unit, updateEff);
                    canAddNew = false;
                }
                else
                {
                    canAddNew = false;
                }
            }

            if (canAddNew)
            {

                unit.Effects.Add(eff);

                OnAddEffect addEffect = new OnAddEffect()
                {
                    Id = eff.Id,
                    Duration = eff.MaxDuration,
                    DurationLeft = eff.DurationLeft,
                    EntityTypeId = eff.EntityTypeId,
                    EntityId = eff.EntityId,
                    Quantity = eff.Quantity,
                    Icon = eff.Icon,
                    TargetId = eff.TargetId,
                };

                _messageService.SendMessageNear(unit, addEffect);
            }

            _statService.CalcStats(unit, false);

        }


        protected void RemoveEffect(IRandom rand, IDisplayEffect timedData)
        {

            ActiveSpellEffect effData = timedData as ActiveSpellEffect;

            if (!_objectManager.GetUnit(effData.TargetId, out Unit unit))
            {
                return;
            }

            if (unit.Effects == null || !unit.Effects.Contains(effData))
            {
                return;
            }

            unit.Effects.Remove(effData);
            effData.SetCancelled(true);

            OnRemoveEffect onRemove = new OnRemoveEffect()
            {
                TargetId = effData.TargetId,
                Id = effData.Id,
            };

            _messageService.SendMessageNear(unit, onRemove);

            _statService.CalcStats(unit, false);
        }

        public bool FullTryStartCast(IRandom rand, Unit caster, long spellId, string targetId)
        {
            TryCastResult result = TryCast(rand, caster, spellId, targetId, false);

            if (result.State != TryCastState.Ok)
            {
                caster.AddMessage(new ErrorMessage(result.StateText));
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
                CastingTime = (float)result.Spell.CastTime,
                EndCastingTime = DateTime.UtcNow.AddSeconds(1.0 * result.Spell.CastTime / 1000),
            };

            caster.ActionMessage = casting;

            _messageService.SendMessage(caster, casting, result.Spell.CastTime);

            OnStartCast onStartCast = caster.GetCachedMessage<OnStartCast>(true);
            onStartCast.CasterId = caster.Id;
            onStartCast.CastingName = result.Spell.Name;
            onStartCast.CastSeconds = result.Spell.CastTime;


            ElementType etype = _gameData.Get<ElementTypeSettings>(caster).Get(result.Spell.ElementTypeId);

            if (etype != null)
            {
                onStartCast.AnimName = etype.CastAnim;
            }
            _messageService.SendMessageNear(caster, onStartCast);

            return true;
        }
    }
}
