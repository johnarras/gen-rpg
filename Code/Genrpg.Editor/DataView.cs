using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Editor.Entities;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.ServerShared.PlayerData;
using System.Threading.Tasks;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Editor.Interfaces;

namespace GameEditor
{
    public partial class DataView : UserControl
    {
        public EditorGameState gs = null;
        public Object obj = null;
        public Object parent = null;
        public Object grandparent = null;
        public Type objType = null;
        public Type parentType = null;
        public Type grandparentType = null;
        public DataWindow win = null;
        public Button BackButton = null;
        public Button HomeButton = null;
        public Button AddButton = null;
        public Button DeleteButton = null;
        public Button CopyButton = null;
        public Button SaveButton = null;
        public Button DetailsButton = null;
        public Panel SingleGrid = null;
        public DataGridView MultiGrid = null;


        public const int MaxButtonsPerRow = 8;
        public const int SingleItemWidth = 120;
        public const int SingleItemHeight = 40;
        public const int SingleItemWidthPad = 5;
        public const int SingleItemHeightPad = 20;
        public const int SingleItemTopPad = 60;
        public const int SingleItemLeftPad = 10;
        public int numSingleTopButtonsShown = 0;

		public IList<Panel> colorPanels = new List<Panel>();

		public IDictionary<string,Graphics> panelGraphics = new Dictionary<string,Graphics>();

		Timer timer = null;


		protected IList<String> IgnoredFields;

        protected IReflectionService _reflectionService;
        protected IGameDataService _gameDataService;

        protected bool IsIgnoreField (String nm)
		{
			if (!string.IsNullOrEmpty(nm) && 
				IgnoredFields != null && 
				IgnoredFields.Contains(nm))
			{
				return true;
			}
			return false;
		}

        public DataView(EditorGameState gsIn, DataWindow winIn, Object objIn, Object parentIn, Object grandparentIn)
        {
            
            gs = gsIn;
            gs.loc.Resolve(this);
            obj = objIn;
            parent = parentIn;
            grandparent = grandparentIn;
            win = winIn;
            if (win != null)
            {
                Size = win.Size;
                win.Controls.Clear();
                win.Controls.Add(this);
                win.ViewStack.Add(this);
            }

            SetupGrids();

            InitializeComponent();

            if (!gsIn.LookedAtObjects.Contains(obj))
            {
                gsIn.LookedAtObjects.Add(obj);
            }

			if (_gameDataService != null)
			{
				IgnoredFields = _gameDataService.GetEditorIgnoreFields();
			}

			if (IgnoredFields ==null)
			{
				IgnoredFields = new List<String>();
			}

            objType = obj.GetType();
            if (parent != null)
            {
                parentType = parent.GetType();
            }

            if (grandparent != null)
            {
                grandparentType = grandparent.GetType();
            }

            AddTopButtons();

            ShowData();

            InitializeComponent();

        }

		public void StartTick()
		{

			timer = new Timer();
			timer.Tick += OnTick;
			timer.Interval = 75;
			timer.Start();

        }

        public Object GetObject()
        {
            return obj;
        }

        public Object GetParent()
        {
            return parent;
        }

        public void Save()
        {
            if (win != null)
            {
                _ = Task.Run(() => win.SaveData());
            }
        }

        private void SetupGrids()
        {
            if (win == null)
            {
                return;
            }

            CreateMultiGrid();

            CreateSingleGrid();
        }

        private void CreateMultiGrid()
        {
            if (MultiGrid != null && Controls.Contains(MultiGrid))
            {
                Controls.Remove(MultiGrid);
            }
            int sx = win.Width - 16;
            int sy = win.Height - SingleItemTopPad - 37;
            MultiGrid = new DataGridView();
            //MultiGrid.MultiSelect = false;
            MultiGrid.Location = new Point(0, SingleItemTopPad);

            MultiGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiGrid.Size = new Size(sx, sy);
            MultiGrid.SelectionChanged += MultiGrid_SelectionChanged;
            MultiGrid.CellValueChanged += MultiGrid_CellValueChanged;
            Controls.Add(MultiGrid);
        }

        private void MultiGrid_SelectionChanged(object sender, EventArgs e)
        {
            MarkSelectedRows();
        }

        private void MarkSelectedRows()
        {
            DataGridViewSelectedRowCollection rows = MultiGrid.SelectedRows;

            if (rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            object item = row.DataBoundItem;

            if (!gs.LookedAtObjects.Contains(item))
            {
                gs.LookedAtObjects.Add(item);
            }
        }



        private void MultiGrid_CellValueChanged(object sender, EventArgs e)
        {
            MarkSelectedRows();
        }

        private void CreateSingleGrid()
        {
            int sx = win.Width - 16;
            int sy = win.Height - SingleItemTopPad - 37;

            SingleGrid = new Panel();
            SingleGrid.AutoScroll = true;
            SingleGrid.Location = new Point(0, SingleItemTopPad);
            SingleGrid.Size = new Size(sx, sy);
            Controls.Add(SingleGrid);
        }


        private bool addedButtons = false;
        private void AddTopButtons()
        {
            if (addedButtons)
            {
                return;
            }

            addedButtons = true;

            Button but = null;

            if (win != null)
            {
                Label label = new Label();
                Controls.Add(label);
                label.Location = new Point(10, 10);
                label.Size = new Size(win.Width, 20);
                label.Text = win.ShowStack();
            }

            int y = 30;
            int x = 20;
            Size sz = new Size(100, 30);

            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "HomeButton";
            but.Text = "Home";
            but.Click += OnClickHome;
            Controls.Add(but);
            HomeButton = but;

            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "BackButton";
            but.Text = "Back";
            but.Click += OnClickBack;
            Controls.Add(but);
            BackButton = but;

            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "SaveButton";
            but.Text = "Save";
            but.Click += OnClickSave;
            Controls.Add(but);
            SaveButton = but;

            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "AddButton";
            but.Text = "Add";
            but.Click += OnClickAdd;
            Controls.Add(but);
            AddButton = but;

            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "CopyButton";
            but.Text = "Copy";
            but.Click += OnClickCopy;
            Controls.Add(but);
            CopyButton = but;

            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "DetailsButton";
            but.Text = "Details";
            but.Click += OnClickDetails;
            Controls.Add(but);
            DetailsButton = but;


            x += sz.Width + 5;
            but = new Button();
            but.Location = new Point(x, y);
            but.Size = sz;
            x += sz.Width + 5;
            but.Name = "DeleteButton";
            but.Text = "Delete";
            but.Click += OnClickDelete;
            Controls.Add(but);
            DeleteButton = but;


        }
        public void OnClickCopy(object sender, EventArgs e)
        {
            CopyItem();
        }


        public void OnClickAdd(object sender, EventArgs e)
        {
            AddItems();
        }

        public void OnClickDelete(object sender, EventArgs e)
        {
            DeleteItem();
        }


        public void OnClickDetails (object sender, EventArgs e)
        {
            SaveChanges();
            ShowDetails();
        }

        public void OnClickBack(object sender, EventArgs e)
        {
            SaveChanges();
            if (win != null)
            {
                win.GoBack();
            }
        }

        public void OnClickHome(object sender, EventArgs e)
        {
            SaveChanges();
            if (win != null)
            {
                win.GoHome();
            }
        }

        public void OnClickSave(object sender, EventArgs e)
        {
            SaveChanges();
            if (win != null)
            {

                _ = Task.Run(() => win.SaveData());
            }
        }

        public virtual void ShowData()
        {
            if (_reflectionService.IsMultiType(objType))
            {
                if (parent is IShowChildListAsButton parentListIsButton &&
                    obj is IEnumerable objEnumerable)
                {
                    ShowMultiItemButtonsForList(objEnumerable);
                }
                else
                {
                    ShowMultiTypeData();
                }
            }
            else
            {
                ShowSingleTypeData();
            }

			StartTick();
        }

        public void ShowMultiTypeData()
        {
            if (MultiGrid == null || SingleGrid == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Setting up data", win);
            SingleGrid.Visible = false;
            MultiGrid.Visible = true;
            SetMultiGridDataSource(obj);
            AddButton.Visible = true;
            DeleteButton.Visible = true;
            CopyButton.Visible = true;
            DetailsButton.Visible = true;
          
            form.Hide();
        }

        public bool AllowEditing (GameState gs, MemberInfo mem, bool primitiveEditor)
        {
            if ((mem as PropertyInfo) == null)
            {
                return false;
            }
            if (IsIgnoreField(mem.Name))
            {
                return false;
            }

            FieldInfo finfo = mem as FieldInfo;
            if (finfo != null && finfo.IsStatic)
            {
                return false;
            }

            if (_reflectionService.MemberIsPrimitive(mem) != primitiveEditor)
            {
                return false;
            }
            return true;
        }

        public void ShowSingleTypeData()
        {
            if (MultiGrid == null || SingleGrid == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Setting up data", win);
            SingleGrid.Controls.Clear();
            ShowMultiItemButtonsOnSingleItemView();
            SingleGrid.Visible = true;
            MultiGrid.Visible = false;
            AddButton.Visible = false;
            DeleteButton.Visible = false;
            CopyButton.Visible = false;
            DetailsButton.Visible = false;
             
            int numButtonsPerRow = 8;

            if (win != null)
            {
                numButtonsPerRow = win.Width/(SingleItemWidth+SingleItemWidthPad);
            }

            if (numButtonsPerRow < 3)
            {
                numButtonsPerRow = 3;
            }

            if (numButtonsPerRow > MaxButtonsPerRow)
            {
                numButtonsPerRow = MaxButtonsPerRow;
            }
            int numRowsUsed = 0;

            if (numSingleTopButtonsShown > 0)
            {
                numRowsUsed = numSingleTopButtonsShown / numButtonsPerRow + (numSingleTopButtonsShown % numSingleTopButtonsShown > 0 ? 1 : 0);
            }

            int editorsPerRow = (numButtonsPerRow + 1) / 2;
            int numEditorsShown = 0;
           
            int sx = SingleItemWidth;
            int sy = SingleItemHeight;

            List<MemberInfo> members = _reflectionService.GetMembers(obj);

            for (int i = 0; i < members.Count; i++)
            {
                MemberInfo mem = members[i];

                if (!AllowEditing(gs, mem, true))
                {
                    continue;
                }
                int tx = (numEditorsShown % editorsPerRow) * (SingleItemWidth + SingleItemWidthPad)*2 + SingleItemLeftPad;
                int ty = (numEditorsShown / editorsPerRow + numRowsUsed+1) * (SingleItemHeight + SingleItemHeightPad);

                int ex = tx + SingleItemWidth*4/5 + SingleItemWidthPad;
                int ey = ty;

                Label tb = new Label();
                tb.Name = mem.Name + "Text";
                tb.Text = mem.Name;
                tb.Size = new Size(sx*4/5, sy);
                tb.Location = new Point(tx, ty);
                SingleGrid.Controls.Add(tb);


                AddEntityDesc(obj, mem, SingleGrid, sx, sy, tx, ty);
               


            Control eb = AddSingleControl(mem, SingleGrid.Controls);
                eb.Name = mem.Name + "Edit";



                TextBox tbox = eb as TextBox;

                if (tbox != null && mem.Name.IndexOf("Description") >= 0)
                {
                    tbox.Multiline = true;
                    eb.Size = new Size(sx*6/5, sy * 3 / 2);
                }
                else
                {
                    eb.Size = new Size(sx*6/5, sy);
                }

                eb.Location = new Point(ex, ey);
                object val = _reflectionService.GetObjectValue(obj, mem.Name);

              

                if (val != null)
                {
                    eb.Text = val.ToString();
                }

                CheckBox cbox = eb as CheckBox;
                if (cbox != null)
                {
                    cbox.Text = "";
                    if (val != null && (bool)(val) == true)
                    {
                        cbox.Checked = true;
                    }
                    else
                    {
                        cbox.Checked = false;
                    }
                }

                numEditorsShown++;
            }
            form.Hide();
        }

        private void AddEntityDesc(object parentObject, MemberInfo mem, Panel panel, int sx, int sy, int tx, int ty)
        {
            string desc = EntityDescriptions.GetEntityDescription(parentObject.GetType().Name, mem.Name);

            if (desc != null)
            { 
                Label descLabel = new Label();
                descLabel.Name = mem.Name + "Desc";
                descLabel.Text = desc;
                descLabel.Size = new Size(sx*2, sy * 4/10);
                descLabel.Location = new Point(tx, ty - sy *4/10);
                SingleGrid.Controls.Add(descLabel);
            }
        }

        // Add a control to the parent object. We add it here because then
        // dropdown menus get their items populated here.
        private Control AddSingleControl(MemberInfo mem, ControlCollection coll)
        {
            Type memType = _reflectionService.GetMemberType(mem);

            if (coll == null)
            {
                return new TextBox();
            }

            if (mem == null || memType == null)
            {
                TextBox tb = new TextBox();
                coll.Add(tb);
                return tb;
            }

            if (memType.Name == "bool" || memType.Name == "Boolean")
            {
                CheckBox cbox = new CheckBox();
                coll.Add(cbox);
                return cbox;
            }

            List<NameValue> ddList = _reflectionService.GetDropdownList(gs, mem, obj);

            if (ddList != null && ddList.Count > 0)
            {
                return AddDropdownComboBox(ddList, mem, coll);
            }

            TextBox tb2 = new TextBox();
            coll.Add(tb2);
            return tb2;

        }

        private void OnEntityTypeIndexChanged (object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (SingleGrid == null)
            {
                return;
            }

            if (cb == null)
            {
                return;
            }

            if (cb.Name == null)
            {
                return;
            }

            if (cb.Name.IndexOf("EntityTypeId") < 0)
            {
                return;
            }

            String prefix = cb.Name.Replace("EntityTypeIdEdit", "");

            String memberName = cb.Name.Replace("Edit", "");

			NameValue selItem = cb.SelectedItem as NameValue;

			if (selItem != null)
			{
				_reflectionService.SetObjectValue(obj, memberName, selItem.IdKey);
			}
            string newname = prefix + "EntityIdEdit";

            string newpropname = prefix + "EntityId";

            ComboBox keyCont = null;

            for (int c = 0; c < SingleGrid.Controls.Count; c++)
            {
                Control cont = SingleGrid.Controls[c];
                if (cont.Name == newname)
                {
                    keyCont = cont as ComboBox;
                    if (keyCont != null)
                    {
                        break;
                    }
                }
            }

            if (keyCont == null)
            {
                return;
            }

			object etypeObj = _reflectionService.GetObjectValue(obj, memberName);
			long etype = EntityTypes.None;

			if (etypeObj != null)
			{
				Int64.TryParse(etypeObj.ToString(), out etype);
			}

            EntityService entityService = gs.loc.Get<EntityService>();

            IEntityHelper helper = entityService.GetEntityHelper(etype);

            if (helper == null)
            {
                return;
            }

            string tname = helper.GetDataPropertyName();
            List<NameValue> dataList = _reflectionService.CreateDataList(gs, tname);

            if (dataList == null)
            {
                return;
            }


            // Do this first, or the value gets wiped due to the event here
            object currObj = _reflectionService.GetObjectValue(obj, newpropname);

            // JRAJRA important: this triggers the OnChangeSelected event and it resets the
            // value, so get the currObj BEFORE resetting the list.
            keyCont.DataSource = dataList;


            int currVal = 0;

            if (currObj != null)
            {
                Int32.TryParse(currObj.ToString(), out currVal);
            }

            for (int i = 0; i < dataList.Count; i++)
            {
                NameValue nv = dataList[i] as NameValue;
                if (nv.IdKey == currVal)
                {
                    keyCont.SelectedItem = nv;
                    break;
                }
            }


            //keyCont.Refresh();
        }

        // Three things.
        // 1. Shift-click takes you to the underlying item.
        // 2. Ctrl-click takes you to the underlying list with the item highlighted
        // 3. Alt-click creates a new item and takes you to it and sets its value here.
        private void OnClickEntityIDDropdown(object sender, EventArgs ev)
        {
            Keys modifiers = Control.ModifierKeys;

            if (!modifiers.HasFlag(Keys.Alt) && !modifiers.HasFlag(Keys.Control) && !modifiers.HasFlag(Keys.Shift))
            {
                return;
            }

            ComboBox cb = sender as ComboBox;
            if (cb == null)
            {
                return;
            }

            if (cb.Name.IndexOf("EntityId") < 0)
            {
                return;
            }

            string nm = cb.Name.Replace("Edit", "");

            object val = _reflectionService.GetObjectValue(obj, nm);

            if (val == null)
            {
                return;
            }

            int id = 0;

            Int32.TryParse(val.ToString(), out id);

            string etypeName = nm.Replace("EntityId", "EntityTypeId");

            object etypeObj = _reflectionService.GetObjectValue(obj, etypeName);
			long etype = EntityTypes.None;
			
			if (etypeObj != null)
			{
				Int64.TryParse(etypeObj.ToString(), out etype);
			}


            if (etype == EntityTypes.None)
            {
                return;
            }

            EntityService entityService = gs.loc.Get<EntityService>();

            IEntityHelper helper = entityService.GetEntityHelper(etype);

            if (helper == null)
            {
                return;
            }

            string dataname = helper.GetDataPropertyName();

            if (String.IsNullOrEmpty(dataname))
            {
                return;
            }

            object datalist = _reflectionService.GetObjectValue(gs.data, dataname);

            if (datalist == null)
            {
                return;
            }

            Type dataType = datalist.GetType();

            MethodInfo arrMethod = dataType.GetMethod("ToArray");

            if (arrMethod == null)
            {
                return;
            }

            object arr = arrMethod.Invoke(datalist, new object[0]);

            if (arr == null)
            {
                return;
            }

            Type arrType = arr.GetType();

            object szVal = _reflectionService.GetObjectValue(arr, "Length");

            int sz = 0;

            if (szVal != null)
            {
                Int32.TryParse(szVal.ToString(), out sz);
            }

			UserControlFactory dvf = new UserControlFactory();
            
            MethodInfo readMethod = arrType.GetMethod("GetValue", new[] { typeof(Int32) });

            if (readMethod == null)
            {
                return;
            }


            object currObject = null;

            int oid = 0;
            if (id > 0)
            {

                for (int i = 0; i < sz; i++)
                {
                    object cobj = readMethod.Invoke(arr, new object[] { i });
                    if (cobj == null)
                    {
                        continue;
                    }

                    object oidVal = _reflectionService.GetObjectValue(cobj, GameDataConstants.IdKey);
                    if (oidVal == null)
                    {
                        continue;
                    }

                    Int32.TryParse(oidVal.ToString(), out oid);
                    if (oid == id)
                    {
                        currObject = cobj;
                        break;
                    }
                }
            }

            // Go to item.
            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                if (currObject != null)
                {
                    UserControl dv = dvf.Create (gs, win, currObject, datalist, obj);
                }
                return;
            }

            // Go to list
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
				UserControl uc = dvf.Create(gs, win, datalist, obj, parent);
				DataView dv = uc as DataView;
				if (dv != null)
				{
					dv.SetSelectedItem(currObject);
				}

            }

            // Create new item and go to it.
            if (Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                object datalist2 = _reflectionService.AddItems(datalist, this, gs.repo, out List<object> newItems, null, 1);
                object newObj = _reflectionService.GetItemWithIndex(datalist2, sz);

                if (newObj != null)
                {
                    foreach (object obj in newItems)
                    {
                        gs.LookedAtObjects.Add(obj);
                        if (obj is IGameSettings settings)
                        {
                            gs.data.Set(settings);
                        }
                    }
					UserControlFactory ucf = new UserControlFactory();
                    UserControl uc = ucf.Create (gs, win, newObj, datalist, this);
                    object idObj = _reflectionService.GetObjectValue(newObj, GameDataConstants.IdKey);
                    int nid = -1;
                    if (idObj != null)
                    {
                        Int32.TryParse(idObj.ToString(), out nid);
                    }

                    if (nid > 0)
                    {
                        _reflectionService.SetObjectValue(obj, nm, nid);
                    }
                }
            }

        }

        private void OnComboBoxChanged (object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null || cb.Name == null)
            {
                return;
            }

            string propname = cb.Name.Replace("Edit", "");

            NameValue selItem = cb.SelectedItem as NameValue;
            if (selItem == null)
            {
                return;
            }

            _reflectionService.SetObjectValue(obj, propname, selItem.IdKey);

        }

        private ComboBox AddDropdownComboBox(Object dropdownList, MemberInfo mem, ControlCollection coll)
        {
            if (dropdownList == null || mem == null || coll == null)
            {
                return new ComboBox();
            }

            ComboBox cb2 = new ComboBox();
            cb2.DropDownStyle = ComboBoxStyle.DropDownList;
            cb2.DataSource = dropdownList;
            cb2.ValueMember = GameDataConstants.IdKey;
            cb2.DisplayMember = "Name";
            coll.Add(cb2);
            cb2.CreateControl();
            object val = _reflectionService.GetObjectValue(obj, mem);

            long idVal = 1;

            if (val != null)
            {
            }

            if (val != null)
            {
                try
                {
                    idVal = (long)(val);
                }
                catch (Exception idE)
                {

                }
                for (int i = 0; i < cb2.Items.Count; i++)
                {
                    NameValue item = cb2.Items[i] as NameValue;
                    if (item != null && item.IdKey.ToString() == val.ToString())
                    {
                        cb2.SelectedItem = item;
                        break;
                    }
                }
            }

            if (cb2.SelectedItem == null)
            {
                NameValue errorItem = new NameValue() { Name = "ErrorItem", IdKey = idVal };
                cb2.Items.Add(errorItem);
                cb2.SelectedItem = errorItem;
            }

            cb2.SelectedIndexChanged += OnComboBoxChanged;
            cb2.Click += OnClickComboBox;

            return cb2;
        }

        private void OnClickComboBox(object sender, EventArgs e)
        {
            ComboBox cbox = sender as ComboBox;
            if (cbox == null)
            {
                return;
            }

            MouseEventArgs me = e as MouseEventArgs;

            if (me == null)
            {
                return;
            }


            // Name = field/prop name + "Edit";
            string name = cbox.Name;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            string memberName = "";
            if (name.Length > 4)
            {
                memberName = name.Substring(0, name.Length - 4);
            }


            MemberInfo mem = _reflectionService.GetMemberInfo(obj, memberName);

            string dropdownName = _reflectionService.GetOnClickDropdownName (gs, obj, mem);
            if (string.IsNullOrEmpty(dropdownName))
            {
                return;
            }

            object dropdownList = _reflectionService.GetObjectValue(gs.data, dropdownName);
            if (dropdownList == null)
            {
                return;
            }

            object memberVal = _reflectionService.GetObjectValue(obj, memberName);

            int memberId = 0;

            if (memberVal != null)
            {
                Int32.TryParse(memberVal.ToString(), out memberId);
            }

			UserControlFactory ucf = new UserControlFactory();

            object memberObject = null;
            
            if (memberId > 0)
            {
                memberObject = _reflectionService.GetItemWithId (dropdownList, memberId);
            }

            // On shift, edit the object. On ctrl go to the list, on alt, create new item and go to it.

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                if (memberObject != null)
                {
                    UserControl uc = ucf.Create (gs, win, memberObject, dropdownList, gs.data);
                    return;
                }
            }
            else if ((ModifierKeys & Keys.Control) != 0)
            {
                UserControl uc = ucf.Create (gs, win, dropdownList, gs.data, gs);
                return;
            }
            else if ((ModifierKeys & Keys.Alt) != 0)
            {
                _reflectionService.AddItems(dropdownList, gs.data, gs.repo, out List<object> newItems, null, 1);
                
                dropdownList = _reflectionService.GetObjectValue(gs.data, dropdownName);
                if (dropdownList == null)
                {
                    return;
                }

                object newItem = _reflectionService.GetLastItem (dropdownList);

                if (newItem != null)
                {

                    UserControl uc = ucf.Create (gs, win, newItem, dropdownList, gs.data);
                    return;
                }

            }

        }


        protected void ShowMultiItemButtonsForList(IEnumerable objects)
        {
            SingleGrid.Controls.Clear();
            SingleGrid.Visible = true;
            MultiGrid.Visible = false;
            AddButton.Visible = false;
            DeleteButton.Visible = false;
            CopyButton.Visible = false;
            DetailsButton.Visible = false;

            numSingleTopButtonsShown = 0;
            
            panelGraphics = new Dictionary<String, Graphics>();
            colorPanels = new List<Panel>();
            int numButtonsPerRow = 8;

            if (win != null)
            {
                numButtonsPerRow = win.Width / (SingleItemWidth + SingleItemWidthPad);
            }

            if (numButtonsPerRow < 3)
            {
                numButtonsPerRow = 3;
            }

            if (numButtonsPerRow > MaxButtonsPerRow)
            {
                numButtonsPerRow = MaxButtonsPerRow;
            }
            Size sz = new Size(SingleItemWidth, SingleItemHeight);



            foreach (object obj in objects)
            {
                object memObj = obj;

                int count = 0;

                object countObj = _reflectionService.GetObjectValue(memObj, "Count");
                if (countObj != null)
                {
                    Int32.TryParse(countObj.ToString(), out count);
                }
                if (count < 1)
                {
                    countObj = _reflectionService.GetObjectValue(memObj, "Length");
                    if (countObj != null)
                    {
                        Int32.TryParse(countObj.ToString(), out count);
                    }
                }
                Button but = new Button();

                Type objType = memObj.GetType();
                if (objType.GetGenericArguments().Length > 0)
                {
                    objType = objType.GetGenericArguments()[0];
                }

                but.Name = objType.Name;
                string txt = objType.Name;
                int maxSize = 18;
                if (txt.Length > maxSize)
                {
                    txt = txt.Substring(0, maxSize);
                }

                but.Text = txt;

                int x = (numSingleTopButtonsShown % numButtonsPerRow) * (SingleItemWidth + SingleItemWidthPad) + SingleItemLeftPad;
                int y = (numSingleTopButtonsShown / numButtonsPerRow) * (SingleItemHeight + SingleItemHeightPad) + 5;
                but.Size = sz;
                but.Location = new Point(x, y);
                but.Click += (a, b) => { OnClickSingleObjectButton(obj); };
                SingleGrid.Controls.Add(but);

                numSingleTopButtonsShown++;

            }
        }

        protected void ShowMultiItemButtonsOnSingleItemView()
        {
            numSingleTopButtonsShown = 0;
            List<MemberInfo> members = _reflectionService.GetMembers(obj);

			panelGraphics = new Dictionary<String, Graphics>();
			colorPanels = new List<Panel>();
            int numButtonsPerRow = 8;

            if (win != null)
            {
                numButtonsPerRow = win.Width/(SingleItemWidth+SingleItemWidthPad);
            }

            if (numButtonsPerRow < 3)
            {
                numButtonsPerRow = 3;
            }

            if (numButtonsPerRow > MaxButtonsPerRow)
            {
                numButtonsPerRow = MaxButtonsPerRow;
            }
            Size sz = new Size(SingleItemWidth,SingleItemHeight);

            foreach (MemberInfo mem in members)
            {
                if (!AllowEditing(gs,mem,false))
                {
                    continue;
                }
                object memObj = null;

                try
                {
                    memObj = _reflectionService.GetObjectValue(obj, mem.Name);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception on load member: " + e.Message + " " + e.StackTrace);
                }

                int count = 0;

                object countObj = _reflectionService.GetObjectValue(memObj, "Count");
                if (countObj != null)
                {
                    Int32.TryParse(countObj.ToString(), out count);
                }
                if (count < 1)
                {
                    countObj = _reflectionService.GetObjectValue(memObj, "Length");
                    if (countObj != null)
                    {
                        Int32.TryParse(countObj.ToString(), out count);
                    }
                }
                Button but = new Button();
                but.Name = mem.Name + "Button";
                string txt = mem.Name;
                if (_reflectionService.MemberIsMultiType(mem))
                {
                    txt = count + " " + txt;
                }
                int maxSize = 18;
                if (txt.Length > maxSize)
                {
                    txt = txt.Substring(0, maxSize);
                }

                but.Text = txt;
                
                int x = (numSingleTopButtonsShown % numButtonsPerRow) * (SingleItemWidth + SingleItemWidthPad) + SingleItemLeftPad;
                int y = (numSingleTopButtonsShown / numButtonsPerRow) * (SingleItemHeight+SingleItemHeightPad) + 5;
                but.Size = sz;
                but.Location = new Point(x, y);
                but.Click += OnClickSingleTopButton;
                SingleGrid.Controls.Add(but);

                AddEntityDesc(obj, mem, SingleGrid, SingleItemWidth, SingleItemHeight, x, y);

                numSingleTopButtonsShown++;

				MyColorF col = EntityUtils.GetObjectValue(obj,mem) as MyColorF;

				if (col != null)
				{
					int extraWidth = 3;
					Panel  colorPanel = new Panel();
					colorPanel.Size = new Size(sz.Width + extraWidth*2, sz.Height + extraWidth*2);
					colorPanel.Location = new Point(but.Location.X - extraWidth, but.Location.Y - extraWidth);
					colorPanel.Name = mem.Name;
					SingleGrid.Controls.Add(colorPanel);
					colorPanels.Add(colorPanel);
				}
            }


        }

        private void OnClickSingleObjectButton(object childObj)
        {
            SaveChanges();
            UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create(gs, win, childObj, obj, parent);

        }

        private void OnClickSingleTopButton(object sender, EventArgs e)
        {
            Button but = sender as Button;

            String nm = but.Name;
            if (String.IsNullOrEmpty(nm))
            {
                return;
            }

            if (nm.Length < 6)
            {
                return;
            }

            nm = nm.Substring(0, nm.Length - 6);

            object childObj = _reflectionService.GetObjectValue(obj, nm);

            if (childObj == null)
            {
				
				MemberInfo member = _reflectionService.GetMemberInfo(obj, nm);
				if (member == null)
                {
                    return;
                }

                Type mtype = _reflectionService.GetMemberType(member);
				if (mtype == null)
                {
                    return;
                }

                if (_reflectionService.IsGenericList(mtype))
				{
					_reflectionService.SetObjectValue(obj, member, _reflectionService.CreateGenericList(mtype));
					childObj = _reflectionService.GetObjectValue(obj, nm);
				}
				if (childObj == null)
                {
                    return;
                }
            }

            SaveChanges();
			UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create (gs, win, childObj, obj, parent);

        }

        public void AddItems(Object copyFrom = null)
        {
            int oldSelectedRow = GetSelectedRow(MultiGrid);

            object obj2 = _reflectionService.AddItems(obj, parent, gs.repo, out List<object> newItems, copyFrom);

            if (obj2 != null)
            {
                foreach (object obj in newItems)
                {
                    gs.LookedAtObjects.Add(obj);
                    if (obj is IGameSettings settings)
                    {
                        gs.data.Set(settings);
                        gs.LookedAtObjects.AddRange(settings.GetChildren());
                    }
                }
                SetMultiGridDataSource(obj2);
                obj = obj2;
            }
            SetSelectedRow(MultiGrid, oldSelectedRow);
        }

        public async Task DeleteItem()
        {

            DataGridViewSelectedRowCollection rows = MultiGrid.SelectedRows;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            object item = row.DataBoundItem;

            int oldSelectedRow = GetSelectedRow(MultiGrid);

            object obj2 = _reflectionService.DeleteItem(obj, parent, item);
            Dictionary<Type, IUnitDataLoader> allLoaders = gs.loc.Get<IPlayerDataService>().GetLoaders();

            if (item is BaseWorldData worldData)
            {
                worldData.Delete(gs.repo);
            }

            if (item is IGameSettings settings)
            {
                await gs.repo.Delete(settings);
                List<IGameSettings> children = settings.GetChildren();
                foreach (IGameSettings child in children)
                {
                    await gs.repo.Delete(child);
                }
            }

            if (item is IUnitData unitData)
            {
                unitData.Delete(gs.repo);
            }

            if (obj2 != null)
            {
                SetMultiGridDataSource(obj2);
                obj = obj2;
            }

            SetSelectedRow(MultiGrid, oldSelectedRow);

        }

       

        public void CopyItem()
        {
            DataGridViewSelectedRowCollection rows = MultiGrid.SelectedRows;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            object item = row.DataBoundItem;
            if (item == null)
            {
                return;
            }

            AddItems(item);
        }

        public void ShowDetails()
        {
            DataGridViewSelectedRowCollection rows = MultiGrid.SelectedRows;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            object item = row.DataBoundItem;
            if (item == null)
            {
                return;
            }

            UserControlFactory ucf = new UserControlFactory();
            UserControl uc = ucf.Create (gs, win, item, this, parent);
        }

        public int GetSelectedRow(DataGridView dgv)
        {
            if (dgv == null)
            {
                return -1;
            }

            DataGridViewSelectedRowCollection rows = dgv.SelectedRows;

            if (rows == null || rows.Count < 1)
            {
                return -1;
            }

            DataGridViewRow row = rows[0];

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                DataGridViewRow row2 = dgv.Rows[i];
                if (row2 == row)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SetSelectedRow(DataGridView dgv, int rid)
        {
            if (dgv == null)
            {
                return;
            }

            if (rid >= dgv.Rows.Count)
            {
                rid = dgv.Rows.Count-1;
            }

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (i != rid)
                {
                    dgv.Rows[i].Selected = false;
                }
            }

            if (rid >= 0 && rid < dgv.Rows.Count)
            {
                dgv.Rows[rid].Selected= true;
            }

        }

        public void SaveChanges()
        {
            if (_reflectionService.IsMultiType(objType))
            {
                return;
            }

            List<MemberInfo> members = _reflectionService.GetMembers(objType);

            foreach (MemberInfo mem in members)
            {
                if (!_reflectionService.MemberIsPrimitive(mem))
                {
                    continue;
                }

                string nm = mem.Name;


                if (nm == "LastEditorSaveTime" || nm == "LastEditorBuildTime")
                {
                    continue;
                }


                Control[] conts = SingleGrid.Controls.Find(mem.Name + "Edit", false);

                if (conts == null || conts.Length < 1)
                {
                    continue;
                }

                Control cont = conts[0];

                CheckBox cbox = cont as CheckBox;
                if (cbox != null)
                {
                    _reflectionService.SetObjectValue(obj, mem.Name, cbox.Checked);
                }

                TextBox tb = cont as TextBox;

                if (tb != null)
                {
                    _reflectionService.SetObjectValue(obj, mem.Name, tb.Text);

                    continue;
                }


                ComboBox cb = cont as ComboBox;

                if (cb != null)
                {
                    object item = cb.SelectedItem;

                    NameValue nv = item as NameValue;
                    if (nv != null)
                    {
                        _reflectionService.SetObjectValue(obj, mem.Name, nv.IdKey);
                    }
                    else if (item != null)
                    {
                        String id = _reflectionService.GetIdString(item.ToString());
                        _reflectionService.SetObjectValue(obj, mem.Name, id);
                    }
                }
            }

        }

        public void SetSelectedItem(Object item)
        {
            if (item == null)
            {
                return;
            }

            if (MultiGrid == null)
            {
                return;
            }

            for (int r = 0; r < MultiGrid.Rows.Count; r++)
            {
                DataGridViewRow row = MultiGrid.Rows[r];
                if (row.DataBoundItem == item)
                {
                    SetSelectedRow(MultiGrid, r);
                    break;
                }
            }
        }


        private void DataGridError (object sender, DataGridViewDataErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.ColumnIndex + " -- " + e.Context);
        }

        public void SetMultiGridDataSource(Object list)
        {
            try
            {
                if (MultiGrid == null || list == null)
                {
                    return;
                }

                IReflectionService reflectionService = gs.loc.Get<IReflectionService>();
                IEnumerable elist = list as IEnumerable;
                List<IIdName> iidlist = new List<IIdName>();
                int listCount = 0;
                if (elist != null)
                {
                    foreach (object item in elist)
                    {
                        IIdName iitem = item as IIdName;
                        if (iitem != null)
                        {
                            iidlist.Add(iitem);
                            listCount++;
                        }
                    }
                }
                if (listCount == iidlist.Count && listCount > 0)
                {
                    iidlist = iidlist.OrderBy(x => x.IdKey).ToList();
                    reflectionService.ReplaceIndexedItems(gs, list, iidlist);
                }
                list = reflectionService.SortOnParameter(elist);



                MultiGrid.DataSource = list;
                (MultiGrid.BindingContext[MultiGrid.DataSource] as CurrencyManager).Refresh();
                if (list == null)
                {
                    return;
                }

                Type underlyingType = reflectionService.GetUnderlyingType(obj);

                if (underlyingType == null)
                {
                    return;
                }

                List<MemberInfo> members = reflectionService.GetMembers(underlyingType);
                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo mem = members[i];
                    if (reflectionService.MemberIsPrimitive(mem))
                    {
                        continue;
                    }

                    for (int j = 0; j < MultiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = MultiGrid.Columns[j];
                        if (col.HeaderText == mem.Name)
                        {
                            MultiGrid.Columns.Remove(col);
                            break;
                        }
                    }
                }

                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo mem = members[i];
                    Type mtype = reflectionService.GetMemberType(mem);

                    if (!mtype.IsEnum)
                    {
                        continue;
                    }

                    for (int j = 0; j < MultiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = MultiGrid.Columns[j];
                        if (col.HeaderText == mem.Name)
                        {
                            DataGridViewComboBoxColumn col2 = new DataGridViewComboBoxColumn();
                            
                            Array values = Enum.GetValues(mtype);

                            col2.DataSource = values;
                            col2.DataPropertyName = mem.Name;
                            col2.Name = mem.Name + "Edit";
                            col2.Width += 40;
                            col2.HeaderText = mem.Name;
                            MultiGrid.Columns.RemoveAt(j);
                            MultiGrid.Columns.Add(col2);
                            break;
                        }
                    }
                }
                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo mem = members[i];

                    List<NameValue> dropdownList = reflectionService.GetDropdownList(gs, mem, null);

                    if (dropdownList == null || dropdownList.Count < 1)
                    {
                        continue;
                    }

                    long firstKey = dropdownList.FirstOrDefault()?.IdKey ?? 1;
                    for (int j = 0; j < MultiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = MultiGrid.Columns[j];
                        if (col.HeaderText == mem.Name)
                        {
                            DataGridViewComboBoxColumn col2 = new DataGridViewComboBoxColumn();

                            col2.DataSource = dropdownList;
                            col2.ValueMember = GameDataConstants.IdKey;
                            col2.ValueType = typeof(Int32);
                            col2.DisplayMember = "Name";
                            col2.DataPropertyName = mem.Name;
                            col2.Name = mem.Name + "Edit";
                            col2.HeaderText = mem.Name;
                            col2.Width += 40;
                            MultiGrid.Columns.RemoveAt(j);
                            MultiGrid.Columns.Add(col2);

                            foreach (IIdName item in iidlist)
                            {
                                object valId = reflectionService.GetObjectValue(item, mem);
                                int val = -1;
                                Int32.TryParse(valId.ToString(), out val);
                                bool foundItem = false;
                                foreach (NameValue dl in dropdownList)
                                {
                                    if (dl.IdKey == val)
                                    {
                                        foundItem = true;
                                        break;
                                    }
                                }
                                if (!foundItem)
                                {
                                    reflectionService.SetObjectValue(item, mem, firstKey);
                                }
                            }

                            break;
                        }
                    }
                }

                MultiGrid.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine("Bad dropdown " + e.Message + " -- " + e.StackTrace);
            }
        }

		private void  OnTick (object sender, EventArgs e)
		{
			if (colorPanels == null)
			{
				return;
			}
			int ClearColor = 240;
			Color clearColor = Color.FromArgb(255, ClearColor, ClearColor, ClearColor);
			foreach (Panel cp in colorPanels)
			{
				Graphics graphics = null;
				if (panelGraphics.ContainsKey(cp.Name))
				{
					graphics = panelGraphics[cp.Name];
				}
				else
				{
					graphics = cp.CreateGraphics();
					panelGraphics[cp.Name] = graphics;
				}

				MyColorF col = EntityUtils.GetObjectValue(obj, cp.Name) as MyColorF;
				if (col == null)
				{
					continue;
				}

				Rectangle rect = new Rectangle(0, 0, cp.Size.Width, cp.Size.Height);
				int A = (int)(255 * col.A);
				int R = (int)(255 * col.R);
				int G = (int)(255 * col.G);
				int B = (int)(255 * col.B);
				Color col2 = Color.FromArgb(A, R, G, B);
				graphics.Clear(clearColor);

				graphics.FillRectangle(new SolidBrush(col2), rect);
			}
		}
    }
}
