﻿using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Setup;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Genrpg.ServerShared.Config;
using System.Threading;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.GameSettings;
using System.Text;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.ServerShared.CloudComms.Constants;
using Microsoft.Azure.Amqp.Framing;
using System.Drawing;

namespace Genrpg.Editor.Utils
{
    public static class EditorGameDataUtils
    {
        public static async Task<FullGameDataCopy> LoadFullGameData(Form form, string env, CancellationToken token)
        {

            EditorGameState gs = await SetupFromConfig(form, env);

            FullGameDataCopy dataCopy = new FullGameDataCopy();

            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            List<IGameSettingsLoader> allLoaders = gameDataService.GetAllLoaders();

            foreach (IGameSettingsLoader loader in allLoaders)
            {
                List<IGameSettings> allSettings = await loader.LoadAll(gs.repo, true);
                foreach (IGameSettings data in allSettings)
                {
                    dataCopy.Data.Add(data);
                }
            }
            return dataCopy;
        }

        public static async Task<EditorGameState> SetupFromConfig (object parent, string env, ServerConfig serverConfig = null)
        {
            if (serverConfig == null)
            {
                serverConfig = await ConfigUtils.SetupServerConfig(EditorGameState.CTS.Token, CloudServerNames.Editor.ToString().ToLower());
            }
            serverConfig.Env = env;
            EditorGameState gs = await SetupUtils.SetupFromConfig<EditorGameState>(parent, CloudServerNames.Editor.ToString().ToLower(), 
                new EditorSetupService(), EditorGameState.CTS.Token, serverConfig);

            List<IGameSettings> allSettings = gs.data.GetAllData();

            foreach (IGameSettings settings in allSettings)
            {
                if (settings is BaseGameSettings baseSettings)
                {
                    if (baseSettings.UpdateTime == DateTime.MinValue)
                    {
                        gs.LookedAtObjects.Add(baseSettings);
                    }
                }
            }

            return gs;
        }

        public static async Task SaveFullGameData(Form form, FullGameDataCopy dataCopy, string env, CancellationToken token)
        {

            EditorGameState gs = await SetupFromConfig(form, env);
            foreach (IGameSettings data in dataCopy.Data)
            {
                await gs.repo.Save(data);
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

            foreach (IGameSettings data in dataCopy.Data)
            {
                WriteGameDataText(dirName, data);

                foreach (IGameSettings child in data.GetChildren())
                {
                    WriteGameDataText(dirName, child);
                }
                RemoveDeletedFiles(dirName, data.GetChildren());
            }
            RemoveDeletedFiles(dirName, dataCopy.Data);


        }

        private static void RemoveDeletedFiles(string parentPath, List<IGameSettings> allSettings)
        {
            if (allSettings.Count < 1)
            {
                return;
            }

            foreach (IGameSettings settings in allSettings)
            {
                string subpath = settings.GetType().Name.ToLower();
                
                string fullDir = Path.Combine(parentPath, subpath);

                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }

                string[] fileNames = Directory.GetFiles(fullDir);

                foreach (string fileName in fileNames)
                {
                    IGameSettings matchingObject = allSettings.FirstOrDefault(x => x.GetType() == settings.GetType() && x.Id == settings.Id);

                    if (matchingObject == null)
                    {
                        string fullPath = Path.Combine(fullDir, fileName);
                        File.Delete(fullPath);
                    }
                }
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

            EditorGameState gs = await SetupFromConfig(form, EnvNames.Dev);

            FullGameDataCopy dataCopy = new FullGameDataCopy();

            List<Type> settingsTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettings));

            string mainDirName = Directory.GetCurrentDirectory();

            mainDirName += GitOffsetPath;

            if (!Directory.Exists(mainDirName))
            {
                Directory.CreateDirectory(mainDirName);
            }

            string[] fullDirectoryNames = Directory.GetDirectories(mainDirName);

            List<string> directoryNames = new List<string>();

            foreach (string fullName in fullDirectoryNames)
            {
                directoryNames.Add(fullName.Replace(mainDirName + "\\", ""));
            }

            foreach (string subDirName in directoryNames)
            {
                Type currType = settingsTypes.FirstOrDefault(x => x.Name.ToLower() == subDirName.ToLower());

                if (currType == null)
                {
                    Console.WriteLine("Unknown IGameSetting type {0}", subDirName);
                    break;
                }

                try
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

                    foreach (string fileData in allFiles)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(fileData);
                        dataCopy.Data.Add((IGameSettings)SerializationUtils.DeserializeWithType(fileData, currType));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message + " " + e.StackTrace);
                }
            }
            return dataCopy;
        }
    }
}
