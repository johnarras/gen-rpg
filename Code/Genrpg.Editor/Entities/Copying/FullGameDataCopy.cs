using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Entities.Copying
{
    public class FullGameDataCopy
    {
        public List<IGameSettings> Data { get; set; } = new List<IGameSettings>();
    }
}
