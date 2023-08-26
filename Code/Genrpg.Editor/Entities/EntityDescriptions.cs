using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Versions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Entities
{
    public class EntityDescriptions
    {
        private static Dictionary<string, string> _descriptions = null;
        public static string GetEntityDescription(string parentTypeName, string childMemberName)
        {
            SetupEntityDescriptions();
               
            if (_descriptions.ContainsKey(parentTypeName + childMemberName))
            {
                return _descriptions[parentTypeName + childMemberName];
            }
            return null;
        }

        private static void SetupEntityDescriptions()
        {
            if (_descriptions != null)
            {
                return;
            }

            _descriptions = new Dictionary<string, string>();

            VersionSettings versionSettings = new VersionSettings();
            AddDesc<VersionSettings>(nameof(versionSettings.ClientVersion), "The client version");
        }

        private static void AddDesc<T>(string memberName, string desc)
        {
            _descriptions.Add(typeof(T).Name + memberName, desc);
        }
    }
}
