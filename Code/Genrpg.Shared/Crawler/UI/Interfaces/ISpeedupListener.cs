using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.UI.Interfaces
{
    public interface ISpeedupListener
    {
        bool TriggerSpeedupNow();
        void ClearSpeedup();
    }
}
