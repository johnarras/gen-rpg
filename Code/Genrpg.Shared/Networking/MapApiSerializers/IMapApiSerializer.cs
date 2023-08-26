using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.MapApiSerializers
{
    public interface IMapApiSerializer
    {
        byte[] Serialize(List<IMapApiMessage> messages);
        List<IMapApiMessage> Deserialize(byte[] bytes, int byteCount);
    }
}
