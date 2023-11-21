using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.Interfaces
{
    public interface IConnection
    {
        void ForceClose();
        void AddMessage(IMapApiMessage message);
        bool RemoveMe();
        ConnMessageCounts GetCounts();
        void Shutdown(Exception e, string message);
    }
}
