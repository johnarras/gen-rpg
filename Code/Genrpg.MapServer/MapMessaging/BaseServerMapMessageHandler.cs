
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Spells;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.ServerShared.CloudComms.Services;

namespace Genrpg.MapServer.MapMessaging
{
    public abstract class BaseServerMapMessageHandler<T> : IMapMessageHandler where T : class, IMapMessage
    {
        public Type GetKey() { return typeof(T); }

        protected IMapMessageService _messageService = null;
        protected IMapObjectManager _objectManager = null;
        protected IServerSpellService _spellService = null;
        protected IAIService _aiService = null;
        protected IEntityService _entityService = null;
        protected ICloudCommsService _cloudCommsService = null;
        public virtual void Setup(GameState gs)
        {
        }


        protected abstract void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, T message);

        protected virtual bool GetOkUnit(MapObject obj, bool playersOk, out Unit unit)
        {
            if (!(obj is Unit objUnit))
            {
                unit = null;
                return false;
            }

            if (!playersOk && obj.IsPlayer())
            {
                unit = null;
                return false;
            }

            if (obj.IsDeleted())
            {
                unit = null;
                return false;
            }

            if (objUnit.HasFlag(UnitFlags.IsDead))
            {
                unit = null;
                return false;
            }
            unit = objUnit;
            return true;
        }

        public void Process(GameState gs, MapMessagePackage pack)
        {
            if (!pack.message.IsCancelled())
            {
                InnerProcess(gs, pack, pack.mapObject, pack.message as T);
            }
        }
    }
}
