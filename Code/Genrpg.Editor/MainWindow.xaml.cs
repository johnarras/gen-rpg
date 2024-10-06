using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Editor.UI;
using Genrpg.Editor.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Settings.Settings;
using Genrpg.Shared.Stats.Settings.Scaling;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Editor.Constants;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, IUICanvas
    {
        const int _topPadding = 50;

        private EditorGameState _gs = null;
        private string _prefix;

        private IGameData _gameData;

        private Canvas _canvas = new Canvas();

        public void Add(UIElement elem, double x, double y)
        {
            _canvas.Children.Add(elem);
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        public void Remove(UIElement elem)
        {
            _canvas?.Children.Remove(elem);
        }

        public bool Contains(UIElement elem)
        {
            return _canvas.Children.Contains(elem);
        }

        public MainWindow()
        {
            Content = _canvas;
            _prefix = Game.Prefix;
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _prefix + "Label", _prefix, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "dev" };
            string[] actionWords = "Data Users Maps Importer CopyToTest CopyToGit CopyToDB MessageSetup UpdateAssets DeleteMetas SetupItemIcons TestAccountSetup".Split(' ');
            int column = 0;
            for (int e = 0; e < envWords.Length; e++)
            {
                string env = envWords[e];

                if (env != EnvNames.Dev)
                {
                    continue;
                }
                if (env != EnvNames.Test)
                {
                    for (int a = 0; a < actionWords.Length; a++)
                    {
                        string action = actionWords[a];

                        if (env == EnvNames.Prod && action == "Data")
                        {
                            continue;
                        }

                        if (action == "Maps")
                        {
                            if (_prefix == Game.Prefix)
                            {

                                UIHelper.CreateButton(this,
                                    EButtonTypes.Default,
                                    env + " " + action,
                                    _prefix + " " + env + " " + action,
                                    getButtonWidth(),
                                    getButtonHeight(),
                                    getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                                    getTotalHeight(buttonCount),
                                    OnClickMaps);
                                buttonCount++;
                            }
                            continue;
                        }



                        UIHelper.CreateButton(this,
                            EButtonTypes.Default,
                            env + " " + action,
                            env + " " + action,
                            getButtonWidth(),
                            getButtonHeight(),
                            getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                            getTotalHeight(buttonCount),
                            OnClickButton);
                        buttonCount++;
                    }
                }

            }


            UIHelper.CreateButton(this,
                EButtonTypes.Default,
                "TestSharedRandom",
                "Test Shared Random",
                getButtonWidth(),
                getButtonHeight(),
                getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                getTotalHeight(buttonCount),
                OnClickSharedRandom);
            buttonCount++;

            UIHelper.SetWindowRect(this, 100, 100,
                 2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2),
            getTotalHeight(buttonCount) + getTopBottomPadding() + _topPadding);

        }

        private void OnClickSharedRandom(object sender, RoutedEventArgs e)
        {
            TestSharedRandomAsync().Wait();
        }

        private async Task TestSharedRandomAsync()
        {
            int numTasks = 100;
            int iterationCount = 100000;

            DateTime startTime = DateTime.UtcNow;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(TestRegRandom(iterationCount));
            }

            await Task.WhenAll(tasks);

            double randSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

            startTime = DateTime.UtcNow;

            tasks.Clear();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(TestSharedRandom(iterationCount));
            }

            await Task.WhenAll(tasks);

            double sharedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

            Trace.WriteLine("RandSeconds: " + randSeconds + " " + totalRandVal + " SharedSeconds: " + sharedSeconds + " " + totalSharedVal);

        }

        private long totalRandVal = 0;
        private long totalSharedVal = 0;
        private async Task TestRegRandom(int iterations)
        {
            Random rand = new Random();
            long val = 0;
            for (int i = 0; i < iterations; i++)
            {
                val += rand.Next() + rand.Next() + rand.Next() + rand.Next() + rand.Next() +
                   rand.Next() + rand.Next() + rand.Next() + rand.Next() + rand.Next();
            }

            totalRandVal += val;
            await Task.CompletedTask;
        }

        private async Task TestSharedRandom(int iterations)
        {
            long val = 0;
            for (int i = 0; i < iterations; i++)
            {
                val += Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() +
                    Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next();
            }

            totalSharedVal += val;
            await Task.CompletedTask;
        }

        private int getButtonWidth() { return 150; }

        private int getButtonHeight() { return 40; }

        private int getLeftRightPadding() { return 20; }

        private int getTopBottomPadding() { return 10; }

        private int getButtonGap() { return 8; }

        private int getTotalHeight(int numButtons)
        {
            return (getButtonHeight() + getButtonGap()) * numButtons + getTopBottomPadding();
        }

        private void OnClickMaps(object sender, RoutedEventArgs e)
        {

            _ = Task.Run(() => OnClickMapsAsync(sender, e));
        }

        private async Task OnClickMapsAsync(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            if (but == null)
            {
                return;
            }

            String txt = but.Content.ToString();
            if (String.IsNullOrEmpty(txt))
            {
                return;
            }

            string[] words = txt.Split(' ');
            if (words.Length < 3)
            {
                return;
            }

            if (string.IsNullOrEmpty(_prefix))
            {
                return;
            }

            String env = words[1];
            String action = words[2];

            _gs = await EditorGameDataUtils.SetupFromConfig(this, env);

        }

        private void OnClickButton(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            if (but == null)
            {
                return;
            }

            String txt = but.Name;
            if (String.IsNullOrEmpty(txt))
            {
                return;
            }
            string[] words = txt.Split(' ');
            if (words.Length < 2)
            {
                return;
            }

            if (string.IsNullOrEmpty(_prefix))
            {
                return;
            }

            String env = words[0];
            String action = words[1];

            if (action == "CopyToTest")
            {
                CopyDataFromEnvToEnv(EnvNames.Dev, EnvNames.Test);
                return;
            }

            if (action == "CopyToGit")
            {
                CopyGameDataFromDatabaseToGit(EnvNames.Dev);
                return;
            }
            if (action == "CopyToDB")
            {
                CopyGameDataFromGitToDatabase(EnvNames.Dev);
                return;
            }
            else if (action == "SetupItemIcons")
            {
                SetupItemIcons();
                return;
            }

            if (action == "MessageSetup")
            {
                EditorGameDataUtils.InitMessages();
                return;
            }

            if (action == "UpdateAssets")
            {
                UpdateAssets(env);
            }

            if (action == "DeleteMetas")
            {
                DeleteAllMetaFiles();
            }
            
            if (action == "Importer")
            {
                ImportWindow importer = new ImportWindow();
                importer.Activate();
                return;

            }

            Task.Run(() => OnClickButtonAsync(action, env, null));
        }


        private async Task OnClickButtonAsync(string action, string env, Action<EditorGameState>? afterAction = null)
        {

            _gs = await EditorGameDataUtils.SetupFromConfig(this, env);


            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


            DateTime buildtime = new DateTime(2000, 1, 1)
                    .AddDays(version.Build).AddSeconds(version.Revision * 2);

            IEditorReflectionService reflectionService = _gs.loc.Get<IEditorReflectionService>();
            reflectionService.InitializeObjectData(_gameData);

            _gs.EditorGameData = new EditorGameData()
            {
                GameData = _gameData,
            };

            List<ITopLevelSettings> allGameData = _gameData.AllSettings();

            List<IGrouping<Type, ITopLevelSettings>> groups = allGameData.GroupBy(x => x.GetType()).ToList();

            groups = groups.OrderBy(x => x.Key.Name).ToList();

            SettingsNameSettings settingSettings = (SettingsNameSettings)allGameData.FirstOrDefault(x => x.Id == "default" && x.GetType().Name == nameof(SettingsNameSettings));

            List<SettingsName> allSettingNames = settingSettings.GetData().ToList();

            long maxIndex = 0;

            if (allSettingNames.Count > 0)
            {
                maxIndex = allSettingNames.Max(x => x.IdKey);
            }

            foreach (IGrouping<Type, ITopLevelSettings> group in groups)
            {
                string typeName = group.Key.Name;

                SettingsName currName = allSettingNames.FirstOrDefault(x => x.Name == typeName);

                if (currName == null)
                {
                    currName = new SettingsName() { Id = "default", Name = typeName, IdKey = ++maxIndex };
                    allSettingNames.Add(currName);
                    _gs.LookedAtObjects.Add(currName);
                }


                List<ITopLevelSettings> orderedList = group.OrderBy(x => x.Id).ToList();

                List<BaseGameSettings> items = new List<BaseGameSettings>();

                for (int i = 0; i < orderedList.Count; i++)
                {
                    BaseGameSettings setting = orderedList[i] as BaseGameSettings;
                    if (setting != null)
                    {
                        items.Add(setting);
                        if (setting.UpdateTime == DateTime.MinValue)
                        {
                            _gs.LookedAtObjects.Add(setting);
                        }
                        foreach (IGameSettings childSetting in setting.GetChildren())
                        {
                            if (childSetting is IUpdateData updateChild)
                            {
                                if (updateChild.UpdateTime == DateTime.MinValue)
                                {
                                    _gs.LookedAtObjects.Add(updateChild);
                                }
                            }
                        }
                    }
                }


                Type baseCollectionType = typeof(TypedEditorSettingsList<>);
                Type genericType = baseCollectionType.MakeGenericType(group.Key);
                EditorSettingsList list = (EditorSettingsList)Activator.CreateInstance(genericType);
                list.SetData(items);
                list.TypeName = "[" + group.Count() + "] " + group.Key.Name;
                _gs.EditorGameData.Data.Add(list);
            }

            settingSettings.SetData(allSettingNames);

            if (afterAction != null)
            {
                afterAction.Invoke(_gs);
            }

            DispatcherQueue.TryEnqueue(()=> 
            {
                DataWindow win = new DataWindow(_gs, _gs.EditorGameData, this, action);

                win.Activate();
            });

        }

        private void CopyDataFromEnvToEnv(string fromEnv, string toEnv)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying data from " + fromEnv + " to " + toEnv);
            _ = Task.Run(() => CopyDataFromEnvToEnvAsync(fromEnv, toEnv, form));
        }

        private async Task CopyDataFromEnvToEnvAsync(string fromEnv, string toEnv, SmallPopup form)
        {

            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, fromEnv, EditorGameState.CTS.Token);
            await EditorGameDataUtils.SaveFullGameData(this, dataCopy, toEnv, true, EditorGameState.CTS.Token);

            form.StartClose();
        }

        private void CopyGameDataFromDatabaseToGit(string env)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying to Git");
            _ = Task.Run(() => CopyGameDataFromDatabaseToGitAsync(env, form, EditorGameState.CTS.Token));
        }

        private async Task CopyGameDataFromDatabaseToGitAsync(string env, SmallPopup form, CancellationToken token)
        {
            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, env, token);

            EditorGameDataUtils.WriteGameDataToDisk(dataCopy);

            form.StartClose();
        }

        private void UpdateAssets(string env)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Updating Assets");
            _ = Task.Run(() => UpdateAssetUtils.UpdateAssetsAsync(env));
        }

        private void CopyGameDataFromGitToDatabase(string env)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying to Mongo");
            _ = Task.Run(() => CopyGameDataFromGitToDatabaseAsync(form, env, EditorGameState.CTS.Token));
        }

        private async Task CopyGameDataFromGitToDatabaseAsync(SmallPopup form, string env, CancellationToken token)
        {
            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadDataFromDisk(form, token);
            await EditorGameDataUtils.SaveFullGameData(form, dataCopy, env, true, token);

            form.StartClose();
        }

        private void DeleteAllMetaFiles()
        {

            string strExeFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            strExeFilePath += "\\..\\..\\..\\..\\Genrpg.Client\\";
            RemoveMetaFiles(strExeFilePath);
        }

        private void RemoveMetaFiles(string strExeFilePath)
        {
            if (!Directory.Exists(strExeFilePath))
            {
                return;
            }

            string[] currDirs = Directory.GetDirectories(strExeFilePath);
            string[] files = Directory.GetFiles(strExeFilePath);

            foreach (string file in files)
            {
                if (file.LastIndexOf(".meta") == file.Length - 5)
                {
                    File.Delete(Path.Combine(strExeFilePath, file));
                }
            }

            foreach (string dir in currDirs)
            {
                RemoveMetaFiles(Path.Combine(strExeFilePath, dir));
            }

        }

        private void SetupItemIcons()
        {
            SmallPopup blocker = UIHelper.ShowBlockingDialog(this, "Setting up Item Icons");
            _ = Task.Run(() => SetupItemIconsAsync(this, EnvNames.Dev, blocker, EditorGameState.CTS.Token));
        }

        private async Task SetupItemIconsAsync(MainWindow form, string env, SmallPopup blocker, CancellationToken token)
        {
            string strExeFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            strExeFilePath += "\\..\\..\\..\\..\\GenrpgClient\\Assets\\FullAssets\\Atlas\\Icons";

            List<string> startFilenames = Directory.GetFiles(strExeFilePath).ToList();

            startFilenames = startFilenames.Where(x => x.IndexOf(".meta") < 0).ToList();


            List<string> fileNames = new List<string>();

            foreach (string startFileName in startFilenames)
            {
                int lastSlash = startFileName.LastIndexOf('\\');

                string finalFileName = startFileName.Substring(lastSlash + 1).Replace(".png", "").Replace(".PNG", "");

                fileNames.Add(finalFileName);
            }

            _gs = await EditorGameDataUtils.SetupFromConfig(this, env);

            IGameData gameData = _gs.data;


            IReadOnlyList<ItemType> itemTypes = gameData.Get<ItemTypeSettings>(null).GetData();

            IReadOnlyList<ScalingType> scalingTypes = gameData.Get<ScalingTypeSettings>(null).GetData();

            _gs.LookedAtObjects.Add(gameData.Get<ItemTypeSettings>(null));

            List<string> prefixNames = new List<string>() { "" };

            foreach (ScalingType scalingType in scalingTypes)
            {
                if (scalingType.IdKey < 1 || string.IsNullOrEmpty(scalingType.Icon))
                {
                    continue;
                }

                prefixNames.Add(scalingType.Icon);
            }


            foreach (ItemType itype in itemTypes)
            {
                if (itype.EquipSlotId < 1 || string.IsNullOrEmpty(itype.Icon))
                {
                    continue;
                }

                itype.IconCounts = new List<NameCount>();

                _gs.LookedAtObjects.Add(itype);

                foreach (string prefixName in prefixNames)
                {

                    int maxIndex = 0;

                    for (int i = 1; i < 1000; i++)
                    {
                        string fullName = itype.Icon + prefixName + "_" + i.ToString("D3");

                        if (fileNames.Contains(fullName))
                        {
                            maxIndex = i;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (maxIndex > 0)
                    {
                        itype.IconCounts.Add(new NameCount() { Count = maxIndex, Name = prefixName });
                    }
                }
            }
            IRepositoryService repoService = _gs.loc.Get<IRepositoryService>();

            List<IGameSettings> settingsList = _gs.LookedAtObjects.Cast<IGameSettings>().ToList();
            try
            {
                foreach (IGameSettings settings in settingsList)
                {
                    await repoService.Save(settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            blocker.StartClose();
        }
    }
}

