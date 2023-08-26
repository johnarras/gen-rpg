
using Genrpg.Shared.Core.Entities;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using UI.Screens.Constants;

public interface IScreenService : IService
{
    void Open(UnityGameState gs, ScreenId name, object data = null);
    void StringOpen(UnityGameState gs, string screenName, object data = null);
    void Close(UnityGameState gs, ScreenId name);
    void FinishClose(UnityGameState gs, ScreenId name);

    void StartUpdates();

    ActiveScreen GetScreen(UnityGameState gs, ScreenId name);

    List<ActiveScreen> GetScreensNamed(UnityGameState gs, ScreenId name);

    List<ActiveScreen> GetAllScreens(UnityGameState gs);

    void CloseAll(UnityGameState gs, List<ScreenId> ignoreScreens = null);

    object GetDragParent();

}

