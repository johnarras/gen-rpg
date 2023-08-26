using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.MapApiSerializers.Serializers
{
    public class JsonMapApiSerializer : IMapApiSerializer
    {
        public List<IMapApiMessage> Deserialize(byte[] bytes, int byteCount)
    {
            string messages = Encoding.UTF8.GetString(bytes, 0, byteCount).Trim();

            return SerializationUtils
                .Deserialize<List<IMapApiMessage>>(messages);
        }

        public byte[] Serialize(List<IMapApiMessage> messages)
        {
            return Encoding.UTF8.GetBytes(SerializationUtils.Serialize(messages));
        }
    }
}
