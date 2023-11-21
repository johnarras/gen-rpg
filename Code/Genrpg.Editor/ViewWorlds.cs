using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Utils;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.GameSettings;

namespace GameEditor
{
    public partial class ViewMaps : Form
	{
		public EditorGameState gs { get; set; }

		private DataGridView Grid = null;
		private GameData Data = null;


		private Button DetailsButton = null;

        private List<MapStub> _stubs = new List<MapStub>();

		public ViewMaps(EditorGameState gsIn)
		{
			gs = gsIn;

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

            Data = gs.data;
			if (Data == null)
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

			Button but = null;


			int y = 30;
			int x = 20;
			Size sz = new Size(110, 30);

			but = new Button();
			but.Location = new Point(x, y);
			but.Size = sz;
			x += sz.Width + 5;
			but.Name = "DetailsButton";
			but.Text = "Details";
			but.Click += OnClickDetails;
			Controls.Add(but);
			DetailsButton = but;

		}

        public void ShowStubList()
        {
            if (Grid != null && Controls.Contains(Grid))
            {
                Controls.Remove(Grid);
            }

            Grid = null;
            if (Data == null)
            {
                return;
            }

            _ = Task.Run(() => LoadMapStubs());
        }

        private async Task LoadMapStubs()
        {
            IMapDataService mapDataSerice = gs.loc.Get<IMapDataService>();

            _stubs = await mapDataSerice.GetMapStubs(gs);

            this.Invoke(FinalSetupGrid);
        }

        private void FinalSetupGrid()
        { 

            Panel SingleGrid = new Panel();
            SingleGrid.AutoScroll = true;
            SingleGrid.Location = new Point(0, DataView.SingleItemTopPad);
            SingleGrid.Size = new Size(XSize-50,YSize-20);
            Controls.Add(SingleGrid);
            Grid = new DataGridView();
			int sx = 20;
			int sy = 100;
			Grid.Location = new Point(sx, sy);

			Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			Grid.Size = new Size(XSize-100,YSize-100);
            SingleGrid.Controls.Add(Grid);


            Grid.DataSource = _stubs;
			Grid.BindingContext = new BindingContext();
			CurrencyManager cm = Grid.BindingContext[Grid.DataSource] as CurrencyManager;
			if (cm != null)
			{
				cm.Refresh();
			}
			Grid.Refresh();


		}

        public void OnClickDetails(object sender, EventArgs e)
        {
            MapStub item = GetSelectedItem(Grid);
            if (item == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Loading map");
            _ = Task.Run(() => OnClickDetailsAsync(item, form));
        }

        private async Task OnClickDetailsAsync(MapStub item, Form form)
        {
            IMapDataService mds = gs.loc.Get<IMapDataService>();
            Map map = await mds.LoadMap(gs, item.Id);

            this.Invoke(()=> AfterLoadMap(map, form));
        }

        private void AfterLoadMap(Map map, Form form)
        { 
			if (form != null)
            {
                form.Hide();
            }

            gs.map = map;
			DataWindow dw = new DataWindow(gs, map, this, "Map");
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