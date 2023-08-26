using Genrpg.MapServer.Maps;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Units;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spells.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Spells.Messages;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public abstract class BaseSpellEffectHandler : ISpellEffectHandler
    {

        protected IServerSpellService _spellService = null;
        protected IMapMessageService _messageService = null;
        protected IMapObjectManager _objectManager = null;
        protected IServerUnitService _unitService = null;
        protected IAIService _aiService = null;
        protected IStatService _statService = null;
        public virtual void Init(GameState gs)
        {
        }
        public virtual float GetTickLength() { return SpellConstants.BaseTickSeconds; }
        public abstract List<SpellEffect> CreateEffects(GameState gs, SpellHit spellHit);
        public abstract long GetKey();
        public abstract bool HandleEffect(GameState gs, SpellEffect eff);
        public abstract bool IsModifyStatEffect();
        public abstract bool UseStatScaling();
    }
}
