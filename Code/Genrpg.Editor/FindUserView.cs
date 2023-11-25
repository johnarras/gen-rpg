using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


using System.Reflection;
using Genrpg.Shared.Accounts.Entities;
using Genrpg.ServerShared.Accounts;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Editor.Utils;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;

namespace GameEditor
{
    public partial class FindUserView : UserControl
    {
        private IAccountService _accountService = null;
        private EditorGameState _gs = null;
        private DataWindow _win = null;
        private TextBox _queryInput = null;
        private ComboBox _queryType = null;
        private DataGridView Grid = null;
        private UIFormatter _formatter = null;
        public FindUserView (EditorGameState gsIn, UIFormatter formatter, DataWindow winIn)
        {
            _formatter = formatter;
            _gs = gsIn;
            _gs.loc.Resolve(this);
            _win = winIn;
            if (_win != null)
            {
                Size = _win.Size;
                _win.Controls.Clear();
                _win.Controls.Add(this);
                _win.ViewStack.Add(this);
            }
            ShowComponents();
            InitializeComponent();
        }

        public void ShowComponents()
        {
            int x = 50;
            int width = 150;
            int height = 30;
            int ypos = 100;
            PropertyInfo[] props = typeof(Account).GetProperties();

            List<string> wordlist = new List<string>();

            foreach (PropertyInfo prop in props)
            {
                if (prop.Name.IndexOf("Password") >= 0)
                {
                    continue;
                }

                wordlist.Add(prop.Name);
            }

            string[] words = wordlist.ToArray();

            
            _queryType = UIHelper.CreateComboBox(Controls, _formatter, "SaerchType", width, height, x, 20);

            _queryType.Items.AddRange(words);
            if (words != null && words.Length > 0)
            {
                _queryType.SelectedItem = "Id";
            }


            _queryInput = UIHelper.CreateTextBox(Controls, _formatter, "Query", null, width, height, 0, 60, null);
                
            int currX = x;
            UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "SearchButton", "Search", width, height, x, ypos, OnClickSearch); currX += width + 5;

            UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "ClearButton", "Clear", width, height, x + width + 5, ypos, OnClickClear); currX += width + 5;

            UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "DetailsButton", "Details", width, height, currX, ypos, OnClickDetails); currX += (width + 5) * 3;

            UIHelper.CreateButton(Controls, EButtonTypes.TopBar, _formatter, "DeleteButton", "Delete", width, height, currX, ypos, OnClickDelete);

            Grid = UIHelper.CreateDataGridView(Controls, _formatter, "UserGrid", _win.Width - 17, _win.Height - 180, 0, 140);
        }
        private void OnClickClear(object sender, EventArgs e)
        {
            Grid.DataSource = null;
        }

        private void OnClickDetails(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection rows = Grid.SelectedRows;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            Account acct = row.DataBoundItem as Account;
            if (acct == null)
            {
                return;
            }

            if (_gs == null || _gs.loc == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Loading user data", _formatter, _win);
            Task.Run(() => EditorPlayerUtils.LoadEditorUserData(_gs, acct.Id)).GetAwaiter().GetResult();
            form.Hide();
            if (_gs.EditorUser.User == null)
            {
                MessageBox.Show("User not found");
                return;
            }

			UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create (_gs, _formatter, _win, _gs.EditorUser, null, null, null);


        }

        private void OnClickDelete(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection rows = Grid.SelectedRows;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            DataGridViewRow row = rows[0];
            Account acct = row.DataBoundItem as Account;
            if (acct == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Loading user data", _formatter, _win);
            Task.Run(() => EditorPlayerUtils.LoadEditorUserData(_gs, acct.Id)).GetAwaiter().GetResult();
            form.Hide();
            form = UIHelper.ShowBlockingDialog("Deleting user data", _formatter, _win);

            // We don't delete the account here.
            Task.Run(() => EditorPlayerUtils.DeleteEditorUserData(_gs)).GetAwaiter().GetResult();
            form.Hide();

        }

        private void OnClickSearch(object sender, EventArgs e)
        {
            string val = _queryInput.Text;
            Object item = _queryType.SelectedItem;
            _ = Task.Run(() => OnClickSearchAsync(val,item));
        }

        private async Task OnClickSearchAsync(string val, object item)
        {
            if (item == null)
            {
                return;
            }

            string key = item.ToString();
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(val))
            {
                return;
            }

            Account acct = null;
            acct = await _accountService.LoadBy(_gs.config, key, val);
            List<Account> list = new List<Account>();
            if (acct != null)
            {
                list.Add(acct);
            }
            Invoke(new Action(() =>
            {
                Grid.DataSource = list;
                Grid.Refresh();
                _formatter.SetupDataGrid(Grid);
            }));  
        }


        public void Save()
        {
            if (_win != null)
            {

                _ = Task.Run(() => _win.SaveData());
            }
        }

    }
}
