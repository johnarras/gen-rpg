using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Assets.Scripts.GameSettings.Services
{
    public interface IClientGameDataService : IInitializable
    {
        Awaitable SaveSettings(IGameSettings settings);

        Awaitable LoadCachedSettings(IClientGameState gs);

    }
}
