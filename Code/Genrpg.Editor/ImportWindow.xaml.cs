using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Editor.UI.Constants;
using Genrpg.Editor.UI;
using Genrpg.Editor.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Settings.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Spells.Settings;
using System.Text;
using System.Reflection;
using Genrpg.Shared.Entities.Utils;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportWindow : Window, IUICanvas
    {
        const int _topPadding = 50;

        private EditorGameState _gs = null;
        private string _prefix;

        private IGameData _gameData;
        private SetupDictionaryContainer<EImportTypes, IDataImporter> _importers = new();

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

        public ImportWindow()
        {
            Content = _canvas;
            _prefix = Game.Prefix;
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _prefix + "Label", _prefix, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "dev" };
            string[] actionWords = 
            {
                "ImportRoles",
                "ImportUnits",
                "ImportUnitSpawns",
                "ImportUnitKeywords",
                "ImportSpells",
                "ImportRiddles"
            };

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

            UIHelper.SetWindowRect(this, 100, 100,
                 2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2),
            getTotalHeight(buttonCount) + getTopBottomPadding() + _topPadding);

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

            Action<EditorGameState>? afterAction = null;
            if (action == "ImportRoles")
            {
                afterAction = (gs) => { ImportData(gs, EImportTypes.CrawlerRoles); };
                action = "Data";
            }
            if (action == "ImportUnits")
            {
                afterAction = (gs) => { ImportData(gs, EImportTypes.UnitTypes); };
                action = "Data";
            }
            if (action == "ImportUnitSpawns")
            {
                afterAction = (gs) => { ImportData(gs, EImportTypes.UnitSpawns); };
                action = "Data";
            }
            if (action == "ImportUnitKeywords")
            {
                afterAction = (gs) => { ImportData(gs, EImportTypes.UnitKeywords); };
                action = "Data";
            }
            if (action == "ImportSpells")
            {
                afterAction = (gs) => { ImportData(gs, EImportTypes.CrawlerSpells); };
                action = "Data";
            }
            if (action == "ImportRiddles")
            {
                afterAction = (gs) => { ImportData(gs, EImportTypes.Riddles); };
                action = "Data";
            }

            Task.Run(() => OnClickButtonAsync(action, env, afterAction));
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

            DispatcherQueue.TryEnqueue(() =>
            {
                DataWindow win = new DataWindow(_gs, _gs.EditorGameData, this, action);

                win.Activate();
            });
        }

        private void ImportData(EditorGameState gs, EImportTypes importType)
        {

            _ = Task.Run(() => ImportDataAsync(gs, importType));
        }

        private async Task ImportDataAsync(EditorGameState gs, EImportTypes importType)
        {
            gs.loc.Resolve(_importers);

            if (_importers.TryGetValue(importType, out IDataImporter importer))
            {
                await importer.ImportData(this, gs);
            }
        }
    }
}