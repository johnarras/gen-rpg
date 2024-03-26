using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Services.Setup
{
    public class EditorSetupService : SetupService
    {
        public override void SetupServiceLocator(GameState gs)
        {
            EditorLocatorSetup els = new EditorLocatorSetup();
            els.Setup(gs);
            gs.loc.ResolveSelf();
        }

        public override bool CreateMissingGameData() { return true; } 
    }
}
