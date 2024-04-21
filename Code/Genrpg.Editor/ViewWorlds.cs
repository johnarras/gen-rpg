using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Editor.Entities.Core;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.GameSettings;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;

namespace GameEditor
{
    public partial class ViewMaps : Form
	{
		public EditorGameState _gs { get; set; }
		private DataGridView _grid = null;
		private IGameData _gameData = null;
        private List<MapStub> _stubs = new List<MapStub>();
        private UIFormatter _formatter = null;

		public ViewMaps(EditorGameState gsIn, UIFormatter formatter)
		{
            _formatter = formatter;
            _formatter.SetupForm(this, EFormTypes.Default);
			_gs = gsIn;

			SetupForm();

			

			InitializeComponent();
		}

        public const int XSize = 1800;
        public const int YSize = 800;
		public void SetupForm()
		{
            int width = XSize;
            int height = YSize;
			Size = new Size(width, height);

            _gameData = _gs.data;
			if (_gameData == null)
            {
                return;
            }

            AddTopButtons();

            ShowStubList();
		}

		private bool addedButtons = false;
		private void AddTopButtons()
		{
			if (addedButtons)
            {
                return;
            }

            addedButtons = true;

            UIHelper.CreateButton(Controls, EButtonTypes.Default,
                _formatter, "DetailsButton", "Details", 110, 30, 20, 30, OnClickDetails);
		}

        public void ShowStubList()
        {
            if (_grid != null && Controls.Contains(_grid))
            {
                Controls.Remove(_grid);
            }

            _grid = null;
            if (_gameData == null)
            {
                return;
            }

            _ = Task.Run(() => LoadMapStubs());
        }

        private async Task LoadMapStubs()
        {
            IMapDataService mapDataSerice = _gs.loc.Get<IMapDataService>();

            _stubs = await mapDataSerice.GetMapStubs(_gs);

            this.Invoke(FinalSetupGrid);
        }

        private void FinalSetupGrid()
        { 

            Panel SingleGrid = new Panel();
            SingleGrid.AutoScroll = true;
            SingleGrid.Location = new Point(0, DataView.SingleItemTopPad);
            SingleGrid.Size = new Size(XSize-50,YSize-20);
            Controls.Add(SingleGrid);

            _grid = UIHelper.CreateDataGridView(Controls, _formatter, "WorldsGrid", XSize - 100, YSize - 100, 20, 100);


            _grid.DataSource = _stubs;
			_grid.BindingContext = new BindingContext();
			CurrencyManager cm = _grid.BindingContext[_grid.DataSource] as CurrencyManager;
			if (cm != null)
			{
				cm.Refresh();
			}
			_grid.Refresh();
            _formatter.SetupDataGrid(_grid);

		}

        public void OnClickDetails(object sender, EventArgs e)
        {
            MapStub item = GetSelectedItem(_grid);
            if (item == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Loading map", _formatter);
            _ = Task.Run(() => OnClickDetailsAsync(item, form));
        }

        private async Task OnClickDetailsAsync(MapStub item, Form form)
        {
            IMapDataService mds = _gs.loc.Get<IMapDataService>();
            Map map = await mds.LoadMap(_gs, item.Id);

            this.Invoke(()=> AfterLoadMap(map, form));
        }

        private void AfterLoadMap(Map map, Form form)
        { 
			if (form != null)
            {
                form.Hide();
            }

            _gs.map = map;
			DataWindow dw = new DataWindow(_gs, _formatter, map, this, "Map");
			dw.Show();
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

		public MapStub GetSelectedItem(DataGridView dgv)
		{
			if (dgv == null || dgv.Rows == null)
            {
                return null;
            }

            int rowId = GetSelectedRow(dgv);
			if (rowId < 0 || rowId >= dgv.Rows.Count)
            {
                return null;
            }

            DataGridViewRow row = dgv.Rows[rowId];
			if (row == null)
            {
                return null;
            }

            return row.DataBoundItem as MapStub;
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

	}
}