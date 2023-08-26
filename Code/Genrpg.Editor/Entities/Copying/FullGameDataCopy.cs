using Genrpg.Shared.GameDatas.Config;
using Genrpg.Shared.GameDatas.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Entities.Copying
{
    public class FullGameDataCopy
    {
        public List<DataConfig> Configs { get; set; } = new List<DataConfig>();
        public List<IGameDataContainer> Data { get; set; } = new List<IGameDataContainer>();
    }
}
