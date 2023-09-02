using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.ProcGen.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameDataUtils
{
    public static void SetData(UnityGameState gs, GameData data)
    {
        gs.data = data;
    }
}
