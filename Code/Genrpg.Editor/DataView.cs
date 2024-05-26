using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.ServerShared.PlayerData;
using System.Threading.Tasks;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Editor.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;
using MongoDB.Driver;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Editor.Entities.MetaData;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;

namespace GameEditor
{
    public partial class DataView : UserControl
    {
        public Object Obj = null;
        private EditorGameState _gs = null;
        private Object _parent = null;
        private Object _grandparent = null;
        protected UIFormatter _formatter = null;
        private Type _objType = null;
        
        
        private DataWindow _window = null;

        public Button AddButton = null;
        public Button DeleteButton = null;
        public Button CopyButton = null;
        public Button DetailsButton = null;
        protected Panel _singleGrid = null;
        protected DataGridView _multiGrid = null;

        private TypeMetaData _typeMetaData = null;


        public const int MaxButtonsPerRow = 8;
        public const int SingleItemWidth = 120;
        public const int SingleItemHeight = 40;
        public const int SingleItemWidthPad = 5;
        public const int SingleItemHeightPad = 20;
        public const int SingleItemTopPad = 60;
        public const int SingleItemLeftPad = 10;
        private int _numSingleTopButtonsShown = 0;

        private IList<Panel> colorPanels = new List<Panel>();
        private IDictionary<string, Graphics> panelGraphics = new Dictionary<string, Graphics>();

        Timer timer = null;


        protected IList<String> IgnoredFields;

        protected IEditorReflectionService _reflectionService;
        protected IGameDataService _gameDataService;
        protected IRepositoryService _repoService;
        protected ILogService _logService;
        protected IGameData _gameData;

        protected bool IsIgnoreField(String nm)
        {
            if (!string.IsNullOrEmpty(nm) &&
                IgnoredFields != null &&
                IgnoredFields.Contains(nm))
            {
                return true;
            }
            return false;
        }

        public DataView(EditorGameState gs, UIFormatter formatter, DataWindow win, Object obj, Object parent, Object grandParent, DataView parentView)
        {
            _formatter = formatter;
            _typeMetaData = gs.data.Get<EditorMetaDataSettings>(null)?.GetMetaDataForType(obj.GetType().Name);
            _gs = gs;
            _gs.loc.Resolve(this);

            if (obj is IEditorScaffold scaffold)
            {
                IEnumerable data = scaffold.GetData();
                if (data != null)
                {
                    grandParent = parent;
                    parent = obj;
                    obj = data;
                }
            }

            Obj = obj;
            _parent = parent;
            _grandparent = grandParent;
            _window = win;
            if (_window != null)
            {
                Size = (_window.Size - new Size(20, 20));
                _window.Controls.Clear();
                _window.Controls.Add(this);
                _window.ViewStack.Add(this);
            }

            SetupGrids();

            InitializeComponent();

            if (!gs.LookedAtObjects.Contains(Obj))
            {
                gs.LookedAtObjects.Add(Obj);
            }

            if (_gameDataService != null)
            {
                IgnoredFields = _gameDataService.GetEditorIgnoreFields();
            }

            if (IgnoredFields == null)
            {
                IgnoredFields = new List<String>();
            }

            _objType = Obj.GetType();
          
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
            return Obj;
        }

        public Object GetParent()
        {
            return _parent;
        }

        public void Save()
        {
            if (_window != null)
            {
                _ = Task.Run(() => _window.SaveData());
            }
        }

        private void SetupGrids()
        {
            if (_window == null)
            {
                return;
            }

            CreateMultiGrid();

            CreateSingleGrid();
        }

        private void CreateMultiGrid()
        {
            if (_multiGrid != null && Controls.Contains(_multiGrid))
            {
                _multiGrid.DataError -= DataGridView1_DataError;
                _multiGrid.SelectionChanged -= MultiGrid_SelectionChanged;
                _multiGrid.CellValueChanged -= MultiGrid_CellValueChanged;
                Controls.Remove(_multiGrid);
            }
            int sx = _window.Width - 16;
            int sy = _window.Height - SingleItemTopPad - 37;
            _multiGrid = UIHelper.CreateDataGridView(Controls, _formatter, "MultiGrid", Width - 16, Height - SingleItemHeight - 37,
                0, SingleItemTopPad);

            _multiGrid.DataError += DataGridView1_DataError;
            _multiGrid.SelectionChanged += MultiGrid_SelectionChanged;
            _multiGrid.CellValueChanged += MultiGrid_CellValueChanged;
            Controls.Add(_multiGrid);
        }

        private void MultiGrid_SelectionChanged(object sender, EventArgs e)
        {
            MarkSelectedRows();
        }

        private bool _markedZeroIdKey = false;
        private bool _markedOtherItem = false;
        private void MarkSelectedRows()
        {
            DataGridViewSelectedRowCollection rows = _multiGrid.SelectedRows;

            if (rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            object item = row.DataBoundItem;

            bool okToAddItem = false;
            if (item is IId idItem && idItem.IdKey == 0)
            {
                if (_markedOtherItem)
                {
                    okToAddItem = true;
                }
            }
            else
            {
                _markedOtherItem = true;
                okToAddItem = true;
            }
    
            if (okToAddItem && !_gs.LookedAtObjects.Contains(item))
            {
                _gs.LookedAtObjects.Add(item);
            }
        }



        private void MultiGrid_CellValueChanged(object sender, EventArgs e)
        {
            MarkSelectedRows();
        }

        private void CreateSingleGrid()
        {
            int sx = _window.Width - 16;
            int sy = _window.Height - SingleItemTopPad - 37;

            _singleGrid = new Panel();
            _singleGrid.AutoScroll = true;
            _singleGrid.Location = new Point(0, SingleItemTopPad);
            _singleGrid.Size = new Size(sx, sy);
            Controls.Add(_singleGrid);
        }


        private bool addedButtons = false;
        private void AddTopButtons()
        {
            if (addedButtons)
            {
                return;
            }

            addedButtons = true;

            if (_window != null)
            {
                UIHelper.CreateLabel(Controls, ELabelTypes.Default, _formatter, "TopLabel", _window.ShowStack(), _window.Width, 20, 10, 10);
            }

            int y = 30;
            int x = 20;
            int w = 100;
            int h = 30;

           UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "HomeButton", "Home", w, h, x, y, OnClickHome); x += w + 5;

           UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "BackButton", "Back", w, h, x, y, OnClickBack); x += w + 5;

            DetailsButton = UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "DetailsButton", "Details", w, h, x, y, OnClickDetails); x += w + 5;

            AddButton = UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "AddButton", "Add", w, h, x, y, OnClickAdd); x += w + 5;

            CopyButton = UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "CopyButton", "Copy", w, h, x, y, OnClickCopy); x += w + 5;

            x += w + 5;
            UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "SaveButton", "Save", w, h, x, y, OnClickSave); x += w + 5;

            x += w + 5;
            DeleteButton = UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "DeleteButton", "Delete", w, h, x, y, OnClickDelete); x += w + 5;

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
            Task.Run(() => DeleteItem());
        }


        public void OnClickDetails(object sender, EventArgs e)
        {
            SaveChanges();
            ShowDetails();
        }

        public void OnClickBack(object sender, EventArgs e)
        {
            SaveChanges();
            if (_window != null)
            {
                _window.GoBack();
            }
        }

        public void OnClickHome(object sender, EventArgs e)
        {
            SaveChanges();
            if (_window != null)
            {
                _window.GoHome();
            }
        }

        public void OnClickSave(object sender, EventArgs e)
        {
            SaveChanges();
            if (_window != null)
            {

                _ = Task.Run(() => _window.SaveData());
            }
        }

        public virtual void ShowData()
        {
            if (_reflectionService.IsMultiType(_objType))
            {
                if (_parent is IShowChildListAsButton parentListIsButton &&
                    Obj is IEnumerable objEnumerable)
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
            if (_multiGrid == null || _singleGrid == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Setting up data", _formatter, _window);
            _singleGrid.Visible = false;
            _multiGrid.Visible = true;
            SetMultiGridDataSource(Obj);
            AddButton.Visible = true;
            DeleteButton.Visible = true;
            CopyButton.Visible = true;
            DetailsButton.Visible = true;

            form.Hide();
        }

        public bool AllowEditing(GameState gs, MemberInfo mem, bool primitiveEditor)
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
            if (_multiGrid == null || _singleGrid == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Setting up data", _formatter, _window);
            _singleGrid.Controls.Clear();
            ShowMultiItemButtonsOnSingleItemView();
            _singleGrid.Visible = true;
            _multiGrid.Visible = false;
            AddButton.Visible = false;
            DeleteButton.Visible = false;
            CopyButton.Visible = false;
            DetailsButton.Visible = false;

            int numButtonsPerRow = 8;

            if (_window != null)
            {
                numButtonsPerRow = _window.Width / (SingleItemWidth + SingleItemWidthPad);
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

            if (_numSingleTopButtonsShown > 0)
            {
                numRowsUsed = _numSingleTopButtonsShown / numButtonsPerRow + (_numSingleTopButtonsShown % _numSingleTopButtonsShown > 0 ? 1 : 0);
            }

            int editorsPerRow = (numButtonsPerRow + 1) / 2;
            int numEditorsShown = 0;

            int sx = SingleItemWidth;
            int sy = SingleItemHeight;

            List<MemberInfo> members = _reflectionService.GetMembers(Obj);

            for (int i = 0; i < members.Count; i++)
            {
                MemberInfo mem = members[i];

                if (!AllowEditing(_gs, mem, true))
                {
                    continue;
                }
                int labelX = (numEditorsShown % editorsPerRow) * (SingleItemWidth + SingleItemWidthPad) * 2 + SingleItemLeftPad;
                int labelY = (numEditorsShown / editorsPerRow + numRowsUsed + 1) * (SingleItemHeight + SingleItemHeightPad);

                int controlX = labelX + SingleItemWidth * 4 / 5 + SingleItemWidthPad;
                int controlY = labelY;

                UIHelper.CreateLabel(_singleGrid.Controls, ELabelTypes.Default, _formatter, mem.Name + "Label", mem.Name, sx * 4 / 5, sy, labelX, labelY);

                AddEntityDesc(Obj, mem, _singleGrid, sx, sy, labelX, labelY);

                Control eb = AddSingleControl(mem, _singleGrid.Controls, sx, sy, controlX, controlY);

                if (eb != null)
                {
                    TextBox tbox = eb as TextBox;

                    if (tbox != null && mem.Name.IndexOf("Description") >= 0)
                    {
                        tbox.Multiline = true;
                        eb.Size = new Size(sx * 6 / 5, sy * 3 / 2);
                    }
                    else
                    {
                        eb.Size = new Size(sx * 6 / 5, sy);
                    }

                    eb.Location = new Point(controlX, controlY);
                    object val = _reflectionService.GetObjectValue(Obj, mem.Name);

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
                }

                numEditorsShown++;
            }
            form.Hide();
        }

        private void AddEntityDesc(object parentObject, MemberInfo mem, Panel panel, int sx, int sy, int tx, int ty)
        {
            MemberMetaData metaData = _typeMetaData?.MemberData?.FirstOrDefault(x => x.MemberName == mem.Name);
            if (metaData != null && !string.IsNullOrEmpty(metaData.Description))
            {
                int height = 20;
                UIHelper.CreateLabel(_singleGrid.Controls, ELabelTypes.Default, _formatter, mem.Name + "Desc", metaData.Description,
                    250, height, tx, ty - height, FormatterConstants.SmallLabelFontSize);
            }
        }

        // Add a control to the parent object. We add it here because then
        // dropdown menus get their items populated here.
        private Control AddSingleControl(MemberInfo mem, ControlCollection coll, int width, int height, int xpos, int ypos)
        {
            Type memType = _reflectionService.GetMemberType(mem);

            if (coll == null)
            {
                return null;
            }

            if (mem == null || memType == null)
            {
                return null;
            }

            if (memType.Name == "bool" || memType.Name == "Boolean")
            {

                return UIHelper.CreateCheckBox(coll, _formatter, mem.Name + "Edit", width, height, xpos, ypos);
            }

            List<NameValue> ddList = _reflectionService.GetDropdownList(_gs, mem, Obj);

            if (ddList != null && ddList.Count > 0)
            {
                return AddDropdownComboBox(ddList, mem, coll, width, height, xpos, ypos);
            }

            TextBox textBox = UIHelper.CreateTextBox(coll, _formatter, mem.Name + "Edit", null, width, height, xpos, ypos, null);
            return textBox;
        }

        private void OnEntityTypeIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (_singleGrid == null)
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
                _reflectionService.SetObjectValue(Obj, memberName, selItem.IdKey);
            }
            string newname = prefix + "EntityIdEdit";

            string newpropname = prefix + "EntityId";

            ComboBox keyCont = null;

            for (int c = 0; c < _singleGrid.Controls.Count; c++)
            {
                Control cont = _singleGrid.Controls[c];
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

            object etypeObj = _reflectionService.GetObjectValue(Obj, memberName);
            long etype = EntityTypes.None;

            if (etypeObj != null)
            {
                Int64.TryParse(etypeObj.ToString(), out etype);
            }

            EntityService entityService = _gs.loc.Get<EntityService>();

            IEntityHelper helper = entityService.GetEntityHelper(etype);

            if (helper == null)
            {
                return;
            }

            string tname = helper.GetDataPropertyName();
            List<NameValue> dataList = _reflectionService.CreateDataList(_gs, tname);

            if (dataList == null)
            {
                return;
            }


            // Do this first, or the value gets wiped due to the event here
            object currObj = _reflectionService.GetObjectValue(Obj, newpropname);

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

            object val = _reflectionService.GetObjectValue(Obj, nm);

            if (val == null)
            {
                return;
            }

            int id = 0;

            Int32.TryParse(val.ToString(), out id);

            string etypeName = nm.Replace("EntityId", "EntityTypeId");

            object etypeObj = _reflectionService.GetObjectValue(Obj, etypeName);
            long etype = EntityTypes.None;

            if (etypeObj != null)
            {
                Int64.TryParse(etypeObj.ToString(), out etype);
            }


            if (etype == EntityTypes.None)
            {
                return;
            }

            EntityService entityService = _gs.loc.Get<EntityService>();

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

            object datalist = _reflectionService.GetObjectValue(_gs.data, dataname);

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
                    UserControl dv = dvf.Create(_gs, _formatter, _window, currObject, datalist, Obj, this);
                }
                return;
            }

            // Go to list
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
                UserControl uc = dvf.Create(_gs, _formatter, _window, datalist, Obj, _parent, this);
                DataView dv = uc as DataView;
                if (dv != null)
                {
                    dv.SetSelectedItem(currObject);
                }

            }

            // Create new item and go to it.
            if (Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                object datalist2 = _reflectionService.AddItems(datalist, this, _repoService, out List<object> newItems, null, 1);
                object newObj = _reflectionService.GetItemWithIndex(datalist2, sz);

                if (newObj != null)
                {
                    foreach (object obj in newItems)
                    {
                        _gs.LookedAtObjects.Add(obj);
                        if (obj is ITopLevelSettings settings)
                        {
                            _gameData.Set(settings);
                        }
                    }
                    UserControlFactory ucf = new UserControlFactory();
                    UserControl uc = ucf.Create(_gs, _formatter, _window, newObj, datalist, this, this);
                    object idObj = _reflectionService.GetObjectValue(newObj, GameDataConstants.IdKey);
                    int nid = -1;
                    if (idObj != null)
                    {
                        Int32.TryParse(idObj.ToString(), out nid);
                    }

                    if (nid > 0)
                    {
                        _reflectionService.SetObjectValue(Obj, nm, nid);
                    }
                }
            }

        }

        private void OnComboBoxChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null || cb.Name == null)
            {
                return;
            }

            string propName = cb.Name.Replace("Edit", "");

            NameValue selItem = cb.SelectedItem as NameValue;
            if (selItem == null)
            {
                return;
            }

            _reflectionService.SetObjectValue(Obj, propName, selItem.IdKey);

            if (propName.IndexOf("EntityTypeId") >= 0)
            {
                string entityPropName = propName.Replace("EntityTypeId", "EntityId");

                string entityControlName = entityPropName + "Edit";

                Control currentControl = _singleGrid.Controls.Find(entityControlName, true).FirstOrDefault();

                MemberInfo mem = Obj.GetType().GetMember(entityPropName).FirstOrDefault();

                if (currentControl != null && mem != null)
                {

                    Console.WriteLine("EntityTypeId Changed");
                    _singleGrid.Controls.Remove(currentControl);

                    Control newControl = AddSingleControl(mem, _singleGrid.Controls, currentControl.Size.Width, currentControl.Size.Height,
                        currentControl.Location.X, currentControl.Location.Y);

                }
            }
        }

        private ComboBox AddDropdownComboBox(Object dropdownList, MemberInfo mem, ControlCollection coll, int xsize, int ysize, int xpos, int ypos)
        {
            if (dropdownList == null || mem == null || coll == null)
            {
                return null;
            }

            ComboBox comboBox = UIHelper.CreateComboBox(coll, _formatter, "DropDown", xsize, ysize, xpos, ypos);
            comboBox.DataSource = dropdownList;
            comboBox.ValueMember = GameDataConstants.IdKey;
            comboBox.DisplayMember = "Name";
            comboBox.Name = mem.Name + "Edit";

            object val = _reflectionService.GetObjectValue(Obj, mem);

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
                    Console.Write("Combobox exception: " + idE.Message);
                }
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    NameValue item = comboBox.Items[i] as NameValue;
                    if (item != null && item.IdKey.ToString() == val.ToString())
                    {
                        comboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (comboBox.SelectedItem == null)
            {
                NameValue errorItem = new NameValue() { Name = "ErrorItem", IdKey = idVal };
                comboBox.Items.Add(errorItem);
                comboBox.SelectedItem = errorItem;
            }

            comboBox.SelectedIndexChanged += OnComboBoxChanged;
            comboBox.Click += OnClickComboBox;

            return comboBox;
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


            MemberInfo mem = _reflectionService.GetMemberInfo(Obj, memberName);

            string dropdownName = _reflectionService.GetOnClickDropdownName(_gs, Obj, mem);
            if (string.IsNullOrEmpty(dropdownName))
            {
                return;
            }

            object dropdownList = _reflectionService.GetObjectValue(_gs.data, dropdownName);
            if (dropdownList == null)
            {
                return;
            }

            object memberVal = _reflectionService.GetObjectValue(Obj, memberName);

            int memberId = 0;

            if (memberVal != null)
            {
                Int32.TryParse(memberVal.ToString(), out memberId);
            }

            UserControlFactory ucf = new UserControlFactory();

            object memberObject = null;

            if (memberId > 0)
            {
                memberObject = _reflectionService.GetItemWithId(dropdownList, memberId);
            }

            // On shift, edit the object. On ctrl go to the list, on alt, create new item and go to it.

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                if (memberObject != null)
                {
                    UserControl uc = ucf.Create(_gs, _formatter, _window, memberObject, dropdownList, _gs.data, this);
                    return;
                }
            }
            else if ((ModifierKeys & Keys.Control) != 0)
            {
                UserControl uc = ucf.Create(_gs, _formatter, _window, dropdownList, _gs.data, _gs, this);
                return;
            }
            else if ((ModifierKeys & Keys.Alt) != 0)
            {
                _reflectionService.AddItems(dropdownList, _gs.data, _repoService, out List<object> newItems, null, 1);

                dropdownList = _reflectionService.GetObjectValue(_gs.data, dropdownName);
                if (dropdownList == null)
                {
                    return;
                }

                object newItem = _reflectionService.GetLastItem(dropdownList);

                if (newItem != null)
                {

                    UserControl uc = ucf.Create(_gs, _formatter, _window, newItem, dropdownList, _gs.data, this);
                    return;
                }

            }

        }


        protected void ShowMultiItemButtonsForList(IEnumerable objects)
        {
            _singleGrid.Controls.Clear();
            _singleGrid.Visible = true;
            _multiGrid.Visible = false;
            AddButton.Visible = false;
            DeleteButton.Visible = false;
            CopyButton.Visible = false;
            DetailsButton.Visible = false;

            _numSingleTopButtonsShown = 0;

            panelGraphics = new Dictionary<String, Graphics>();
            colorPanels = new List<Panel>();
            int numButtonsPerRow = 8;

            if (_window != null)
            {
                numButtonsPerRow = _window.Width / (SingleItemWidth + SingleItemWidthPad);
            }

            if (numButtonsPerRow < 3)
            {
                numButtonsPerRow = 3;
            }

            if (numButtonsPerRow > MaxButtonsPerRow)
            {
                numButtonsPerRow = MaxButtonsPerRow;
            }
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
                Type objType = memObj.GetType();
                if (objType.GetGenericArguments().Length > 0)
                {
                    objType = objType.GetGenericArguments()[0];
                }

                string txt = objType.Name;
                int maxSize = 18;
                if (txt.Length > maxSize)
                {
                    txt = txt.Substring(0, maxSize);
                }

                int x = (_numSingleTopButtonsShown % numButtonsPerRow) * (SingleItemWidth + SingleItemWidthPad) + SingleItemLeftPad;
                int y = (_numSingleTopButtonsShown / numButtonsPerRow) * (SingleItemHeight + SingleItemHeightPad) + 5;


                UIHelper.CreateButton(_singleGrid.Controls, EButtonTypes.Default, _formatter, objType.Name, txt, SingleItemWidth, SingleItemHeight,
                    x, y, (a, b) => { OnClickSingleObjectButton(obj); });


                _numSingleTopButtonsShown++;

            }
        }

        protected void ShowMultiItemButtonsOnSingleItemView()
        {
            _numSingleTopButtonsShown = 0;
            List<MemberInfo> members = _reflectionService.GetMembers(Obj);

            panelGraphics = new Dictionary<String, Graphics>();
            colorPanels = new List<Panel>();
            int numButtonsPerRow = 8;

            if (_window != null)
            {
                numButtonsPerRow = _window.Width / (SingleItemWidth + SingleItemWidthPad);
            }

            if (numButtonsPerRow < 3)
            {
                numButtonsPerRow = 3;
            }

            if (numButtonsPerRow > MaxButtonsPerRow)
            {
                numButtonsPerRow = MaxButtonsPerRow;
            }
            foreach (MemberInfo mem in members)
            {
                if (!AllowEditing(_gs, mem, false))
                {
                    continue;
                }
                object memObj = null;

                try
                {
                    memObj = _reflectionService.GetObjectValue(Obj, mem.Name);
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

                int x = (_numSingleTopButtonsShown % numButtonsPerRow) * (SingleItemWidth + SingleItemWidthPad) + SingleItemLeftPad;
                int y = (_numSingleTopButtonsShown / numButtonsPerRow) * (SingleItemHeight + SingleItemHeightPad) + 5;

                Button button = UIHelper.CreateButton(_singleGrid.Controls, EButtonTypes.Default, _formatter, mem.Name + "Button", txt, SingleItemWidth, SingleItemHeight,
                    x, y, OnClickSingleTopButton);

                AddEntityDesc(Obj, mem, _singleGrid, SingleItemWidth, SingleItemHeight, x, y);

                _numSingleTopButtonsShown++;

                MyColorF col = EntityUtils.GetObjectValue(Obj, mem) as MyColorF;

                if (col != null)
                {
                    int extraWidth = 3;
                    Panel colorPanel = new Panel();
                    colorPanel.Size = new Size(button.Width + extraWidth * 2, button.Height + extraWidth * 2);
                    colorPanel.Location = new Point(button.Location.X - extraWidth, button.Location.Y - extraWidth);
                    colorPanel.Name = mem.Name;
                    _singleGrid.Controls.Add(colorPanel);
                    colorPanels.Add(colorPanel);
                }
            }


        }

        private void OnClickSingleObjectButton(object childObj)
        {
            SaveChanges();
            UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create(_gs, _formatter, _window, childObj, Obj, _parent, this);

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

            object childObj = _reflectionService.GetObjectValue(Obj, nm);

            if (childObj == null)
            {

                MemberInfo member = _reflectionService.GetMemberInfo(Obj, nm);
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
                    _reflectionService.SetObjectValue(Obj, member, _reflectionService.CreateGenericList(mtype));
                    childObj = _reflectionService.GetObjectValue(Obj, nm);
                }
                if (childObj == null)
                {
                    return;
                }
            }

            SaveChanges();
            UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create(_gs, _formatter, _window, childObj, Obj, _parent, this);

        }

        public void AddItems(Object copyFrom = null)
        {
            int oldSelectedRow = GetSelectedRow(_multiGrid);

            object obj2 = _reflectionService.AddItems(Obj, _parent, _repoService, out List<object> newItems, copyFrom);

            object oneNewItem = null;

            bool addedGameSettings = false;
            if (obj2 != null)
            {
                foreach (object newItem in newItems)
                {
                    _gs.LookedAtObjects.Add(newItem);
                    if (newItem is ITopLevelSettings settings)
                    {
                        _gameData.Set(settings);
                        _gs.LookedAtObjects.AddRange(settings.GetChildren());
                        addedGameSettings = true;
                        oneNewItem = newItem;
                    }
                }
                SetMultiGridDataSource(obj2);
                Obj = obj2;
            }

            if (oneNewItem is ChildSettings childSettings)
            {
                if (_parent is IGameSettings parentSettings)
                {
                    MemberInfo memberInfo = _reflectionService.GetMemberInfo(_parent, "_data");
                    if (memberInfo != null)
                    {
                        _reflectionService.SetObjectValue(_parent, _reflectionService.GetMemberInfo(_parent, "_data"), Obj);
                    }
                }
            }

            if (addedGameSettings)
            {
                _gameData.ClearIndex();
            }

            SetSelectedRow(_multiGrid, oldSelectedRow);
        }

        public async Task DeleteItem()
        {

            DataGridViewSelectedRowCollection rows = _multiGrid.SelectedRows;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            object item = row.DataBoundItem;

            int oldSelectedRow = GetSelectedRow(_multiGrid);

            object obj2 = _reflectionService.DeleteItem(Obj, _parent, item);
            Dictionary<Type, IUnitDataLoader> allLoaders = _gs.loc.Get<IPlayerDataService>().GetLoaders();

            if (item is BaseWorldData worldData)
            {
                worldData.Delete(_repoService);
                _gs.LookedAtObjects.Remove(worldData);
            }

            if (item is IGameSettings settings)
            {
                await _repoService.Delete(settings);
                _gs.LookedAtObjects.Remove(settings);
                List<IGameSettings> children = settings.GetChildren();
                foreach (IGameSettings child in children)
                {
                    await _repoService.Delete(child);
                    _gs.LookedAtObjects.Remove(child);
                }

                _gameData.ClearIndex();

            }

            if (item is IUnitData unitData)
            {
                unitData.QueueDelete(_repoService);
                _gs.LookedAtObjects.Remove(unitData);
            }

            if (obj2 != null)
            {
                SetMultiGridDataSource(obj2);
                Obj = obj2;
            }

            SetSelectedRow(_multiGrid, oldSelectedRow);

        }



        public void CopyItem()
        {
            DataGridViewSelectedRowCollection rows = _multiGrid.SelectedRows;
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
            DataGridViewSelectedRowCollection rows = _multiGrid.SelectedRows;
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
            UserControl uc = ucf.Create(_gs, _formatter, _window, item, this, this, this);
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
                rid = dgv.Rows.Count - 1;
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
                dgv.Rows[rid].Selected = true;
            }

        }

        public void SaveChanges()
        {
            if (_reflectionService.IsMultiType(_objType))
            {
                return;
            }

            List<MemberInfo> members = _reflectionService.GetMembers(_objType);

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

                Control[] conts = _singleGrid.Controls.Find(mem.Name + "Edit", false);

                if (conts == null || conts.Length < 1)
                {
                    continue;
                }

                Control cont = conts[0];

                CheckBox cbox = cont as CheckBox;
                if (cbox != null)
                {
                    _reflectionService.SetObjectValue(Obj, mem.Name, cbox.Checked);
                }

                TextBox tb = cont as TextBox;

                if (tb != null)
                {
                    _reflectionService.SetObjectValue(Obj, mem.Name, tb.Text);

                    continue;
                }


                ComboBox cb = cont as ComboBox;

                if (cb != null)
                {
                    object item = cb.SelectedItem;

                    NameValue nv = item as NameValue;
                    if (nv != null)
                    {
                        _reflectionService.SetObjectValue(Obj, mem.Name, nv.IdKey);
                    }
                    else if (item != null)
                    {
                        String id = _reflectionService.GetIdString(item.ToString());
                        _reflectionService.SetObjectValue(Obj, mem.Name, id);
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

            if (_multiGrid == null)
            {
                return;
            }

            for (int r = 0; r < _multiGrid.Rows.Count; r++)
            {
                DataGridViewRow row = _multiGrid.Rows[r];
                if (row.DataBoundItem == item)
                {
                    SetSelectedRow(_multiGrid, r);
                    break;
                }
            }
        }


        private void DataGridError(object sender, DataGridViewDataErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.ColumnIndex + " -- " + e.Context);
        }

        public void SetMultiGridDataSource(Object list)
        {
            Invoke(() =>
            {

                if (_multiGrid == null || list == null)
                {
                    return;
                }

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
                    _reflectionService.ReplaceIndexedItems(_gs, list, iidlist);
                }
                list = _reflectionService.SortOnParameter(elist);

                CreateMultiGrid();
                _multiGrid.ClearSelection();
                _multiGrid.DataSource = list;
                (_multiGrid.BindingContext[_multiGrid.DataSource] as CurrencyManager).Refresh();
                _multiGrid.Visible = false;

                Type underlyingType = _reflectionService.GetUnderlyingType(Obj);

                if (underlyingType == null)
                {
                    return;
                }

                for (int j = 0; j < _multiGrid.Columns.Count; j++)
                {
                    DataGridViewColumn col = _multiGrid.Columns[j];
                    col.Width -= 20;
                }

                List<string> ignoreFields = _gameDataService.GetEditorIgnoreFields();
                List<MemberInfo> members = _reflectionService.GetMembers(underlyingType);

                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo mem = members[i];
                    if (_reflectionService.MemberIsPrimitive(mem))
                    {
                        continue;
                    }

                    for (int j = 0; j < _multiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = _multiGrid.Columns[j];
                        if (col.HeaderText == mem.Name)
                        {
                            _multiGrid.Columns.Remove(col);
                            break;
                        }
                    }
                }


                foreach (string ignored in ignoreFields)
                {
                    for (int j = 0; j < _multiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = _multiGrid.Columns[j];
                        if (col.HeaderText == ignored)
                        {
                            _multiGrid.Columns.Remove(col);
                            break;
                        }
                    }
                }



                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo mem = members[i];
                    Type mtype = _reflectionService.GetMemberType(mem);

                    if (!mtype.IsEnum)
                    {
                        continue;
                    }

                    for (int j = 0; j < _multiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = _multiGrid.Columns[j];
                        if (col.HeaderText == mem.Name)
                        {
                            DataGridViewComboBoxColumn col2 = new DataGridViewComboBoxColumn();

                            Array values = Enum.GetValues(mtype);

                            col2.DataSource = values;
                            col2.DataPropertyName = mem.Name;
                            col2.Name = mem.Name + "Edit";
                            col2.Width += 10;
                            col2.HeaderText = mem.Name;
                            _multiGrid.Columns.RemoveAt(j);
                            _multiGrid.Columns.Add(col2);
                            break;
                        }
                    }
                }
                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo mem = members[i];

                    List<NameValue> dropdownList = _reflectionService.GetDropdownList(_gs, mem, null);

                    if (dropdownList == null || dropdownList.Count < 1)
                    {
                        continue;
                    }

                    long firstKey = dropdownList.FirstOrDefault()?.IdKey ?? 1;
                    for (int j = 0; j < _multiGrid.Columns.Count; j++)
                    {
                        DataGridViewColumn col = _multiGrid.Columns[j];
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
                            col2.Width += 10;
                            _multiGrid.Columns.RemoveAt(j);
                            _multiGrid.Columns.Add(col2);

                            foreach (IIdName item in iidlist)
                            {
                                object valId = _reflectionService.GetObjectValue(item, mem);
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
                                    _reflectionService.SetObjectValue(item, mem, firstKey);
                                }
                            }

                            break;
                        }
                    }
                }

                _multiGrid.Refresh();
                _formatter.SetupDataGrid(_multiGrid);
                _multiGrid.Show();
            });
        }

        private void OnTick(object sender, EventArgs e)
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

                MyColorF col = EntityUtils.GetObjectValue(Obj, cp.Name) as MyColorF;
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
        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
        }
    }
}