using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMessaging
{
    public class MapMessagePackage
    {
        public MapObject mapObject;
        public IMapMessage message;
        public IMapMessageHandler handler;
        public float delaySeconds = 0;

        public void Process(GameState gs)
        {
            handler.Process(gs, this);
        }

        public void SendError(GameState gs, MapObject obj, string errorText)
        {
            obj.AddMessage(new ErrorMessage(errorText));
        }

        public void Clear()
        {
            mapObject = null;
            message = null;
            handler = null;
        }
    }
}
