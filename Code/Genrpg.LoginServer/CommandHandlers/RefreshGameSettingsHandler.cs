using Genrpg.LoginServer.Core;
using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.ServerShared.GameSettings.Interfaces;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Login.Messages.RefreshGameData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class RefreshGameSettingsHandler : BaseLoginCommandHandler<RefreshGameSettingsCommand>
    {
        private IGameDataService _gameDataService = null;

        protected override async Task InnerHandleMessage(LoginGameState gs, RefreshGameSettingsCommand command, CancellationToken token)
        {
            RefreshGameSettingsResult result = new RefreshGameSettingsResult();

            List<IGameSettings> newSettings = new List<IGameSettings>();


            gs.ch = await gs.repo.Load<Character>(command.CharId);

            GameDataOverrideData overrideData = await gs.repo.Load<GameDataOverrideData>(command.CharId);
            if (overrideData == null)
            {
                overrideData = new GameDataOverrideData() { Id = command.CharId };
            }

            overrideData.AddTo(gs.ch);

            _gameDataService.SetGameDataOverrides(gs, gs.ch, true);

            GameDataOverrideList overrideList = gs.ch.GetGameDataOverrideList();

            List<IGameSettings> allSettings = gs.data.GetAllData();
           
            foreach (IGameSettings settings in allSettings)
            {
                BaseGameSettings baseSettings = settings as BaseGameSettings;

                if (baseSettings.Id == GameDataConstants.DefaultFilename)
                {
                    // If it's default data saved after the current save time, then send it to the client.
                    if (baseSettings.UpdateTime >= gs.data.CurrSaveTime)
                    {
                        newSettings.Add(settings);
                    }
                }
                else // Send all A/B test data to the client
                {
                    PlayerSettingsOverrideItem overrideItem = overrideList.Items.FirstOrDefault(x => x.SettingId == settings.GetType().Name &&
                    x.DocId == settings.Id);

                    if (overrideItem != null)
                    {
                        newSettings.Add(settings);
                    }
                }
            }

            await gs.repo.Save(overrideData);

            result.Overrides = overrideList;
            result.NewSettings = _gameDataService.MapToApi(gs, newSettings);

            gs.Results.Add(result);

        }
    }
}
