using Genrpg.MapServer.MainServer;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Settings.Settings;

namespace Tests.MapServerTests
{
    [TestClass]
    public class TestDataLoadSave
    {
        const string serverName = "mapservertests";


        private IGameDataService _gameDataService = null!;
        private IGameData _gameData = null!;

        [TestMethod]
        public async Task TestLoadSettings()
        {
            MapServerMain? mapServer = await MapServerTestSetup.GetMapServer(serverName, this);

            Assert.IsNotNull(mapServer, "Map Server Was Null");

            ServerGameState gs = mapServer.GetServerGameState();

            List<IGameSettingsLoader> allLoaders = _gameDataService.GetAllLoaders();

            List<ITopLevelSettings> allSettings = _gameData.AllSettings();



            foreach (IGameSettingsLoader loader in allLoaders)
            {
                List<ITopLevelSettings> currSettings = allSettings.Where(x=>x.GetType() == loader.GetServerType()).ToList();

                Assert.IsNotNull(currSettings.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename), "Missing Default setting for " +
                    loader.GetServerType());

            }


            List<DataOverrideSettings> allDataOverrides = allSettings.Where(x => x.GetType() == typeof(DataOverrideSettings)).Cast<DataOverrideSettings>().ToList();

            Assert.IsTrue(allDataOverrides.Count == 1 && allDataOverrides[0].Id == GameDataConstants.DefaultFilename,
                "There must be exactly one DataOverrideSettings and its Id must be " + GameDataConstants.DefaultFilename);

            DataOverrideSettings defaultOverrides = allDataOverrides[0];


            List<SettingsNameSettings> allNameSettings = allSettings.Where(x => 
            x.GetType() == typeof(SettingsNameSettings)).Cast<SettingsNameSettings>().ToList();


            Assert.IsTrue(allNameSettings.Count == 1 && allNameSettings[0].Id == GameDataConstants.DefaultFilename,
                "There must be exactly one SettingsNameSettings and its Id must be " + GameDataConstants.DefaultFilename);

            SettingsNameSettings defaultNames = allNameSettings[0];

            List<string> otherSettingsNeeded = new List<string>();

            foreach (DataOverrideGroup overrideGroup in defaultOverrides.GetData())
            {
                foreach (DataOverrideItem dataItem in overrideGroup.Items)
                {
                    SettingsName name = defaultNames.Get(dataItem.SettingsNameId);

                    Assert.IsNotNull(name, "SettingName for Id " + dataItem.SettingsNameId + " does not exist.");

                    Assert.IsTrue(!string.IsNullOrEmpty(name.Name), "SettingsName " + name.IdKey + " has an empty type name.");

                    Assert.IsTrue(!string.IsNullOrEmpty(dataItem.DocId), "Empty doc Id on setting for Setting " + name.Name);

                    ITopLevelSettings? overrideSetting = allSettings.FirstOrDefault(X => X.Id == dataItem.DocId &&
                    X.GetType().Name == name.Name);

                    Assert.IsNotNull(overrideSetting, "Data override for type " + name.Name + " with doc Id " + dataItem.DocId + " is missing");
                }
            }
        }
    }
}
