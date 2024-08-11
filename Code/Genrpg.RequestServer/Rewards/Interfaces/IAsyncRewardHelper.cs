using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Rewards.Interfaces
{
    /// <summary>
    /// Only use this inside of the website since it has to do async loads.
    /// </summary>
    public interface IAsyncRewardHelper : ISetupDictionaryItem<long>
    {
        /// <summary>
        /// Website async only.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="ch"></param>
        /// <param name="entityId"></param>
        /// <param name="quantity"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData = null);
    }
}
