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
using Genrpg.Shared.GameSettings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Diagnostics;
using Genrpg.Shared.Settings.Settings;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using System.Text;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Azure.Amqp.Framing;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Inventory.Settings.Slots;
using Amazon.Runtime.Documents;
using Genrpg.Shared.DataStores.Entities;
using Amazon.Runtime.Internal;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Microsoft.Extensions.Azure;
using ZstdSharp.Unsafe;

namespace GameEditor
{
    public partial class CommandForm : Form
    {
        const int _topPadding = 50;

        private EditorGameState _gs = null;
        private string _prefix;
        private MainForm _parentForm;
        private UIFormatter _formatter = null;
        private IGameData _gameData;

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
            string[] actionWords = "Data Users Maps CopyToTest CopyToGit CopyToDB MessageSetup UpdateAssets DeleteMetas SetupItemIcons ImportCrawler".Split(' ');
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


            UIHelper.CreateButton(Controls,
                EButtonTypes.Default,
                _formatter,
                "TestSharedRandom",
                "Test Shared Random",
                getButtonWidth(),
                getButtonHeight(),
                getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                getTotalHeight(buttonCount),
                OnClickSharedRandom);
            buttonCount++;


            Size = new Size(2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2), getTotalHeight(buttonCount) + getTopBottomPadding() + _topPadding);

            InitializeComponent();
        }
        
        private void OnClickSharedRandom(object sender , EventArgs e)
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

            this.Invoke((System.Windows.Forms.MethodInvoker)
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
            else if (action == "SetupItemIcons")
            {
                SetupItemIcons();
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

            Action<EditorGameState>? afterAction = null;
            if (action == "ImportCrawler")
            {
                afterAction = ImportCrawlerClassSkillData;
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

            List<IGrouping<Type,ITopLevelSettings>> groups = allGameData.GroupBy(x => x.GetType()).ToList();

            groups = groups.OrderBy(x => x.Key.Name).ToList();

            SettingsNameSettings settingSettings = (SettingsNameSettings)allGameData.FirstOrDefault(x => x.Id == "default" && x.GetType().Name == nameof(SettingsNameSettings));

            List<SettingsName> allSettingNames = settingSettings.GetData().ToList();

            long maxIndex = 0;
            
            if (allSettingNames.Count > 0)
            {
                maxIndex = allSettingNames.Max(x => x.IdKey);
            }

            foreach (IGrouping<Type,ITopLevelSettings> group in groups)
            {
                string typeName = group.Key.Name;

                SettingsName currName = allSettingNames.FirstOrDefault(x=>x.Name ==  typeName);

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

            this.Invoke((System.Windows.Forms.MethodInvoker)delegate()
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
            await EditorGameDataUtils.SaveFullGameData(this, dataCopy, toEnv, true, EditorGameState.CTS.Token);

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
            await EditorGameDataUtils.SaveFullGameData(form, dataCopy, env, true, token);

            this.Invoke(form.Hide);
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

        private void SetupItemIcons()
        {
            Form blocker = UIHelper.ShowBlockingDialog("Setting up Item Icons", _formatter, this);
            _ = Task.Run(() => SetupItemIconsAsync(this, EnvNames.Dev, blocker, EditorGameState.CTS.Token));
        }

        private async Task SetupItemIconsAsync(CommandForm form, string env, Form blocker, CancellationToken token)
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

            this.Invoke(blocker.Hide);
        }



        private void ImportCrawlerClassSkillData(EditorGameState gs)
        {


            IRepositoryService repoService = gs.loc.Get<IRepositoryService>();
            try
            {
                string missingWords = "";

                string strExeFilePath = Assembly.GetExecutingAssembly().Location;


                int lastSlashIndex = strExeFilePath.LastIndexOf("\\");

                strExeFilePath = strExeFilePath.Substring(0, lastSlashIndex);

                string fullFilePath = strExeFilePath + "\\ClassSkillImporter.csv";

                string text = File.ReadAllText(fullFilePath);

                string[] lines = text.Split('\n');

                string[] firstLine = lines[0].Split(',');


                IReadOnlyList<Class> classes = gs.data.Get<ClassSettings>(null).GetData();

                int maxClasses = 50;
                Class[] topRow = new Class[maxClasses];

                for (int s = 0; s < firstLine.Length; s++)
                {
                    topRow[s] = classes.FirstOrDefault(x => x.Name == firstLine[s]);
                    if (topRow[s] != null)
                    {
                        _gs.LookedAtObjects.Add(topRow[s]);
                        topRow[s].Bonuses = new List<ClassBonus>();
                    }
                }

                IReadOnlyList<PartyBuff> partyBuffs = gs.data.Get<PartyBuffSettings>(null).GetData();

                IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

                IReadOnlyList<StatType> statTypes = gs.data.Get<StatSettings>(null).GetData();


                PropertyInfo[] props = typeof(Class).GetProperties();

                for (int line = 1; line < lines.Length; line++)
                {
                    string[] words = lines[line].Split(',');

                    if (words.Length < 2 || string.IsNullOrEmpty(words[0]))
                    {
                        continue;
                    }

                    PartyBuff partyBuff = partyBuffs.FirstOrDefault(x => x.Name == words[0]);

                    if (partyBuff != null)
                    {
                        for (int w = 1; w < words.Length && w < maxClasses; w++)
                        {
                            if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                            {
                                topRow[w].Bonuses.Add(new ClassBonus() { EntityTypeId = EntityTypes.PartyBuff, EntityId = partyBuff.IdKey, Quantity = 1 });
                            }
                        }
                        continue;
                    }

                    StatType statType = statTypes.FirstOrDefault(x => x.Name == words[0]);

                    if (statType != null)
                    {
                        for (int w = 1; w < words.Length && w < maxClasses; w++)
                        {
                            if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                            {
                                topRow[w].Bonuses.Add(new ClassBonus() { EntityTypeId = EntityTypes.Stat, EntityId = statType.IdKey, Quantity = 1 });
                            }
                        }
                        continue;
                    }

                    CrawlerSpell crawlerSpell = crawlerSpells.FirstOrDefault(x => x.Name == words[0]);


                    if (crawlerSpell != null)
                    {
                        for (int w = 1; w < words.Length && w < maxClasses; w++)
                        {
                            if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                            {
                                topRow[w].Bonuses.Add(new ClassBonus() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = crawlerSpell.IdKey, Quantity = 1 });
                            }
                        }
                        continue;
                    }


                    PropertyInfo prop = props.FirstOrDefault(x=>x.Name == words[0]);

                    if (prop != null)
                    {
                        for (int w = 0; w < words.Length && w < maxClasses; w++)
                        {
                            if (topRow[w] != null && !String.IsNullOrEmpty(words[w]))
                            {
                                if (Int32.TryParse(words[w], out int val))
                                {
                                    EntityUtils.SetObjectValue(topRow[w], prop, val);
                                }
                            }
                        }
                        continue;
                    }


                    missingWords += words[0] + " ";
                }


                Console.WriteLine(missingWords);

                foreach (object obj in _gs.LookedAtObjects)
                {
                    if (obj is IGameSettings settings)
                    {
                        settings.SaveAll(repoService);
                    }
                }

                _gs.LookedAtObjects.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message + " " + e.StackTrace);
            }
        }
    }
}
