using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Client.Updates
{
    public interface IGlobalUpdater
    {
        void OnUpdate();
        void OnLateUpdate();
    }
}
