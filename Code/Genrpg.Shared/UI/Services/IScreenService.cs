using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UI.Services
{
    public interface IScreenService : IInitializable
    {
        void Open(ScreenId name, object data = null);
        void StringOpen(string screenName, object data = null);
        void Close(ScreenId name);
        void FinishClose(ScreenId name);

        void StartUpdates();

        ActiveScreen GetScreen(ScreenId name);

        ActiveScreen GetLayerScreen(ScreenLayers layerId);

        List<ActiveScreen> GetScreensNamed(ScreenId name);

        public ActiveScreen GetScreen(string screenName);

        List<ActiveScreen> GetAllScreens();

        void CloseAll(List<ScreenId> ignoreScreens = null);

        object GetDragParent();
        string GetSubdirectory(ScreenId screenName);
        string GetFullScreenNameFromEnum(ScreenId id);

    }

}
