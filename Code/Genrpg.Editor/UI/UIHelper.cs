using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using System;
using Microsoft.UI.Xaml.Media;
using Genrpg.Editor.Constants;
using static System.Collections.Specialized.BitVector32;

namespace Genrpg.Editor.UI
{
    public static class UIHelper
    {
        public static SmallPopup ShowBlockingDialog(Window parent, string text, double width = 0, double height = 0)
        {         
            SmallPopup win = new SmallPopup(text, (int)width, (int)height);
            win.Activate();
            return win;
        }

        public static async Task<ContentDialogResult> ShowMessageBox(Window window, string content, string title = null, bool showCancelButton = false)
        {

            MessageBoxWaiter waiter = new MessageBoxWaiter();


            window.DispatcherQueue.TryEnqueue(() =>
            {
                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "Ok",
                    SecondaryButtonText = (showCancelButton ? "Cancel" : null),
                };

                noWifiDialog.XamlRoot = window.Content.XamlRoot;

                waiter.Operation = noWifiDialog.ShowAsync();
                waiter.DidSetOperation = true;
            });

            while (!waiter.DidSetOperation ||
               waiter.Operation.Status == AsyncStatus.Started)
            {
                await Task.Delay(1);
            }

            waiter.Result = waiter.Operation.GetResults();
            return waiter.Result;
        }

        public static Button CreateButton(IUICanvas canvas, EButtonTypes buttonType, 
            string name, string text, double width, double height, double xpos, double ypos, RoutedEventHandler clickAction)
        {
            Button button = new Button()
            {
                Height = height,
                Width = width,
                Content = text,
                Name = name,
            };

            canvas.Add(button, xpos, ypos);
            button.Click += clickAction;

            return button;
        }

        public static TextBlock CreateLabel(IUICanvas canvas, ELabelTypes labelType,
            string name, string text, double width, double height, double xpos, double ypos, float fontSize = FormatterConstants.DefaultLabelFontSize, TextAlignment alignment = TextAlignment.Center)
        {
            TextBlock label = new TextBlock()
            {
                RequestedTheme = ElementTheme.Default,
                Height = height,
                Width = width,
                Text = text,
                Name = name,
                TextAlignment = alignment,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            };

            canvas.Add(label, xpos, ypos);

            return label;
        }

        public static void SetPosition(UIElement elem, int x, int y)
        {
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        public static DataGrid CreateDataGridView(IUICanvas canvas,
            string name, double width, double height, double xpos, double ypos)
        {
            DataGrid dataGridView = new DataGrid()
            {
                Name = name,
                Width = width,
                Height = height,
                SelectionMode = DataGridSelectionMode.Extended,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            };

            canvas.Add(dataGridView, xpos, ypos);

            return dataGridView;
        }

        public static ComboBox CreateComboBox(IUICanvas canvas, string name,
            double width, double height, double xpos, double ypos)
        {
            ComboBox comboBox = new ComboBox()
            {
                Height = height,
                Width = width,
                Name = name,
                AllowDrop = true,
            };

            canvas.Add(comboBox, xpos, ypos);   
            return comboBox;
        }


        public static TextBox CreateTextBox(IUICanvas canvas,  string name, string initialText,
            double width, double height, double xpos, double ypos, TextChangedEventHandler eventHandler)
        {
            TextBox textBox = new TextBox()
            {
                Name = name,
                Text = initialText,
                Height = height,
                Width = width,
            };

            if (eventHandler != null)
            {
                textBox.TextChanged += eventHandler;
            }

            canvas.Add(textBox, xpos, ypos);

            return textBox;
        }

        public static CheckBox CreateCheckBox(IUICanvas canvas,  string name,
            double width, double height, double xpos, double ypos)
        {
            CheckBox checkBox = new CheckBox()
            {
                Name = name,
                Height = height,
                Width = width,
            };

            canvas.Add(checkBox, xpos, ypos);
            return checkBox;
        }


        public static void SetWindowRect(Window window, double xpos, double ypos, double width, double height)
        {
            window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32((int)xpos,(int)ypos, (int)(width*ScalingConstants.DisplayScaling),
                (int)(height*ScalingConstants.DisplayScaling)));    
        }

        public static bool IsKeyDown(VirtualKey key)
        {
            CoreVirtualKeyStates state = CoreWindow.GetForCurrentThread().GetKeyState(key);
            return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
    }
}
