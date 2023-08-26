using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.MapApiSerializers.Serializers
{
    public class MessagePackMapApiSerializer : IMapApiSerializer
    {
        public List<IMapApiMessage> Deserialize(byte[] bytes, int byteCount)
        {
            return SerializationUtils.BinaryDeserialize<List<IMapApiMessage>>(bytes);
        }

        public byte[] Serialize(List<IMapApiMessage> messages)
        {
            return SerializationUtils.BinarySerialize(messages);
        }
    }
}
