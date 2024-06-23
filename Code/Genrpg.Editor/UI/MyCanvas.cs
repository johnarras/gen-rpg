using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.UI
{
    public class MyCanvas : Canvas, IUICanvas
    {
        public void Add(UIElement elem,  double x, double y)
        {
            Children.Add(elem);
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        public bool Contains(UIElement elem)
        {
          return Children.Contains(elem);   
        }

        public void Remove(UIElement elem)
        {
            Children.Remove(elem);
        }
    }
}
