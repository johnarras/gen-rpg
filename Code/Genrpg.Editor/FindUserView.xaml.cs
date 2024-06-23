using System;
using System.Threading.Tasks;

using System.Reflection;
using Genrpg.Shared.Accounts.Entities;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor;
using Genrpg.Editor.Utils;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.ServerShared.Config;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.Generic;

namespace Genrpg.Editor
{
    public partial class FindUserView : UserControl, IUICanvas
    {
        private IAccountService _accountService = null;
        private IRepositoryService _repoService = null;
        private IServerConfig _config = null;
        private EditorGameState _gs = null;
        private DataWindow _win = null;
        private TextBox _queryInput = null;
        private ComboBox _queryType = null;
        private DataGrid Grid = null;

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

        public FindUserView(EditorGameState gsIn, DataWindow winIn)
        {
            Content = _canvas;
            _gs = gsIn;
            _gs.loc.Resolve(this);
            _win = winIn;
            if (_win != null)
            {
                Width = _win.Width;
                Height = _win.Height;
                _win.AddChildView(this);
                _win.ViewStack.Add(this);
            }
            ShowComponents();        
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


            _queryType = UIHelper.CreateComboBox(this, "SaerchType", width, height, x, 20);

            _queryType.ItemsSource = words;
            if (words != null && words.Length > 0)
            {
                _queryType.SelectedItem = "Id";
            }


            _queryInput = UIHelper.CreateTextBox(this, "Query", null, width, height, 0, 60, null);

            int currX = x;
            UIHelper.CreateButton(this, EButtonTypes.TopBar, "SearchButton", "Search", width, height, x, ypos, OnClickSearch); currX += width + 5;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "ClearButton", "Clear", width, height, x + width + 5, ypos, OnClickClear); currX += width + 5;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "DetailsButton", "Details", width, height, currX, ypos, OnClickDetails); currX += (width + 5) * 3;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "DeleteButton", "Delete", width, height, currX, ypos, OnClickDelete);

            Grid = UIHelper.CreateDataGridView(this, "UserGrid", _win.Width - 17, _win.Height - 180, 0, 140);
        }
        private void OnClickClear(object sender, RoutedEventArgs e)
        {
            Grid.ItemsSource = null;
        }

        private void OnClickDetails(object sender, RoutedEventArgs e)
        {
            object row = Grid.SelectedItem;

            Account acct = row as Account;
            if (acct == null)
            {
                return;
            }

            if (_gs == null || _gs.loc == null)
            {
                return;
            }

            SmallPopup form = UIHelper.ShowBlockingDialog(_win, "Loading user data");
            Task.Run(() => EditorPlayerUtils.LoadEditorUserData(_gs, _repoService, acct.Id)).GetAwaiter().GetResult();
            form.StartClose();
            if (_gs.EditorUser.User == null)
            {
                UIHelper.ShowMessageBox(_win, "User Not Found").Wait();
                return;
            }

            UserControlFactory ucf = new UserControlFactory();
            UserControl view = ucf.Create(_gs, _win, _gs.EditorUser, null, null, null);


        }

        private void OnClickDelete(object sender, RoutedEventArgs e)
        {
            System.Collections.IList rows = Grid.SelectedItems;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            Account acct = rows[0] as Account;
            if (acct == null)
            {
                return;
            }

            SmallPopup form = UIHelper.ShowBlockingDialog(_win, "Loading user data");
            Task.Run(() => EditorPlayerUtils.LoadEditorUserData(_gs, _repoService, acct.Id)).GetAwaiter().GetResult();

            form.StartClose();
            form = UIHelper.ShowBlockingDialog(_win, "Deleting user data");

            // We don't delete the account here.
            Task.Run(() => EditorPlayerUtils.DeleteEditorUserData(_gs, _repoService)).GetAwaiter().GetResult();
            form.StartClose();

        }

        private void OnClickSearch(object sender, RoutedEventArgs e)
        {
            string val = _queryInput.Text;
            Object item = _queryType.SelectedItem;
            _ = Task.Run(() => OnClickSearchAsync(val, item));
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
            acct = await _accountService.LoadBy(_config, key, val);
            List<Account> list = new List<Account>();
            if (acct != null)
            {
                list.Add(acct);
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                Grid.ItemsSource = list;
            });
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
