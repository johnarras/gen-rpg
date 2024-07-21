using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Casting
{
    public class SpecialSpellCastingStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SpecialSpellCast; }
        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            if (selectSpellAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing Special Select Spell" };
            }

            CrawlerSpell spell = selectSpellAction.Spell;

            CrawlerSpellEffect specialEffect = spell.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.SpecialMagic);

            if (specialEffect == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing Special Select Spell Effect" };
            }

            ISpecialMagicHelper helper = _spellService.GetSpecialEffectHelper(specialEffect.EntityId);
            if (helper != null)
            {
                return await helper.HandleEffect(stateData, selectSpellAction, spell, specialEffect, token);
            }
            return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That spell is missing a special effect." };
        }
    }
}
