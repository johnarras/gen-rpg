using Amazon.Runtime.Internal.Util;
using Genrpg.Editor.UI.Constants;
using Genrpg.Editor.UI.FormatterHelpers;
using Genrpg.Editor.UI.Formatters;
using Genrpg.Shared.Utils;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor.UI
{
    public class UIFormatter
    {
        public const int ButtonDarkGray = 60;

        public FormFormatter DefaultFormFormatter { get; set; } = new FormFormatter();
        public FormFormatter BlockingFormFormatter { get; set; } = new FormFormatter();
        private Dictionary<EFormTypes, FormFormatter> _formFormatters = new Dictionary<EFormTypes, FormFormatter>();


        public ButtonFormatter DefaultButtonFormatter { get; set; } = new ButtonFormatter();
        public ButtonFormatter TopBarButtonFormatter { get; set; } = new ButtonFormatter();
        private Dictionary<EButtonTypes, ButtonFormatter> _buttonFormatters = new Dictionary<EButtonTypes, ButtonFormatter>();

        public LabelFormatter DefaultLabelFormatter { get; set; } = new LabelFormatter();
        private Dictionary<ELabelTypes, LabelFormatter> _labelFormatters = new Dictionary<ELabelTypes, LabelFormatter>();

        public DataGridViewFormatter DefaultDataGridFormatter { get; set; } = new DataGridViewFormatter();

        public ComboBoxFormatter DefaultComboBoxFormatter { get; set; } = new ComboBoxFormatter();

        public TextBoxFormatter DefaultTextBoxFormatter { get; set; } = new TextBoxFormatter();

        public CheckBoxFormatter DefaultCheckBoxFormatter { get; set; } = new CheckBoxFormatter();

        public UIFormatter()
        {

            BlockingFormFormatter.BackColor = Color.DarkRed;
            BlockingFormFormatter.ForeColor = Color.Yellow;

            int topBarOffsetColor = 10;
            TopBarButtonFormatter.BackColor = CreateGray(ButtonFormatter.DefaultBG + topBarOffsetColor);
            TopBarButtonFormatter.ForeColor = CreateGray(ButtonFormatter.DefaultFG + topBarOffsetColor);

            SetupIndexes();
        }       
        public void SetupIndexes()
        {
            _formFormatters = new Dictionary<EFormTypes, FormFormatter>();
            _formFormatters[EFormTypes.Default] = DefaultFormFormatter;
            _formFormatters[EFormTypes.Blocking] = BlockingFormFormatter;

            _buttonFormatters = new Dictionary<EButtonTypes, ButtonFormatter>();
            _buttonFormatters[EButtonTypes.Default] = DefaultButtonFormatter;
            _buttonFormatters[EButtonTypes.TopBar] = TopBarButtonFormatter;

            _labelFormatters = new Dictionary<ELabelTypes, LabelFormatter>();
            _labelFormatters[ELabelTypes.Default] = DefaultLabelFormatter;
        }

        public static Color CreateGray(int grayVal)
        {
            return CreateColor(grayVal, grayVal, grayVal);
        }

        public static Color CreateColor(int r, int g, int b)
        {
            return CreateTransparentColor(r, g, b, 255);
        }

        public static Color CreateTransparentColor(int r, int g, int b, int a)
        {
            r = MathUtils.Clamp(0, r, 255);
            g = MathUtils.Clamp(0, g, 255);
            b = MathUtils.Clamp(0, b, 255);
            a = MathUtils.Clamp(0, a, 255);
            return Color.FromArgb(a, r, g, b);
        }

        public void SetupForm(Form form, EFormTypes formType)
        {
            if (_formFormatters.TryGetValue(formType, out FormFormatter formatter))
            {
                formatter.Format(form);
            }
        }

        public void SetupButton(Button button, EButtonTypes buttonType)
        {
            if (_buttonFormatters.TryGetValue(buttonType, out ButtonFormatter formatter))
            {
                formatter.Format(button);
            }
        }

        public void SetupLabel(Label label, ELabelTypes labelType, float fontSize)
        {
            if (_labelFormatters.TryGetValue(labelType, out LabelFormatter formatter))
            {
                formatter.Format(label, fontSize);
            }
        }

        public void SetupDataGrid(DataGridView dataGrid)
        {
            DefaultDataGridFormatter.Format(dataGrid);

        }

        public void SetupComboBox(ComboBox comboBox)
        {
            DefaultComboBoxFormatter.Format(comboBox);
        }

        public void SetupTextBox(TextBox textBox)
        {
            DefaultTextBoxFormatter.Format(textBox);
        }

        public void SetupCheckBox(CheckBox checkBox)
        {
            DefaultCheckBoxFormatter.Format(checkBox);
        }
    }
}
