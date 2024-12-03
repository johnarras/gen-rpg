using Genrpg.Editor.Constants;
using Genrpg.Editor.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SmallPopup : Window, IUICanvas
    {
        public SmallPopup(string text, int width = 0, int height = 0)
        {

            if (width == 0)
            {
                width = 400;
            }

            if (height == 0)
            {
                height = 200;
            }

            Content = _canvas;

            UIHelper.SetWindowRect(this, 200, 200, width, height);

            int border = 50;

            TextBlock tb = UIHelper.CreateLabel(this, ELabelTypes.Default, "DialogText", text, width - 2*border, height-2*border, border, border,36);
        }

        private Canvas _canvas = new Canvas();

        public void Add(UIElement elem, double x, double y)
        {
            _canvas.Children.Add(elem);
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        public void Remove(UIElement elem)
        {
            _canvas.Children.Remove(elem);
        }

        public bool Contains(UIElement uIElement)
        {
            return _canvas.Children.Contains(uIElement);
        }

        public void StartClose()
        {
            DispatcherQueue.TryEnqueue(() => Close());
        }
    }
}
