using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Genrpg.Editor;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Constants;

namespace GameEditor
{
    public partial class MainForm : Form
    {
        private static MainForm _instance = null;
        private EditorGameState gs = null;

        public MainForm()
        {
            gs = new EditorGameState();
            _instance = this;
            int numButtons = 0;

            Button button = null;
            button = new Button();
            button.Text = Game.Prefix;
            button.Location = new Point(10, getTotalHeight(numButtons) - getButtonGap() / 2);
            button.Size = new Size(getButtonWidth(), getButtonHeight());
            numButtons++;
            Controls.Add(button);
            button.Click += OnClickButton;

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

            Form form = UIHelper.ShowBlockingDialog("Loading Data", this);
            CommandForm win = new CommandForm(prefix, this);
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
