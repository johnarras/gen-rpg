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

namespace GameEditor
{
    public partial class FindUserView : UserControl
    {
        private EditorGameState gs = null;
        private DataWindow win = null;
        private TextBox queryInput = null;
        private ComboBox queryType = null;
        private DataGridView Grid = null;
        private AccountService _accountService = null;
        public FindUserView (EditorGameState gsIn, DataWindow winIn)
        {
            gs = gsIn;
            win = winIn;
            if (win != null)
            {
                Size = win.Size;
                win.Controls.Clear();
                win.Controls.Add(this);
                win.ViewStack.Add(this);
            }
            _accountService = new AccountService();
            ShowComponents();
            InitializeComponent();
        }

        public void ShowComponents()
        {
            int x = 50;
            Size sz = new Size(150, 30);
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

            
            queryType = new ComboBox();
            queryType.Size = sz;
            queryType.Items.AddRange(words);
            if (words != null && words.Length > 0)
            {
                queryType.SelectedItem = "DatabaseId";
                queryType.SelectedItem = "Id";
            }
            queryType.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(queryType);
            queryType.Location = new Point(x, 20);
            queryInput = new TextBox();
            queryInput.Location = new Point(x, 60);
            queryInput.Size = sz;
            Controls.Add(queryInput);

            Button button = new Button();
            button.Text = "Search";
            button.Click += OnClickSearch;
            button.Location = new Point(x, 100);
            button.Size = sz;
            Controls.Add(button);
            if (win != null)
            {
                win.AcceptButton = button;
            }

            button = new Button();
            button.Text = "Clear";
            button.Click += OnClickClear;
            button.Location = new Point(x+(sz.Width+5), 100);
            button.Size = sz;
            Controls.Add(button);

            button = new Button();
            button.Text = "Details";
            button.Click += OnClickDetails;
            button.Location = new Point(x + (sz.Width+5)*2, 100);
            button.Size = sz;
            Controls.Add(button);

            button = new Button();
            button.Text = "Delete";
            button.Click += OnClickDelete;
            button.Location = new Point(x + (sz.Width + 5) * 6, 100);
            button.Size = sz;
            Controls.Add(button);

            Grid = new DataGridView();
            Grid.Location = new Point(0, 140);
            Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Controls.Add(Grid);
            if (win != null)
            {
                Grid.Size = new Size(win.Width-17, win.Height - 180);
            }
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

            if (gs == null || gs.loc == null)
            {
                return;
            }

            Form form = UIHelper.ShowBlockingDialog("Loading user data", win);
            Task.Run(() => EditorPlayerUtils.LoadEditorUserData(gs, acct.Id)).GetAwaiter().GetResult();
            form.Hide();
            if (gs.EditorUser.User == null)
            {
                MessageBox.Show("User not found");
                return;
            }

			UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create (gs, win, gs.EditorUser, null, null);


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

            Form form = UIHelper.ShowBlockingDialog("Loading user data", win);
            Task.Run(() => EditorPlayerUtils.LoadEditorUserData(gs, acct.Id)).GetAwaiter().GetResult();
            form.Hide();
            form = UIHelper.ShowBlockingDialog("Deleting user data", win);

            // We don't delete the account here.
            Task.Run(() => EditorPlayerUtils.DeleteEditorUserData(gs)).GetAwaiter().GetResult();
            form.Hide();

        }

        private void OnClickSearch(object sender, EventArgs e)
        {
            string val = queryInput.Text;
            Object item = queryType.SelectedItem;
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
            acct = await _accountService.LoadBy(gs.config, key, val);
            List<Account> list = new List<Account>();
            if (acct != null)
            {
                list.Add(acct);
            }
            Invoke(new Action(() =>
            {
                Grid.DataSource = list;
                Grid.Refresh();
            }));  
        }


        public void Save()
        {
            if (win != null)
            {

                _ = Task.Run(() => win.SaveData());
            }
        }

    }
}
