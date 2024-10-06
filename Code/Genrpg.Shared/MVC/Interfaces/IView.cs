using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MVC.Interfaces
{
    public interface IView
    {
        T Get<T>(string name) where T : class;
    }
}
