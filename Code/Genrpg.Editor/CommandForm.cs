using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using System.Threading.Tasks;
using Genrpg.Shared.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.MapMessages;
using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Utils;
using System.Reflection;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.Inventory.Settings.LootRanks;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Utils;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameEditor
{
    public partial class CommandForm : Form
    {
        const int _topPadding = 50;

        private EditorGameState _gs = null;
        private string _prefix;
        private MainForm _parentForm;
        private UIFormatter _formatter = null;

        public CommandForm (string prefix, UIFormatter formatter, MainForm parentForm) 
        {
            _parentForm = parentForm;
            _prefix = prefix;
            _formatter = formatter;
            _formatter.SetupForm(this, EFormTypes.Default);
            int buttonCount = 0;


            UIHelper.CreateLabel(Controls, ELabelTypes.Default, _formatter, _prefix + "Label", _prefix, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);


            buttonCount++;

            string[] envWords = { "dev" };
            string[] actionWords = "Data Users Maps CopyToTest CopyToGit CopyToDB MessageSetup UpdateAssets DeleteMetas".Split(' ');
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

                                UIHelper.CreateButton(Controls, 
                                    EButtonTypes.Default, 
                                    _formatter, 
                                    env + action, 
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



                        UIHelper.CreateButton(Controls,
                            EButtonTypes.Default,
                            _formatter,
                            env + action,
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



            Size = new Size(2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2), getTotalHeight(buttonCount) + getTopBottomPadding() + _topPadding);

            InitializeComponent();
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

        private void OnClickMaps(object sender, EventArgs e)
        {

            _ = Task.Run(() => OnClickMapsAsync(sender, e));
        }

        private async Task OnClickMapsAsync(object sender, EventArgs e)
        { 
			Button but = sender as Button;
			if (but == null)
            {
                return;
            }

            String txt = but.Text;
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

            this.Invoke((MethodInvoker)
                delegate ()
                {
                    ViewMaps va = new ViewMaps(_gs, _formatter);
                    va.Show();
                });

        }

        private void OnClickButton(object sender, EventArgs e)
        {
            Button but = sender as Button;
            if (but == null)
            {
                return;
            }

            String txt = but.Text;
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

            if (action == "MessageSetup")
            {
                MapMessageInit.InitMapMessages();
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

            Task.Run(() => OnClickButtonAsync(action, env));
        }

        private async Task OnClickButtonAsync(string action, string env)
        { 

            _gs = await EditorGameDataUtils.SetupFromConfig(this, env);


            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


            DateTime buildtime = new DateTime(2000, 1, 1)
                    .AddDays(version.Build).AddSeconds(version.Revision * 2);

            IEditorReflectionService reflectionService = _gs.loc.Get<IEditorReflectionService>();
            reflectionService.InitializeObjectData(_gs.data);

            _gs.EditorGameData = new EditorGameData()
            {
                GameData = _gs.data,
            };

            List<ITopLevelSettings> allGameData = _gs.data.AllSettings();

            List<IGrouping<Type,ITopLevelSettings>> groups = allGameData.GroupBy(x => x.GetType()).ToList();

            groups = groups.OrderBy(x => x.Key.Name).ToList();

            foreach (IGrouping<Type,ITopLevelSettings> group in groups)
            {

                List<ITopLevelSettings> orderedList = group.OrderBy(x => x.Id).ToList();

                List<BaseGameSettings> items = new List<BaseGameSettings>();

                for (int i = 0; i <  orderedList.Count; i++) 
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

            this.Invoke((MethodInvoker)delegate()
               {
                   DataWindow win = new DataWindow(_gs, _formatter, _gs.EditorGameData, this, action);
                   win.Show();
               });

        }

        private void CopyDataFromEnvToEnv(string fromEnv, string toEnv)
        {
            Form form = UIHelper.ShowBlockingDialog("Copying data from " + fromEnv + " to " + toEnv, _formatter, this);
            _ = Task.Run(() => CopyDataFromEnvToEnvAsync(fromEnv, toEnv, form));
        }

        private async Task CopyDataFromEnvToEnvAsync(string fromEnv, string toEnv, Form form)
        {

            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, fromEnv, EditorGameState.CTS.Token);
            await EditorGameDataUtils.SaveFullGameData(this, dataCopy, toEnv, EditorGameState.CTS.Token);

            this.Invoke(form.Hide);
        }

        public void CloseAllDataForms ()
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                Form f = Application.OpenForms[i];
                if (f == this || f == _parentForm)
                {
                    continue;
                }

                DataWindow win = f as DataWindow;
                if (win != null)
                {

                    _ = Task.Run(() => win.SaveData());
                }
                f.Close();
                i--;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        protected void CopyGameDataFromDatabaseToGit(string env)
        {
            Form form = UIHelper.ShowBlockingDialog("Copying to Git", _formatter, this);
            _ = Task.Run(() => CopyGameDataFromDatabaseToGitAsync(env, form, EditorGameState.CTS.Token));
        }

        protected async Task CopyGameDataFromDatabaseToGitAsync(string env, Form form, CancellationToken token)
        {
            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, env, token);

            EditorGameDataUtils.WriteGameDataToDisk(dataCopy);

            this.Invoke(form.Hide);
        }

        protected void UpdateAssets(string env)
        {
            Form form = UIHelper.ShowBlockingDialog("Updating Assets", _formatter);
            _ = Task.Run(() => UpdateAssetsUtils.UpdateAssetsAsync(env));
        }

        protected void CopyGameDataFromGitToDatabase(string env)
        {
           Form form = UIHelper.ShowBlockingDialog("Copying to Mongo", _formatter, this);
            _ = Task.Run(() => CopyGameDataFromGitToDatabaseAsync(form, env, EditorGameState.CTS.Token));
        }

        private async Task CopyGameDataFromGitToDatabaseAsync(Form form, string env, CancellationToken token)
        {
            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadDataFromDisk(form, token);
            await EditorGameDataUtils.SaveFullGameData(form, dataCopy, env, token);

            this.Invoke(form.Hide);
        }

        private void DeleteAllMetaFiles()
        {

            string strExeFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            strExeFilePath += "\\..\\..\\..\\..\\Genrpg.Client\\Genrpg\\Genrpg.Game\\";
            RemoveMetaFiles(strExeFilePath);
        }

        private void RemoveMetaFiles(string strExeFilePath)
        {
            if (!Directory.Exists(strExeFilePath))
            {
                return;
            }

            string [] currDirs = Directory.GetDirectories(strExeFilePath);
            string [] files = Directory.GetFiles(strExeFilePath);

            foreach (string file in files)
            {
                if (file.LastIndexOf(".meta") == file.Length-5)
                {
                    File.Delete(Path.Combine(strExeFilePath, file));
                }
            }

            foreach (string dir in currDirs)
            {
                RemoveMetaFiles(Path.Combine(strExeFilePath, dir));
            }

        }
    }
}
