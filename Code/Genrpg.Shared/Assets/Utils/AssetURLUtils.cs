using MessagePack;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Assets.Utils
{
    [MessagePackObject]
    public class AssetURLUtils
    {
        public static string GetArtURLPrefix(GameState gs)
        {

            string prefix = gs.data.GetGameData<CoreSettings>(null).ArtURL;

            prefix += Game.Prefix.ToLower();

            if (gs.data.GetGameData<CoreSettings>(null).Env != EnvNames.Prod)
            {
                prefix += EnvNames.Dev.ToLower();
            }

            prefix += "/";

            return prefix;
        }
    }
}
