using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.GameSettings.Services
{
    public interface IClientGameDataService : IInitializable
    {
        UniTask SaveSettings(UnityGameState gs, IGameSettings settings);

        UniTask LoadCachedSettings(UnityGameState gs);
    }
}
