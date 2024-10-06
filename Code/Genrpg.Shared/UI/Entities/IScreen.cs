using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.UI.Entities
{
    public interface IScreen
    {
        ScreenId ScreenID { get; }
        Task StartOpen(object data, CancellationToken token);
        void StartClose();
        void ErrorClose(string txt);
        void OnInfoChanged();
        bool BlockMouse();
        string GetName();
        CancellationToken GetToken();
    }
}
