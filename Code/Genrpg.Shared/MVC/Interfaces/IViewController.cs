using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.MVC.Interfaces
{
    public interface IViewController<TModel, IView> : IInjectable
    {
        Task Init(TModel model, IView view, CancellationToken token);
        IView GetView();
        TModel GetModel();
        CancellationToken GetToken();
    }
}
