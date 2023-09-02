
using Genrpg.Shared.GameSettings.Config;
using Genrpg.Shared.GameSettings.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Entities.Copying
{
    public class FullGameDataCopy
    {
        public List<DataConfig> Configs { get; set; } = new List<DataConfig>();
        public List<IGameSettingsContainer> Data { get; set; } = new List<IGameSettingsContainer>();
    }
}
