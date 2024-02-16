using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Editor.Utils;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System.Linq;
using System.Text;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.Versions.Settings;
using Genrpg.Editor.Services.Reflection;

namespace GameEditor
{
    public partial class DataWindow : Form
    {

        private ICloudCommsService _cloudCommsService = null;

        private EditorGameState gs = null;
        public IList<UserControl> ViewStack = null;
        private Object obj = null;
        public String action = "";
        private Form _parentForm;
        protected UIFormatter _formatter;
        public DataWindow(EditorGameState gsIn, UIFormatter formatter, Object objIn, Form parentFormIn, String actionIn)
        {
            _parentForm = parentFormIn;
            _formatter = formatter;
            gs = gsIn;
            gs.loc.Resolve(this);
            action = actionIn;
            _formatter.SetupForm(this, EFormTypes.Default);
            ViewStack = new List<UserControl>();
            obj = objIn;
            if (obj == null)
            {
                return;
            }

            Size = new Size(1600, 900);
            AddView(action);
            InitializeComponent();

        }

        public void AddView(String action)
        {
            UserControlFactory ucf = new UserControlFactory();
            UserControl view = null;
            if (action == "Users")
            {
                view = new FindUserView(gs, _formatter, this);
            }
            else if (action == "Data")
            {
                view = ucf.Create(gs, _formatter, this, obj, null, null, null);
            }
            else if (action == "Map")
            {
                view = ucf.Create(gs, _formatter, this, obj, null, null, null);
            }
            else if (action == "CopyToTest")
            {
                view = new CopyDataView(gs, this);
            }
        }

        public void GoBack()
        {
            if (ViewStack == null || ViewStack.Count < 2)
            {
                return;
            }

            UserControl control = ViewStack[ViewStack.Count - 2];
            if (control == null)
            {
                return;
            }

            ViewStack.RemoveAt(ViewStack.Count - 1);
            Controls.Clear();
            Controls.Add(control);
            DataView dv = control as DataView;
            if (dv != null)
            {
                dv.ShowData();
            }
        }

        public void GoHome()
        {
            if (ViewStack == null || ViewStack.Count < 2)
            {
                return;
            }

            UserControl control = ViewStack[0];
            if (control == null)
            {
                return;
            }

            while (ViewStack.Count > 1)
            {
                ViewStack.RemoveAt(ViewStack.Count - 1);
            }

            Controls.Clear();
            Controls.Add(control);
            DataView dv = control as DataView;
            if (dv != null)
            {
                dv.StartTick();
            }
        }

        public async Task SaveData()
        {

            String env = gs.config.Env;

            if (action == "Data")
            {

                foreach (DataView dataView in ViewStack)
                {
                    if (dataView.Obj is IGameSettings settings &&
                        !gs.LookedAtObjects.Contains(settings))
                    {
                        gs.LookedAtObjects.Add(settings);
                    }
                }

                IGameDataService gds = gs.loc.Get<IGameDataService>();

                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                bool foundBadData = false;

                List<IGrouping<Type, ITopLevelSettings>> groups = gs.data.AllSettings().GroupBy(x => x.GetType()).ToList();


                List<ITopLevelSettings> allSettings = gs.data.AllSettings();
                foreach (ITopLevelSettings settings in allSettings)
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        DialogResult result2 = MessageBox.Show("Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }
                foreach (IGrouping<Type,ITopLevelSettings> group in groups)
                {
                    List<ITopLevelSettings> items = group.ToList();

                    List<IGrouping<string, ITopLevelSettings>> nameGroups = items.GroupBy(x => x.Id).ToList();

                    if (group.Key == typeof(CurrencySettings))
                    {
                        Console.WriteLine("Looking at currencies");
                    }

                    if (items.Count != nameGroups.Count)
                    {
                        DialogResult result2 = MessageBox.Show("Setting " + group.Key.Name + " has duplicate DocId");
                        foundBadData = true;
                    }
                }

                if (foundBadData)
                {
                    return;
                }

                StringBuilder saveList = new StringBuilder();

                foreach (ITopLevelSettings settings in gs.data.AllSettings())
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        DialogResult result2 = MessageBox.Show("Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }


                List<IGameSettings> settingsToSave = new List<IGameSettings>();
                foreach (object obj in gs.LookedAtObjects)
                {
                    if (obj is IGameSettings settings)
                    {
                        settingsToSave.Add(settings);
                    }
                }

                List<IGrouping<Type, IGameSettings>> groupingList =
                    settingsToSave.GroupBy(x => x.GetType()).ToList();

                groupingList = groupingList.OrderBy(x => x.Key.Name).ToList();

                foreach (IGrouping<Type, IGameSettings> group in groupingList)
                {
                    saveList.Append(group.Key.Name + ": ");

                    List<IGameSettings> orderedList = group.OrderBy(x => x.Id).ToList();

                    for (int i = 0; i < orderedList.Count; i++)
                    {
                        saveList.Append(orderedList[i].Id + (i < orderedList.Count - 1 ? ", " : "\n"));
                    }
                }

                DialogResult result = MessageBox.Show(saveList.ToString(), "Save This Data?", MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK)
                {
                    return;
                }

                // Set Save time to before the data is saved so it's older than anything that's saved now.
                VersionSettings versionSettings = gs.data.Get<VersionSettings>(null);
                versionSettings.GameDataSaveTime = DateTime.UtcNow;
                if (!gs.LookedAtObjects.Contains(versionSettings))
                {
                    gs.LookedAtObjects.Add(versionSettings);
                }

                List<BaseGameSettings> settingsList = new List<BaseGameSettings>();
                foreach (object obj in gs.LookedAtObjects)
                {
                    if (obj is BaseGameSettings baseGameSetting)
                    {
                        settingsList.Add(baseGameSetting);
                    }
                }

                if (settingsList.Count > 0)
                {
                    await gs.repo.TransactionSave(settingsList);
                }

                gs.LookedAtObjects = new List<object>();

                _cloudCommsService.SendPubSubMessage(gs, new UpdateGameDataAdminMessage());
            }

            else if (action == "Users")
            {
                Task.Run(() => EditorPlayerUtils.SaveEditorUserData(gs).GetAwaiter().GetResult()).GetAwaiter().GetResult();
            }

        }
        public String ShowStack()
        {
            string txt = "";

            IEditorReflectionService reflectionService = gs.loc.Get<IEditorReflectionService>();

            for (int i = 0; i < ViewStack.Count; i++)
            {
                DataView dv = ViewStack[i] as DataView;
                if (dv == null)
                {
                    continue;
                }

                object obj = dv.GetObject();
                object par = dv.GetParent();
                if (obj == null)
                {
                    continue;
                }

                Type type = obj.GetType();

                object idObj = reflectionService.GetObjectValue(obj, GameDataConstants.IdKey);

                if (idObj == null)
                {
                    idObj = "";
                }

                string idStr = idObj.ToString();

                object nameObj = reflectionService.GetObjectValue(obj, "Name");

                if (!String.IsNullOrEmpty(txt))
                {
                    txt += " >>> ";
                }

                string mname = reflectionService.GetMemberName(par, obj);
                if (string.IsNullOrEmpty(mname))
                {
                    mname = type.Name;
                }

                if (mname.IndexOf("BackingField") >= 0)
                {
                    mname = "List";
                }

                txt += mname;
                if (!String.IsNullOrEmpty(idStr))
                {
                    txt += " [#" + idStr + "] ";
                    if (nameObj != null && !string.IsNullOrEmpty(nameObj.ToString()))
                    {
                        txt += nameObj.ToString() + " ";
                    }
                }

            }

            return txt;
        }

    }
}