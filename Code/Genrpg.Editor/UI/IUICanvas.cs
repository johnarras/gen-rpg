using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.UI
{
    public interface IUICanvas
    {
        void Add(UIElement elem, double x, double y);
        void Remove(UIElement elem);
        bool Contains(UIElement elem);
    }
}
