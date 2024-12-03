using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Editor.UI;
using Genrpg.Editor.Utils;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Versions.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataWindow : Window, IUICanvas
    {

        private ICloudCommsService _cloudCommsService = null;
        private IRepositoryService _repoService = null;
        private IServerConfig _config = null;

        private EditorGameState gs = null;
        public IList<UserControl> ViewStack = null;
        private Object obj = null;
        public String action = "";
        private Window _parentForm;


        int width = 2500;
        int height = 1200;

        public int Width => width;
        public int Height => height;

        private Canvas _canvas = new Canvas();
        public void Add(UIElement elem, double x, double y)
        {
            _canvas.Children.Add(elem);
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        public void Remove(UIElement cont)
        {
            _canvas.Children.Remove(cont);
        }

        public bool Contains(UIElement cont)
        {
            return _canvas.Children.Contains(cont);
        }

        public DataWindow(EditorGameState gsIn, Object objIn, Window parentFormIn, String actionIn)
        {
            Content = _canvas;
            _parentForm = parentFormIn;
            gs = gsIn;
            gs.loc.Resolve(this);
            action = actionIn;
            ViewStack = new List<UserControl>();
            obj = objIn;
            if (obj == null)
            {
                return;
            }

            UIHelper.SetWindowRect(this, 50, 50, width, height);
                
            AddView(action);

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
                view = ucf.Create(gs, this, obj, null, null, null);
            }
            else if (action == "Map")
            {
                view = ucf.Create(gs, this, obj, null, null, null);
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

            _canvas.Children.Clear();
            _canvas.Children.Add(control);
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

            _canvas.Children.Clear();
            _canvas.Children.Add(control);
            DataView dv = control as DataView;
            if (dv != null)
            {
                dv.StartTick();
            }
        }

        public void AddChildView(UserControl dv)
        {
            _canvas.Children.Clear();
            _canvas.Children.Add(dv);
            ViewStack.Add(dv);
        }

        public void AddControl(UIElement cont, int top = 0, int left = 0)
        {
            _canvas.Children.Add(cont);
        }

        public async Task SaveData()
        {

            String env = _config.Env;

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
                        ContentDialogResult result2 = await UIHelper.ShowMessageBox(this, "Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }
                foreach (IGrouping<Type, ITopLevelSettings> group in groups)
                {
                    List<ITopLevelSettings> items = group.ToList();

                    List<IGrouping<string, ITopLevelSettings>> nameGroups = items.GroupBy(x => x.Id).ToList();

                    if (items.Count != nameGroups.Count)
                    {
                        ContentDialogResult result2 = await UIHelper.ShowMessageBox(this, "Setting " + group.Key.Name + " has duplicate DocId");
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
                        ContentDialogResult result2 = await UIHelper.ShowMessageBox(this, "Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }


                List<IGameSettings> settingsToSave = new List<IGameSettings>();
                foreach (object obj in gs.LookedAtObjects) // Grouping, not saving
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

                ContentDialogResult result = await UIHelper.ShowMessageBox(this, saveList.ToString(), "Save This Data?", true);

                if (result != ContentDialogResult.Primary)
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
                foreach (object obj in gs.LookedAtObjects) // Saving
                {
                    if (obj is BaseGameSettings baseGameSetting)
                    {
                        settingsList.Add(baseGameSetting);
                        baseGameSetting.UpdateTime = versionSettings.GameDataSaveTime;
                    }
                }

                if (settingsList.Count > 0)
                {
                    await _repoService.TransactionSave(settingsList);
                }

                gs.LookedAtObjects = new List<object>();

                _cloudCommsService.SendPubSubMessage(new UpdateGameDataAdminMessage());
            }

            else if (action == "Users")
            {
                Task.Run(() => EditorPlayerUtils.SaveEditorUserData(gs, _repoService).GetAwaiter().GetResult()).GetAwaiter().GetResult();
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
