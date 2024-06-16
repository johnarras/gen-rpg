using Genrpg.Editor.Services.Reflection;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Services.Setup
{
    public class EditorSetupService : BaseServerSetupService
    {
        public EditorSetupService(IServiceLocator loc) : base(loc) { }  

        protected override void AddServices()
        {
            base.AddServices();
            Set<IEditorReflectionService>(new EditorReflectionService());
            _loc.ResolveSelf();
        }

        public override bool CreateMissingGameData() { return true; } 
    }
}
