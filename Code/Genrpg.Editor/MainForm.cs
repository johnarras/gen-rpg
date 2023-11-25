using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Genrpg.Editor;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;
using Genrpg.Shared.Constants;

namespace GameEditor
{
    public partial class MainForm : Form
    {
        private static MainForm _instance = null;

        private UIFormatter _formatter = null;
        public MainForm()
        {
            _instance = this;
            int numButtons = 0;

            _formatter = new UIFormatter();
            _formatter.SetupForm(this, EFormTypes.Default);

            UIHelper.CreateButton(Controls, EButtonTypes.Default, _formatter, Game.Prefix + "Button",
                Game.Prefix, getButtonWidth(), getButtonHeight(), 10, getTotalHeight(numButtons) - getButtonGap() / 2, OnClickButton);
            numButtons++;

            Size = new Size(getButtonWidth() + getButtonGap() * 4, getTotalHeight(numButtons) + getTopBottomPadding() * 4);

            InitializeComponent();
        }

        private int getButtonWidth() { return 150; }

        private int getButtonHeight() { return 40; }

        private int getLeftRightPadding() { return 20; }

        private int getTopBottomPadding() { return 10; }

        private int getButtonGap() { return 8; }

        private int getTotalHeight(int numButtons)
        {
            return (getButtonHeight()+getButtonGap())*numButtons + getTopBottomPadding();
        }

        private void OnClickButton(object sender, EventArgs e)
        {
            Button but = sender as Button;
            String prefix = but.Text;
            if (String.IsNullOrEmpty(prefix))
            {
                return;
            }

            CloseAllForms(true);

            Form form = UIHelper.ShowBlockingDialog("Loading Data", _formatter, this);
            CommandForm win = new CommandForm(prefix, _formatter, this);
            win.Show();
            if (form != null)
            {
                form.Hide();
            }
        }


        public void CloseAllForms(bool exceptMainForm = true)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                Form f = Application.OpenForms[i];
                if (exceptMainForm && f == _instance)
                {
                    continue;
                }

                DataWindow win = f as DataWindow;
                if (win != null)
                {

                    _ = Task.Run(() => win.SaveData());
                }
                f.Close();
                i--;
            }
        }

        protected override void OnFormClosing (FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            CloseAllForms();
            Application.Exit();
        }

    }
}
