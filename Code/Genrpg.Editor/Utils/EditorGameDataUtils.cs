using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Setup;
using Genrpg.ServerShared.GameDatas.Services;
using Genrpg.ServerShared.GameDatas;
using Genrpg.ServerShared.Setup;
using Genrpg.ServerShared;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Genrpg.ServerShared.Config;
using System.Threading;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.GameDatas.Interfaces;
using Microsoft.Identity.Client;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Azure;

namespace Genrpg.Editor.Utils
{
    public static class EditorGameDataUtils
    {
        public static async Task<FullGameDataCopy> LoadFullGameData(Form form, string env, CancellationToken token)
        {

            ServerConfig serverConfig = await ConfigUtils.SetupServerConfig(token, "Editor");
            serverConfig.Env = env;
            EditorGameState gs = await SetupUtils.SetupFromConfig<EditorGameState>(form, "Editor", new EditorSetupService(), EditorGameState.CTS.Token, serverConfig);

            FullGameDataCopy dataCopy = new FullGameDataCopy();
            dataCopy.Configs = await gs.repo.Search<DataConfig>(x => true);

            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            List<IGameDataLoader> allLoaders = gameDataService.GetAllLoaders();

            foreach (IGameDataLoader loader in allLoaders)
            {
                List<BaseGameData> allSettings = await loader.LoadAll(gs.repo);
                foreach (BaseGameData data in allSettings)
                {
                    dataCopy.Data.Add(loader.CreateContainer(data));
                }
            }
            return dataCopy;
        }

        public static async Task SaveFullGameData(Form form, FullGameDataCopy dataCopy, string env, CancellationToken token)
        {

            ServerConfig serverConfig = await ConfigUtils.SetupServerConfig(token, "Editor");
            serverConfig.Env = env;
            EditorGameState gs = await SetupUtils.SetupFromConfig<EditorGameState>(form, "Editor", new EditorSetupService(), EditorGameState.CTS.Token, serverConfig);

            foreach (DataConfig config in dataCopy.Configs)
            {
                await gs.repo.Save(config);
            }

            foreach (IGameDataContainer container in dataCopy.Data)
            {
                await container.SaveData(gs.repo);
            }
        }

        const string GitOffsetPath = "..\\..\\..\\..\\..\\..\\GameData";
        public static void WriteGameDataToDisk(FullGameDataCopy dataCopy)
        {

            string dirName = Directory.GetCurrentDirectory();

            dirName += GitOffsetPath;

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            foreach (DataConfig dataConfig in dataCopy.Configs)
            {
                WriteGameDataText(dirName, dataConfig);
            }

            foreach (IGameDataContainer container in dataCopy.Data)
            {
                WriteGameDataText(dirName, container.GetData());
            }
        }
        private static void WriteGameDataText(string parentPath, object objectToSave)
        {
            IStringId idObj = objectToSave as IStringId;

            if (idObj == null)
            {
                return;
            }

            string subpath = objectToSave.GetType().Name.ToLower();

            string fullDir = Path.Combine(parentPath, subpath);

            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }

            string fullPath = Path.Combine(fullDir, idObj.Id);
            File.WriteAllText(fullPath, SerializationUtils.Serialize(idObj));
        }

        public static async Task<FullGameDataCopy> LoadDataFromDisk(Form form, CancellationToken token)
        {
            ServerConfig serverConfig = await ConfigUtils.SetupServerConfig(token, "Editor");
            serverConfig.Env = EnvNames.Dev;
            EditorGameState gs = await SetupUtils.SetupFromConfig<EditorGameState>(form, "Editor", new EditorSetupService(), EditorGameState.CTS.Token, serverConfig);

            FullGameDataCopy dataCopy = new FullGameDataCopy();

            string mainDirName = Directory.GetCurrentDirectory();

            mainDirName += GitOffsetPath;

            if (!Directory.Exists(mainDirName))
            {
                Directory.CreateDirectory(mainDirName);
            }


            List<IGameDataLoader> allLoaders = gs.loc.Get<IGameDataService>().GetAllLoaders();

            string[] fullDirectoryNames = Directory.GetDirectories(mainDirName);

            List<string> directoryNames = new List<string>();

            foreach (string fullName in fullDirectoryNames)
            {
                directoryNames.Add(fullName.Replace(mainDirName + "\\", ""));
            }

            foreach (string subDirName in directoryNames)
            {
                string fullDirectoryName = Path.Combine(mainDirName, subDirName);

                if (!Directory.Exists(fullDirectoryName))
                {
                    continue;
                }                

                string[] fileNames = Directory.GetFiles(fullDirectoryName);

                List<string> allFiles = new List<string>();

                foreach (string file in fileNames)
                {
                    allFiles.Add(File.ReadAllText(Path.Combine(fullDirectoryName, file)));
                }
                    
                if (subDirName == typeof(DataConfig).Name.ToLower())
                {
                    foreach (string fileData in allFiles)
                    {
                        dataCopy.Configs.Add(SerializationUtils.Deserialize<DataConfig>(fileData));
                    }
                }
                else
                {
                    IGameDataLoader loader = allLoaders.FirstOrDefault(x=>x.GetTypeName().ToLower() == subDirName.ToLower());
                    if (loader == null)
                    {
                        throw new Exception("Could not find data loader for typename: " + subDirName);
                    }

                    foreach (string fileData in allFiles)
                    {
                        dataCopy.Data.Add(loader.CreateContainer(loader.Deserialize(fileData)));
                    }
                }
            }
            return dataCopy;
        }
    }
}
