using Genrpg.Editor.Services.Reflection;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Services.Setup
{
    public class EditorLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(GameState gs)
        {
            gs.loc.Set<IEditorReflectionService>(new EditorReflectionService());
            base.Setup(gs);
        }
    }
}
