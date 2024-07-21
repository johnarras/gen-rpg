using Assets.Scripts.UI.Services;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Screens
{
    public abstract class ErrorMessageScreen : BaseScreen
    {
        protected IUIService _uiService;

        public abstract void ShowError(string errorMessage);
    }
}
