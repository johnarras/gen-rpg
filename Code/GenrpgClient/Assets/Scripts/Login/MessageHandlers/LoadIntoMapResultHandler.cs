using Assets.Scripts.Login.Messages.Core;
using System.Threading.Tasks;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using System.Threading;
using Genrpg.Shared.DataStores.Categories;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoadIntoMapResultHandler : BaseClientLoginResultHandler<LoadIntoMapResult>
    {
        private IZoneGenService _zoneGenService;
        protected override void InnerProcess(UnityGameState gs, LoadIntoMapResult result, CancellationToken token)
        {
            TaskUtils.AddTask(_zoneGenService.OnLoadIntoMap(gs, result, token), "loadintomap", token);

        }
    }
}
