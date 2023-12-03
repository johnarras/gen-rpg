using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GameSettings.Services
{
    public interface IClientGameDataService : ISetupService
    {
        Task SaveSettings(UnityGameState gs, IGameSettings settings);

        Task LoadCachedSettings(UnityGameState gs);
    }
}
