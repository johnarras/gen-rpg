using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Editor.Utils;
using Genrpg.Shared.GameSettings;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System.Linq;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Names.Entities;

namespace GameEditor
{
    public partial class DataWindow : Form
    {
        private EditorGameState gs = null;
        public IList<UserControl> ViewStack = null;
        private Object obj = null;
        public String action = "";
        public Form parentForm;
        public DataWindow(EditorGameState gsIn, Object objIn, Form parentFormIn, String actionIn)
        {
            parentForm = parentFormIn;
            gs = gsIn;
            action = actionIn;
            ViewStack = new List<UserControl>();
            obj = objIn;
            if (obj == null)
            {
                return;
            }

            Size = new Size(1600, 1000);
            AddView(action);
            InitializeComponent();

        }

        public void AddView(String action)
        {
            UserControlFactory ucf = new UserControlFactory();
            UserControl view = null;
            if (action == "Users")
            {
                view = new FindUserView(gs, this);
            }
            else if (action == "Data")
            {
                view = ucf.Create(gs, this, obj, null, null);
            }
            else if (action == "Map")
            {
                view = ucf.Create(gs, this, obj, null, null);
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

            DialogResult result = MessageBox.Show("Do you want to save the data?", "Saving Data", MessageBoxButtons.OKCancel);

            if (result != DialogResult.OK)
            {
                return;
            }

            String prefix = Game.Prefix;
            String env = gs.config.Env;

            if (action == "Data")
            {
                IGameDataService gds = gs.loc.Get<IGameDataService>();

                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


                bool foundBadData = false;

                List<IGrouping<Type, IGameSettings>> groups = gs.data.GetAllData().GroupBy(x => x.GetType()).ToList();

                foreach (IGameSettings settings in gs.data.GetAllData())
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        DialogResult result2 = MessageBox.Show("Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }
                foreach (IGrouping<Type,IGameSettings> group in groups)
                {
                    List<IGameSettings> items = group.ToList();

                    List<IGrouping<string, IGameSettings>> nameGroups = items.GroupBy(x => x.Id).ToList();

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

                foreach (IGameSettings settings in gs.data.GetAllData())
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        DialogResult result2 = MessageBox.Show("Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }

                foreach (object obj in gs.LookedAtObjects)
                {
                    if (obj is IGameSettings gameSetting)
                    {
                        await gameSetting.SaveAll(gs.repo);
                    }
                }
            }

            else if (action == "Users")
            {
                Task.Run(() => EditorPlayerUtils.SaveEditorUserData(gs).GetAwaiter().GetResult()).GetAwaiter().GetResult();
            }

        }
        public String ShowStack()
        {
            string txt = "";

            IReflectionService reflectionService = gs.loc.Get<IReflectionService>();

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