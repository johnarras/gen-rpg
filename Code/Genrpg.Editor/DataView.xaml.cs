using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
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
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Editor.Entities.MetaData;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using System.Threading;
using Microsoft.UI.Xaml;
using Windows.System;
using System.Runtime.CompilerServices;
using Windows.UI.Input;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Graphics.Printing.PrintSupport;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;

namespace Genrpg.Editor
{
    public partial class DataView : UserControl, IUICanvas
    {
        public Object Obj = null;
        private EditorGameState _gs = null;
        private Object _parent = null;
        private Object _grandparent = null;
        private Type _objType = null;


        private DataWindow _window = null;

        public Button AddButton = null;
        public Button DeleteButton = null;
        public Button CopyButton = null;
        public Button DetailsButton = null;
        protected MyCanvas _singleGrid = null;
        protected DataGrid _multiGrid = null;

        private TypeMetaData _typeMetaData = null;


        public const int MaxButtonsPerRow = 8;
        public const int SingleItemWidth = 120;
        public const int SingleItemHeight = 40;
        public const int SingleItemWidthPad = 5;
        public const int SingleItemHeightPad = 20;
        public const int SingleItemTopPad = 60;
        public const int SingleItemLeftPad = 10;
        private int _numSingleTopButtonsShown = 0;
        private bool _everShownMultigrid = false;

        private IList<MyCanvas> colorPanels = new List<MyCanvas>();

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

        public DataView(EditorGameState gs, DataWindow win, Object obj, Object parent, Object grandParent, DataView parentView)
        {
            Content = _canvas;
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
                Width = _window.Width - 20;
                Height = _window.Height - 20;
                _window.AddChildView(this);
            }

            SetupGrids();

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

        }

        public void StartTick()
        {

            timer = new Timer(OnTick, null, 0, 75);
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
            if (_multiGrid != null && Contains(_multiGrid))
            {
                _multiGrid.SelectionChanged -= MultiGrid_SelectionChanged;
                _multiGrid.CurrentCellChanged -= MultiGrid_CellValueChanged;

                Remove(_multiGrid);
            }
            int sx = _window.Width - 16;
            int sy = _window.Height - SingleItemTopPad - 37;
            _multiGrid = UIHelper.CreateDataGridView(this, "MultiGrid", (int)Width - 16, (int)Height - SingleItemHeight - 37,
                0, SingleItemTopPad);



            _multiGrid.SelectionChanged += MultiGrid_SelectionChanged;
            _multiGrid.CurrentCellChanged += MultiGrid_CellValueChanged;
        }



        private void MultiGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {
            MarkSelectedRows();
        }

        private bool _markedOtherItem = false;
        private void MarkSelectedRows()
        {
            IList rows = _multiGrid.SelectedItems;

            if (rows.Count < 1)
            {
                return;
            }

            object item = rows[0];

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

            _singleGrid = new MyCanvas()
            {
                Width = sx,
                Height = sy,
            };
            Add(_singleGrid, 0, SingleItemTopPad);
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
                UIHelper.CreateLabel (this, ELabelTypes.Default, "TopLabel", _window.ShowStack(), _window.Width, 20, 10, 10);
            }

            int y = 30;
            int x = 20;
            int w = 100;
            int h = 30;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "HomeButton", "Home", w, h, x, y, OnClickHome); x += w + 5;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "BackButton", "Back", w, h, x, y, OnClickBack); x += w + 5;

            DetailsButton = UIHelper.CreateButton(this, EButtonTypes.TopBar, "DetailsButton", "Details", w, h, x, y, OnClickDetails); x += w + 5;

            AddButton = UIHelper.CreateButton(this, EButtonTypes.TopBar, "AddButton", "Add", w, h, x, y, OnClickAdd); x += w + 5;

            CopyButton = UIHelper.CreateButton(this, EButtonTypes.TopBar, "CopyButton", "Copy", w, h, x, y, OnClickCopy); x += w + 5;

            x += w + 5;
            UIHelper.CreateButton(this, EButtonTypes.TopBar, "SaveButton", "Save", w, h, x, y, OnClickSave); x += w + 5;

            x += w + 5;
            DeleteButton = UIHelper.CreateButton(this, EButtonTypes.TopBar, "DeleteButton", "Delete", w, h, x, y, OnClickDelete); x += w + 5;

        }
        public void OnClickCopy(object sender, RoutedEventArgs e)
        {
            CopyItem();
        }


        public void OnClickAdd(object sender, RoutedEventArgs e)
        {
            AddItems();
        }

        public void OnClickDelete(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => DeleteItem());
        }


        public void OnClickDetails(object sender, RoutedEventArgs e)
        {
            SaveChanges();
            ShowDetails();
        }

        public void OnClickBack(object sender, RoutedEventArgs e)
        {
            SaveChanges();
            if (_window != null)
            {
                _window.GoBack();
            }
        }

        public void OnClickHome(object sender, RoutedEventArgs e)
        {
            SaveChanges();
            if (_window != null)
            {
                _window.GoHome();
            }
        }

        public void OnClickSave(object sender, RoutedEventArgs e)
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

            SmallPopup form = UIHelper.ShowBlockingDialog(_window, "Setting up data");
            _singleGrid.Visibility = Visibility.Collapsed;
            _multiGrid.Visibility = Visibility.Visible;
            SetMultiGridDataSource(Obj);
            AddButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            DetailsButton.Visibility = Visibility.Visible;

            form.StartClose();
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

            SmallPopup form = UIHelper.ShowBlockingDialog(_window, "Setting up data");

            _singleGrid.Children.Clear();
            ShowMultiItemButtonsOnSingleItemView();
            _singleGrid.Visibility = Visibility.Visible;
            _multiGrid.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            CopyButton.Visibility = Visibility.Collapsed;
            DetailsButton.Visibility = Visibility.Collapsed;

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

                UIHelper.CreateLabel(_singleGrid, ELabelTypes.Default, mem.Name + "Label", mem.Name, sx * 4 / 5, sy, labelX, labelY);

                AddEntityDesc(Obj, mem, _singleGrid, sx, sy, labelX, labelY);

                Control eb = AddSingleControl(mem, _singleGrid, sx, sy, controlX, controlY);

                if (eb != null)
                {
                    TextBox tbox = eb as TextBox;

                    if (tbox != null && mem.Name.IndexOf("Description") >= 0)
                    {
                        tbox.AcceptsReturn = true;
                        tbox.TextWrapping = TextWrapping.Wrap;
                        eb.Width = sx * 6 / 5;
                        eb.Height = sy * 3 / 2;
                    }
                    else
                    {
                        eb.Width = sx * 6 / 5;
                        eb.Height = sy;
                    }

                    UIHelper.SetPosition(eb, controlX, controlY);

                    object val = _reflectionService.GetObjectValue(Obj, mem.Name);

                    if (tbox != null && val != null)
                    {
                        tbox.Text = val.ToString();
                    }

                    CheckBox cbox = eb as CheckBox;
                    if (cbox != null)
                    {
                        cbox.Content = "";
                        if (val != null && (bool)(val) == true)
                        {
                            cbox.IsChecked = true;
                        }
                        else
                        {
                            cbox.IsChecked = false;
                        }
                    }
                }

                numEditorsShown++;
            }


            form.StartClose();
        }

        private void AddEntityDesc(object parentObject, MemberInfo mem, Panel panel, int sx, int sy, int tx, int ty)
        {
            MemberMetaData metaData = _typeMetaData?.MemberData?.FirstOrDefault(x => x.MemberName == mem.Name);
            if (metaData != null && !string.IsNullOrEmpty(metaData.Description))
            {
                int height = 20;
                UIHelper.CreateLabel(_singleGrid, ELabelTypes.Default, mem.Name + "Desc", metaData.Description,
                    250, height, tx, ty - height, FormatterConstants.SmallLabelFontSize);
            }
        }

        // Add a control to the parent object. We add it here because then
        // dropdown menus get their items populated here.
        private Control AddSingleControl(MemberInfo mem, IUICanvas coll, int width, int height, int xpos, int ypos)
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

                return UIHelper.CreateCheckBox(coll, mem.Name + "Edit", width, height, xpos, ypos);
            }

            List<IIdName> ddList = _reflectionService.GetDropdownList(_gs, mem, Obj);

            if (ddList != null && ddList.Count > 0)
            {
                return AddDropdownComboBox(ddList, mem, coll, width, height, xpos, ypos);
            }

            TextBox textBox = UIHelper.CreateTextBox(coll, mem.Name + "Edit", null, width, height, xpos, ypos, null);
            return textBox;
        }

        private void OnEntityTypeIndexChanged(object sender, RoutedEventArgs e)
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

            IIdName selItem = cb.SelectedItem as IIdName;

            if (selItem != null)
            {
                _reflectionService.SetObjectValue(Obj, memberName, selItem.IdKey);
            }
            string newname = prefix + "EntityIdEdit";

            string newpropname = prefix + "EntityId";

            ComboBox keyCont = null;

            for (int c = 0; c < _singleGrid.Children.Count; c++)
            {
                Control cont = _singleGrid.Children[c] as Control;
                if (cont != null && cont.Name == newname)
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


            List<IIdName> dataList = helper.GetChildList(null);

            // Do this first, or the value gets wiped due to the event here
            object currObj = _reflectionService.GetObjectValue(Obj, newpropname);

            // JRAJRA important: this triggers the OnChangeSelected event and it resets the
            // value, so get the currObj BEFORE resetting the list.
            keyCont.ItemsSource = dataList;

            int currVal = 0;

            if (currObj != null)
            {
                Int32.TryParse(currObj.ToString(), out currVal);
            }

            for (int i = 0; i < dataList.Count; i++)
            {
                IIdName nv = dataList[i] as IIdName;
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
        private void OnClickEntityIDDropdown(object sender, RoutedEventArgs ev)
        {

            bool shiftIsDown = UIHelper.IsKeyDown(VirtualKey.Shift);
            bool ctrlIsDown = UIHelper.IsKeyDown(VirtualKey.Control);
            bool altIsDown = UIHelper.IsKeyDown(VirtualKey.Menu);

            if (!shiftIsDown && !ctrlIsDown && !altIsDown)
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

            List<IIdName> datalist = helper.GetChildList(null);

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
            if (shiftIsDown)
            {
                if (currObject != null)
                {
                    UserControl dv = dvf.Create(_gs, _window, currObject, datalist, Obj, this);
                }
                return;
            }

            // Go to list
            if (ctrlIsDown)
            {
                UserControl uc = dvf.Create(_gs, _window, datalist, Obj, _parent, this);
                DataView dv = uc as DataView;
                if (dv != null)
                {
                    dv.SetSelectedItem(currObject);
                }

            }

            // Create new item and go to it.
            if (altIsDown)
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
                    UserControl uc = dvf.Create(_gs, _window, newObj, datalist, this, this);
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

        private void OnComboBoxChanged(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null || cb.Name == null)
            {
                return;
            }

            string propName = cb.Name.Replace("Edit", "");

            IIdName selItem = cb.SelectedItem as IIdName;
            if (selItem == null)
            {
                return;
            }

            _reflectionService.SetObjectValue(Obj, propName, selItem.IdKey);

            if (propName.IndexOf("EntityTypeId") >= 0)
            {
                string entityPropName = propName.Replace("EntityTypeId", "EntityId");

                string entityControlName = entityPropName + "Edit";


                Control currentControl = null;
                List<UIElement> elements = _singleGrid.Children.ToList();



                foreach (UIElement element in elements)
                {
                    if (element is Control cont && cont.Name == entityControlName)
                    {
                        currentControl = cont;
                        break;
                    }
                }

                MemberInfo mem = Obj.GetType().GetMember(entityPropName).FirstOrDefault();

                if (currentControl != null && mem != null)
                {

                    Console.WriteLine("EntityTypeId Changed");
                    _singleGrid.Children.Remove(currentControl);

                    Control newControl = AddSingleControl(mem, _singleGrid, (int)currentControl.Width, (int)currentControl.Height,
                        (int)Canvas.GetLeft(currentControl), (int)Canvas.GetTop(currentControl));

                }
            }
        }

        private ComboBox AddDropdownComboBox(Object dropdownList, MemberInfo mem, IUICanvas coll, int xsize, int ysize, int xpos, int ypos)
        {
            if (dropdownList == null || mem == null || coll == null)
            {
                return null;
            }

            ComboBox comboBox = UIHelper.CreateComboBox(coll, "DropDown", xsize, ysize, xpos, ypos);
            comboBox.ItemsSource = dropdownList;
            comboBox.DisplayMemberPath = "Name";
            comboBox.SelectedValuePath = GameDataConstants.IdKey;
            comboBox.Name = mem.Name + "Edit";

            object val = _reflectionService.GetObjectValue(Obj, mem);

            long idVal = 1;

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
                    IIdName item = comboBox.Items[i] as IIdName;
                    if (item != null && item.IdKey.ToString() == val.ToString())
                    {
                        comboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (comboBox.SelectedItem == null)
            {
                IIdName errorItem = new NameValue() { Name = "ErrorItem", IdKey = idVal };
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedItem = comboBox.Items[0];
                }
            }

            comboBox.SelectionChanged += OnComboBoxChanged; 

            return comboBox;
        }

        protected void ShowMultiItemButtonsForList(IEnumerable objects)
        {
            _singleGrid.Children.Clear();
            _singleGrid.Visibility = Visibility.Visible;
            _multiGrid.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            CopyButton.Visibility = Visibility.Collapsed;
            DetailsButton.Visibility = Visibility.Collapsed;

            _numSingleTopButtonsShown = 0;

            colorPanels = new List<MyCanvas>();
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


                UIHelper.CreateButton(_singleGrid, EButtonTypes.Default, objType.Name, txt, SingleItemWidth, SingleItemHeight,
                    x, y, (a, b) => { OnClickSingleObjectButton(obj); });


                _numSingleTopButtonsShown++;

            }
        }

        protected void ShowMultiItemButtonsOnSingleItemView()
        {
            _numSingleTopButtonsShown = 0;
            List<MemberInfo> members = _reflectionService.GetMembers(Obj);

            colorPanels = new List<MyCanvas>();
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

                Button button = UIHelper.CreateButton(_singleGrid, EButtonTypes.Default, mem.Name + "Button", txt, SingleItemWidth, SingleItemHeight,
                    x, y, OnClickSingleTopButton);

                AddEntityDesc(Obj, mem, _singleGrid, SingleItemWidth, SingleItemHeight, x, y);

                _numSingleTopButtonsShown++;

                MyColorF col = EntityUtils.GetObjectValue(Obj, mem) as MyColorF;

                if (col != null)
                {
                    int extraWidth = 3;
                    MyCanvas colorPanel = new MyCanvas()
                    {
                        Width = button.Width + extraWidth * 2,
                        Height = button.Height + extraWidth * 2,
                        Name = mem.Name,
                    };

                    _singleGrid.Add(colorPanel, Canvas.GetLeft(button) - extraWidth, Canvas.GetTop(button) - extraWidth);

                    colorPanel.Name = mem.Name;
                    _singleGrid.Children.Add(colorPanel);
                    colorPanels.Add(colorPanel);
                }
            }


        }

        private void OnClickSingleObjectButton(object childObj)
        {
            SaveChanges();
            UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create(_gs, _window, childObj, Obj, _parent, this);

        }

        private void OnClickSingleTopButton(object sender, RoutedEventArgs e)
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
            UserControl view = ucf.Create(_gs, _window, childObj, Obj, _parent, this);

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


            try
            {
                IEnumerable list = _multiGrid.ItemsSource;

                int count = 0;

                if (list == null)
                {
                    return;
                }

                object firstRow = null;
                foreach (object it in list)
                {
                    firstRow = it;
                    break;
                }

                if (firstRow == null)
                {
                    return;
                }

                IList selected = _multiGrid.SelectedItems;

                if (selected == null || selected.Count < 1)
                {
                    return;
                }

                object row = selected[0];
                object item = row;

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }



        public void CopyItem()
        {

            IList selected = _multiGrid.SelectedItems;

            if (selected == null || selected.Count < 1)
            {
                return;
            }


            object row = selected[0];
            object item = row;
            if (item == null)
            {
                return;
            }

            AddItems(item);
        }

        public void ShowDetails()
        {
            object selected = _multiGrid.SelectedItem;

            if (selected == null)
            {
                return;
            }

            object item = selected;
            if (item == null)
            {
                return;
            }

            UserControlFactory ucf = new UserControlFactory();
            UserControl uc = ucf.Create(_gs, _window, item, this, this, this);
        }

        public int GetSelectedRow(DataGrid dgv)
        {
            if (dgv == null)
            {
                return -1;
            }


            IList rows = dgv.SelectedItems;

            if (rows == null || rows.Count < 1)
            {
                return -1;
            }

            object row = rows[0];

            int index = 0;
            foreach (object item in dgv.ItemsSource)
            {
                object row2 = item;

                if(row2 == row)
                {
                    return index;
                }

                index++;
            }
            return -1;
        }

        public void SetSelectedRow(DataGrid dgv, int rid)
        {
            if (dgv == null)
            {
                return;
            }

            List<object> newItems = new List<object>();

            foreach (object item in dgv.ItemsSource)
            {
                newItems.Add(item);
            }

            if (rid < 0 || rid >= newItems.Count) 
            {
                rid = newItems.Count -1;
            }

            if (rid >= 0)
            {
                dgv.SelectedItem = newItems[rid];
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

                List<UIElement> children = _singleGrid.Children.ToList();


                Control control = null;
                foreach (UIElement element in _singleGrid.Children)
                {
                    if (element is Control currControl)
                    {
                        if (currControl.Name == mem.Name + "Edit")
                        {
                            control = currControl;
                            break;
                        }
                    }
                }

                CheckBox cbox = control as CheckBox;
                if (cbox != null)
                {
                    _reflectionService.SetObjectValue(Obj, mem.Name, cbox.IsChecked);
                }

                TextBox tb = control as TextBox;

                if (tb != null)
                {
                    _reflectionService.SetObjectValue(Obj, mem.Name, tb.Text);

                    continue;
                }


                ComboBox cb = control as ComboBox;

                if (cb != null)
                {
                    object item = cb.SelectedItem;

                    IIdName nv = item as IIdName;
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

            IEnumerable list = _multiGrid.ItemsSource;


            int r = 0;
            foreach (object obj in list)
            {
                DataGridRow row = obj as DataGridRow;

                if (obj == item)
                {

                    _multiGrid.SelectedIndex = r;
                    break;
                }
            }
        }

        private void OnTapGridCell(object o, TappedRoutedEventArgs e)
        {

        }



        private void DataGridError(object sender, Exception e)
        {
            Console.WriteLine("Error: ");
        }

        public void SetMultiGridDataSource(Object list)
        {
            DispatcherQueue.TryEnqueue(async () =>
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
                _multiGrid.ItemsSource = new List<IIdName>();
                _multiGrid.SelectedItem = null;
                _multiGrid.ItemsSource = elist;
                _multiGrid.Visibility = Visibility.Visible;

                _multiGrid.SelectionChanged += MultiGrid_SelectionChanged;
                _multiGrid.CurrentCellChanged += MultiGrid_CellValueChanged;

                Type underlyingType = _reflectionService.GetUnderlyingType(Obj);

                if (underlyingType == null)
                {
                    return;
                }

                _ = Task.Run(() => WaitForGridColumns(underlyingType, iidlist));
            });
        }


        private async Task WaitForGridColumns(Type underlyingType, List<IIdName> iidlist)
        {
            while (_multiGrid.Columns.Count == 0)
            {
                await Task.Delay(1);
            }

            DispatcherQueue.TryEnqueue(async () =>
            {

                for (int j = 0; j < _multiGrid.Columns.Count; j++)
                {
                    DataGridColumn col = _multiGrid.Columns[j];
                }

                List<string> ignoreFields = _gameDataService.GetEditorIgnoreFields();
                List<MemberInfo> members = _reflectionService.GetMembers(underlyingType);

                for (int i = 0; i < members.Count; i++)
                {
                    MemberInfo objMember = members[i];
                    if (_reflectionService.MemberIsPrimitive(objMember))
                    {
                        continue;
                    }

                    for (int j = 0; j < _multiGrid.Columns.Count; j++)
                    {
                        DataGridColumn col = _multiGrid.Columns[j];
                        if (col.Header != null && col.Header.ToString() == objMember.Name)
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
                        DataGridColumn col = _multiGrid.Columns[j];
                        if (col.Header != null && col.Header.ToString() == ignored)
                        {
                            _multiGrid.Columns.Remove(col);
                            break;
                        }
                    }
                }


                // Make dropdowns for enums and other lists of objects.
                // Need to do this weird conversion due to how WinUI requries you
                // to do data binding.
                MemberInfo nameMember = members.FirstOrDefault(x => x.Name == "Name");

                if (nameMember != null)
                {
                    // Add enum dropdowns.
                    for (int i = 0; i < members.Count; i++)
                    {
                        MemberInfo objMember = members[i];

                        Type mtype = _reflectionService.GetMemberType(objMember);

                        if (!mtype.IsEnum)
                        {
                            continue;
                        }

                        DataGridColumn col = _multiGrid.Columns.FirstOrDefault(x => x.Header != null &&
                        x.Header.ToString() == objMember.Name);

                        if (col == null)
                        {
                            continue;
                        }

                        Array values = Enum.GetValues(mtype);

                        List<IIdName> enumIds = new List<IIdName>();

                        foreach (object eobj in values)
                        {
                            enumIds.Add(new NameValue() { IdKey = (int)eobj, Name = Enum.GetName(mtype, eobj) });
                        }

                        AddComboBoxColumn(col, objMember, underlyingType, nameMember, enumIds);

                    }

                    for (int i = 0; i < members.Count; i++)
                    {
                        MemberInfo mem = members[i];

                        List<IIdName> dropdownList = _reflectionService.GetDropdownList(_gs, mem, null);

                        if (dropdownList == null || dropdownList.Count < 1)
                        {
                            continue;
                        }

                        DataGridColumn col = _multiGrid.Columns.FirstOrDefault(x => x.Header != null &&
                        x.Header.ToString() == mem.Name);

                        if (col == null)
                        {
                            continue;
                        }

                        AddComboBoxColumn(col, mem, underlyingType, nameMember, dropdownList);
                    }
                }


                if (!_everShownMultigrid)
                {
                    SetSelectedRow(_multiGrid, 0);
                    _everShownMultigrid = true;
                }

                _multiGrid.Visibility = Visibility.Visible;
            });
        }

        private void AddComboBoxColumn(DataGridColumn col, MemberInfo mem, Type underlyingType, MemberInfo nameMember, List<IIdName> dropdownList)
        {
            long firstKey = dropdownList.FirstOrDefault()?.IdKey ?? 1;

            // This is the dirtiest thing I've done I think.
            // Because the dropdown has to have a property with
            // the same name as the property of the underlying 
            // object we want to modify, convert the 
            // Idkey,Name pairs in the original dropdownList
            // to new objects of the current type that have
            // the correct property name and almost always a 
            // Name property.
            List<object> newDropdown = new List<object>();

            foreach (IIdName nv in dropdownList)
            {
                object newObj = Activator.CreateInstance(underlyingType);

                newDropdown.Add(newObj);

                EntityUtils.SetObjectValue(newObj, nameMember, nv.Name);
                EntityUtils.SetObjectValue(newObj, mem, nv.IdKey);
            }

            Binding binding = new Binding()
            {
                Mode = BindingMode.TwoWay,
                Path = new Microsoft.UI.Xaml.PropertyPath(mem.Name),
                ElementName = mem.Name,
            };

            DataGridComboBoxColumn col2 = new DataGridComboBoxColumn()
            {
                ItemsSource = newDropdown,
                DisplayMemberPath = "Name",
                Header = col.Header,
                Binding = binding,
            };

            _multiGrid.Columns.Remove(col);
            _multiGrid.Columns.Add(col2);

        }

        private void OnTick(object? sender)
        {
        }
    }
}