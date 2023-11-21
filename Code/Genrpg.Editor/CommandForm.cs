using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.Threading.Tasks;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Constants;
using Genrpg.ServerShared.Setup;
using Genrpg.Editor.Services.Setup;
using Genrpg.ServerShared;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Shared.MapMessages;
using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Utils;
using Amazon.Runtime.Internal;
using System.Reflection;
using Genrpg.Shared.Purchasing.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Chat.Entities;
using Genrpg.ServerShared.DataStores.NoSQL;
using Genrpg.ServerShared.CloudComms.Constants;

namespace GameEditor
{
    public partial class CommandForm : Form
    {
        private EditorGameState gs = null;
        private string prefix;
        private MainForm parentForm;
        public CommandForm (string prefixIn, MainForm parentFormIn) 
        {
            parentForm = parentFormIn;
            prefix = prefixIn;
            int numButtons = 0;

            Label lab = new Label();
            lab.Location = new Point(getLeftRightPadding(), getTopBottomPadding());
            lab.Size = new Size(getButtonWidth(), getButtonHeight());
            lab.Text = prefix;
            lab.Font = new Font("Arial", 20);
            lab.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lab);
            numButtons++;

            string[] envWords = Enum.GetNames(typeof(EnvEnum)); 
            string[] actionWords = "Data Users Maps CopyToTest CopyToGit CopyToDB MessageSetup UpdateAssets DeleteMetas".Split(' ');
            Button button = null;
            int p = 0;
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
                            if (prefix == Game.Prefix)
                            {

                                button = new Button();
                                button.Text = prefix + " " + env + " " + action;
                                button.Name = env + action;
                                button.Size = new Size(getButtonWidth(), getButtonHeight());
                                button.Location = new Point(getLeftRightPadding() + p * (getButtonWidth() + p * getButtonGap()), getTotalHeight(numButtons));
                                numButtons++;
                                Controls.Add(button);
                                button.Click += OnClickMaps;
                            }
                            continue;
                        }


                        button = new Button();
                        button.Text = env + " " + action;
                        button.Name = env + action;
                        button.Size = new Size(getButtonWidth(), getButtonHeight());
                        button.Location = new Point(getLeftRightPadding() + p * (getButtonWidth() + p * getButtonGap()), getTotalHeight(numButtons));
                        numButtons++;
                        button.Click += OnClickButton;
                        Controls.Add(button);
                    }
                }
            }



            Size = new Size(2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2), getTotalHeight(numButtons)*2 + getTopBottomPadding());

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

            if (string.IsNullOrEmpty(prefix))
            {
                return;
            }

            String env = words[1];
			String action = words[2];

            gs = await EditorGameDataUtils.SetupFromConfig(this, env);

            this.Invoke((MethodInvoker)
                delegate ()
                {
                    ViewMaps va = new ViewMaps(gs);
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

            if (string.IsNullOrEmpty(prefix))
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

            gs = await EditorGameDataUtils.SetupFromConfig(this, env);


            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


            DateTime buildtime = new DateTime(2000, 1, 1)
                    .AddDays(version.Build).AddSeconds(version.Revision * 2);

            IReflectionService reflectionService = gs.loc.Get<IReflectionService>();
            reflectionService.InitializeObjectData(gs.data);

            gs.EditorGameData = new EditorGameData()
            {
                GameData = gs.data,
            };

            

            List<IGameSettings> allGameData = gs.data.GetAllData();

            List<IGrouping<Type,IGameSettings>> groups = allGameData.GroupBy(x => x.GetType()).ToList();

            groups = groups.OrderBy(x => x.Key.Name).ToList();

            foreach (IGrouping<Type,IGameSettings> group in groups)
            {

                List<IGameSettings> orderedList = group.OrderBy(x => x.Id).ToList();

                List<BaseGameSettings> items = new List<BaseGameSettings>();

                for (int i = 0; i <  orderedList.Count; i++) 
                {
                    BaseGameSettings setting = orderedList[i] as BaseGameSettings;
                    if (setting != null)
                    {
                        items.Add(setting);
                    }
                }


                Type baseCollectionType = typeof(TypedEditorSettingsList<>);
                Type genericType = baseCollectionType.MakeGenericType(group.Key);
                EditorSettingsList list = (EditorSettingsList)Activator.CreateInstance(genericType);
                list.SetData(items);
                list.TypeName = "[" + group.Count() + "] " + group.Key.Name;
                gs.EditorGameData.Data.Add(list);
            }

            this.Invoke((MethodInvoker)delegate()
               {
                   DataWindow win = new DataWindow(gs, gs.EditorGameData, this, action);
                   win.Show();
               });

        }

        private void CopyDataFromEnvToEnv(string fromEnv, string toEnv)
        {
            Form form = UIHelper.ShowBlockingDialog("Copying data from " + fromEnv + " to " + toEnv, this);
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
                if (f == this || f == parentForm)
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
            Form form = UIHelper.ShowBlockingDialog("Copying to Git", this);
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
            Form form = UIHelper.ShowBlockingDialog("Updating Assets");
            _ = Task.Run(() => UpdateAssetsUtils.UpdateAssetsAsync(env));
        }

        protected void CopyGameDataFromGitToDatabase(string env)
        {
           Form form = UIHelper.ShowBlockingDialog("Copying to Mongo", this);
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
