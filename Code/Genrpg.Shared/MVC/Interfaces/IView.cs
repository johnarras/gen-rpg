using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MVC.Interfaces
{
    public interface IView
    {
        T Get<T>(string name) where T : class;

        object Position();
        object LocalPosition();
        void SetPosition(object position);
        void SetLocalPosition(object localPosition);
    }
}
