using Genrpg.MapServer.Maps;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Trades.Entities;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Trades.Services
{
    public interface ITradeService : IInjectable
    {
        void HandleStartTrade(Character ch, StartTrade startTrade);
        void HandleCancelTrade(Character ch, CancelTrade cancelTrade);
        void HandleOnCancelTrade(Character ch, OnCancelTrade message);
        void HandleAcceptTrade(Character ch, AcceptTrade acceptTrade);
        void HandleOnAcceptTrade(Character ch, OnAcceptTrade message);
        void HandleUpdateTrade(Character ch, UpdateTrade updateTrade);
        void HandleOnUpdateTrade(Character ch, OnUpdateTrade message);
        void HandleOnCompleteTrade(Character ch, OnCompleteTrade message);
        T SafeModifyObject<T>(MapObject obj, Func<T> modifyFunc, T failureResult);
        void SafeModifyObject(MapObject obj, Action modifyFunc);
    }
}
