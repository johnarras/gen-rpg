using Genrpg.Editor.UI.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor.UI.FormatterHelpers
{
    public class DataGridViewFormatter
    {

        public const int DefaultBG = ButtonFormatter.DefaultBG+30;

        public Color BackColor { get; set; } = Color.Black;
        public Color ForeColor { get; set; } = Color.Black;
        public Color BackTextColor { get; set; } = Color.White;
        public Color ForeTextColor { get; set; } = Color.White;
        public string FamilyName { get; set; } = FormatterConstants.DefaultFontFamily;
        public FontStyle FontStyle { get; set; } = FormatterConstants.DefaultFontStyle;
        Color SelectionForeColor { get; set; } = Color.Black;
        Color SelectionBackColor { get; set; } = Color.White;

        public DataGridViewFormatter()
        {
            BackColor = UIFormatter.CreateGray(DefaultBG);
            ForeColor = Color.White;
            SelectionForeColor = Color.Black;
            SelectionBackColor = UIFormatter.CreateGray(255 - DefaultBG);
        }

        public void Format(DataGridView dataGrid)
        {
            dataGrid.BackgroundColor = BackColor;
            dataGrid.BorderStyle = BorderStyle.None;

            DataGridViewCellStyle mainStyle = new DataGridViewCellStyle();
            mainStyle.BackColor = BackColor;
            mainStyle.ForeColor = ForeColor;
            mainStyle.SelectionBackColor = SelectionBackColor;
            mainStyle.SelectionForeColor = SelectionForeColor;
            mainStyle.Font = new Font(familyName: FormatterConstants.DefaultFontFamily, emSize: FormatterConstants.SmallLabelFontSize);


            DataGridViewCellStyle borderStyle = new DataGridViewCellStyle();
            borderStyle.BackColor = UIFormatter.CreateGray(DefaultBG - 20);
            borderStyle.ForeColor = Color.White;
            borderStyle.SelectionBackColor = Color.Black;
            borderStyle.SelectionForeColor = Color.White;        
            borderStyle.Font = new Font(familyName: FormatterConstants.DefaultFontFamily, emSize: FormatterConstants.SmallLabelFontSize);


            dataGrid.EnableHeadersVisualStyles = false;
            for (int c = 0; c < dataGrid.Columns.Count; c++)
            {
                dataGrid.Columns[c].HeaderCell.Style = borderStyle;
            }

            for (int r = 0; r < dataGrid.Rows.Count; r++)
            {
                dataGrid.Rows[r].HeaderCell.Style = borderStyle;
            }

            for (int r = 0; r < dataGrid.Rows.Count; r++)
            {
                for (int c = 0; c < dataGrid.Columns.Count; c++)
                {
                    DataGridViewCell cell = dataGrid.Rows[r].Cells[c];
                    
                    cell.Style = mainStyle;
                }
            }
        }
    }
}
