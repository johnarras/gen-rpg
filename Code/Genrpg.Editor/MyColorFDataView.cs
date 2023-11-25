using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Entities;

using Genrpg.Shared.Entities.Utils;
using Genrpg.Editor.Entities.Core;
using GameEditor;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Constants;

namespace Genrpg.Editor
{
    public class MyColorFDataView : DataView
    {
        const int ColorScaleBitmapHeight = 18;
        const byte ClearColor = 240;

        MyColorF color = null;

        const int R = 0;
        const int G = 1;
        const int B = 2;
        const int A = 3;
        const int NumChannels = 4;
        string[] channels = new string[NumChannels] { "Red", "Green", "Blue", "Alpha" };

        Label[] names = new Label[NumChannels];
        TrackBar[] bars = new TrackBar[NumChannels];
        TextBox[] inputs = new TextBox[NumChannels];

        int[] vals = new int[NumChannels];

        Panel colorPanel = null;

        PictureBox[] pics = new PictureBox[NumChannels];


        Timer timer = null;

        public MyColorFDataView(EditorGameState gsIn, UIFormatter formatter, DataWindow winIn, object objIn, object parentIn, object grandparentIn, DataView parentView) :
            base(gsIn, formatter, winIn, objIn, parentIn, grandparentIn, parentView)
        {
        }
        public override void ShowData()
        {
            if (_multiGrid == null || _singleGrid == null)
            {
                return;
            }

            _singleGrid.Controls.Clear();
            _singleGrid.Visible = true;
            _multiGrid.Visible = false;
            AddButton.Visible = false;
            DeleteButton.Visible = false;
            CopyButton.Visible = false;
            DetailsButton.Visible = false;


            color = Obj as MyColorF;
            if (color == null)
            {
                return;
            }
            vals[R] = MathUtils.Clamp(0, (int)(255 * color.R), 255);
            vals[G] = MathUtils.Clamp(0, (int)(255 * color.G), 255);
            vals[B] = MathUtils.Clamp(0, (int)(255 * color.B), 255);
            vals[A] = MathUtils.Clamp(0, (int)(255 * color.A), 255);


            int sx = 100;
            int sy = 300;
            int dy = 75;

            for (int c = 0; c < NumChannels; c++)
            {
                string cn = channels[c];

                UIHelper.CreateLabel(_singleGrid.Controls, ELabelTypes.Default, _formatter, cn + "Text", cn, 50, 30, sx, sy);

                int cx = 180;
                int tbwid = 280;
                int pbwid = 256;
                PictureBox pb = new PictureBox();
                pics[c] = pb;
                pb.Image = new Bitmap(pbwid, ColorScaleBitmapHeight);
                pb.Name = cn + "Colors";
                pb.Size = new Size(pbwid, ColorScaleBitmapHeight);
                pb.Location = new Point(cx, sy + 20);
                _singleGrid.Controls.Add(pb);

                TrackBar tb = new TrackBar();
                bars[c] = tb;
                tb.BackColor = Color.Black;
                tb.Name = cn + "Bar";
                tb.TickFrequency = 15;
                tb.Size = new Size(tbwid, 40);
                tb.Location = new Point(cx - (tbwid - pbwid) / 2, sy);
                tb.SetRange(0, 255);
                tb.Value = vals[c];
                tb.ValueChanged += OnTrackbarChanged;
                _singleGrid.Controls.Add(tb);

                inputs[c] = UIHelper.CreateTextBox(_singleGrid.Controls, _formatter, cn + "Input", vals[c].ToString(), 50, 30, sx + 400, sy, OnTextChanged);

                sy += dy;
            }
            colorPanel = new Panel();
            colorPanel.Size = new Size(150, 150);
            colorPanel.Location = new Point(230, 120);
            _singleGrid.Controls.Add(colorPanel);
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
            null, colorPanel, new object[] { true });


            timer = new Timer();
            timer.Tick += OnTick;
            timer.Interval = 75;
            timer.Start();


            RefreshScreen();
        }

        Color clearColor = Color.FromArgb(255, ClearColor, ClearColor, ClearColor);
        bool refreshing = false;
        Graphics graphics = null;
        bool drawSquare = true;
        private void RefreshScreen()
        {

            if (!refreshing)
            {
                refreshing = true;
                if (color != null && vals != null && vals.Length >= NumChannels)
                {
                    color.R = vals[R] / 255.0f;
                    color.G = vals[G] / 255.0f;
                    color.B = vals[B] / 255.0f;
                    color.A = vals[A] / 255.0f;
                }

                for (int c = 0; c < NumChannels; c++)
                {
                    bars[c].Value = vals[c];
                    inputs[c].Text = vals[c].ToString();
                }
                refreshing = false;
            }
            drawSquare = true;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!drawSquare)
            {
                return;
            }

            drawSquare = false;

            if (colorPanel != null)
            {
                if (graphics == null)
                {
                    graphics = colorPanel.CreateGraphics();
                }


                Rectangle rect = new Rectangle(0, 0, colorPanel.Size.Width, colorPanel.Size.Height);
                Color col = Color.FromArgb(vals[A], vals[R], vals[G], vals[B]);
                graphics.Clear(clearColor);
                graphics.FillRectangle(new SolidBrush(col), rect);
            }

            for (int c = 0; c < NumChannels; c++)
            {
                PictureBox pb = pics[c];
                Graphics graphics = Graphics.FromImage(pb.Image);
                graphics.Clear(clearColor);
                for (int x = 0; x < 256; x++)
                {
                    int r = vals[R];
                    int g = vals[G];
                    int b = vals[B];
                    int a = vals[A];
                    if (c == R)
                    {
                        r = x;
                    }
                    else if (c == G)
                    {
                        g = x;
                    }
                    else if (c == B)
                    {
                        b = x;
                    }
                    else if (c == A)
                    {
                        a = x;
                    }
                    Color col = Color.FromArgb(a, r, g, b);
                    Rectangle rect = new Rectangle(x, 0, 1, ColorScaleBitmapHeight);
                    graphics.FillRectangle(new SolidBrush(col), rect);

                }
                pb.Image = pb.Image;
            }

        }

        private void OnTextChanged(object sender, EventArgs e)
        {

            UpdateFromSender(sender);
        }


        private void OnTrackbarChanged(object sender, EventArgs e)
        {
            UpdateFromSender(sender);
        }


        private void UpdateFromSender(object sender)
        {
            if (sender == null)
            {
                return;
            }

            string[] namesToTry = new string[] { "Value", "Text" };

            object valObj = null;

            foreach (string nm in namesToTry)
            {
                valObj = EntityUtils.GetObjectValue(sender, nm);
                if (valObj != null)
                {
                    break;
                }
            }
            if (valObj == null)
            {
                return;
            }

            string valStr = valObj.ToString();
            int val = 0;
            int.TryParse(valStr, out val);

            Control cont = sender as Control;
            if (cont == null || string.IsNullOrEmpty(cont.Name))
            {
                return;
            }

            for (int c = 0; c < NumChannels; c++)
            {
                if (cont.Name.IndexOf(channels[c]) == 0)
                {
                    vals[c] = MathUtils.Clamp(0, val, 255);
                    RefreshScreen();
                    break;
                }
            }
        }
    }
}
