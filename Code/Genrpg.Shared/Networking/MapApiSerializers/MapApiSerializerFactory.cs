using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.MapApiSerializers.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.MapApiSerializers
{
    public class MapApiSerializerFactory
    {
        public static IMapApiSerializer Create(EMapApiSerializers serializer)
        {
            if (serializer == EMapApiSerializers.Json)
            {
                return new JsonMapApiSerializer();
            }
            else if (serializer == EMapApiSerializers.MessagePack)
            {
                return new MessagePackMapApiSerializer();
            }

            throw new Exception("Unknown map message serializer type: " + serializer.ToString());
        }
    }
}
