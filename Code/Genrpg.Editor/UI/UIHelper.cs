using Genrpg.Editor.UI.Constants;
using System;
using System.Drawing;
using System.Windows.Forms;
using ZstdSharp.Unsafe;
using static System.Windows.Forms.Control;

namespace Genrpg.Editor.UI
{
    public static class UIHelper
    {
        public static Form ShowBlockingDialog(string text, UIFormatter formatter, Form owner = null, int width = 0, int height = 0)
        {
            Form f = new Form();
            f.Owner = owner;

            if (width == 0)
            {
                width = 100;
            }

            if (height == 0)
            {
                height = 80;
            }

            CreateLabel(f.Controls, ELabelTypes.Default, formatter, "DialogText", text, width - 20, height - 40, 0, 0, 20);

            formatter.SetupForm(f, EFormTypes.Blocking);
            f.Show();
            f.Refresh();
            return f;
        }


        public static Button CreateButton(ControlCollection controls, EButtonTypes buttonType, UIFormatter formatter, 
            string name, string text, int width, int height, int xpos, int ypos, EventHandler clickAction)
        {
            Button button = new Button();
            button.Size = new Size(width, height);
            button.Location = new Point(xpos, ypos);
            button.Click += clickAction;
            button.Name = name;
            button.Text = text;
            controls.Add(button);
            formatter.SetupButton(button, buttonType);
            return button;
        }

        public static Label CreateLabel(ControlCollection controls, ELabelTypes labelType, UIFormatter formatter,
            string name, string text, int width, int height, int xpos, int ypos, float fontSize=FormatterConstants.DefaultLabelFontSize, ContentAlignment alignment = ContentAlignment.MiddleCenter)       
        {
            Label label = new Label();
            label.Name = name;
            label.Text = text;
            label.Size = new Size(width, height);
            label.Location = new Point(xpos, ypos);
            label.TextAlign = alignment;
            controls.Add(label);

            formatter.SetupLabel(label, labelType, fontSize);

            return label;
        }

        public static DataGridView CreateDataGridView(ControlCollection controls, UIFormatter formatter,
            string name, int width, int height, int xpos, int ypos)
        {
            DataGridView dataGridView = new DataGridView();
            dataGridView.Name = name;
            dataGridView.Size = new Size(width, height);
            dataGridView.Location = new Point(xpos, ypos);
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            formatter.SetupDataGrid(dataGridView);
            controls.Add(dataGridView);

            return dataGridView;
        }

        public static ComboBox CreateComboBox(ControlCollection controls, UIFormatter formatter, string name,
            int width, int height, int xpos, int ypos)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Name = name;
            comboBox.Size = new Size(width, height);
            comboBox.Location = new Point(xpos, ypos); 
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            controls.Add(comboBox);
            formatter.SetupComboBox(comboBox);
            return comboBox;
        }


        public static TextBox CreateTextBox(ControlCollection controls, UIFormatter formatter, string name, string initialText,
            int width, int height, int xpos, int ypos, EventHandler eventHandler)
        {
            TextBox textBox = new TextBox();
            textBox.Name = name;
            textBox.Text = initialText;
            textBox.Size = new Size(width, height);
            textBox.Location = new Point(xpos, ypos);
            if (eventHandler != null)
            {
                textBox.TextChanged += eventHandler;
            }
            controls.Add(textBox);
            formatter.SetupTextBox(textBox);
            return textBox;
        }

        public static CheckBox CreateCheckBox(ControlCollection controls, UIFormatter formatter, string name,
            int width, int height, int xpos, int ypos)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Name = name;
            checkBox.Size = new Size(width, height);
            checkBox.Location = new Point(xpos, ypos);
            controls.Add(checkBox);
            formatter.SetupCheckBox(checkBox);
            return checkBox;
        }
    }
}
