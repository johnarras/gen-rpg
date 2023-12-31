﻿using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Users.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Purchasing.Services
{
    public interface IPurchasingService : ISetupService
    {
        Task<PlayerStoreOfferData> GetCurrentStores(ServerGameState gs, User user, Character ch, bool forceRefresh);
    }
}
