using Assets.Scripts.Login.Messages.Core;

using System.Threading;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.MapServer.WebApi.LoadIntoMap;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoadIntoMapResponseHandler : BaseClientLoginResultHandler<LoadIntoMapResponse>
    {

        public override int Priority() { return 1000; }

        private IZoneGenService _zoneGenService;
        protected override void InnerProcess(LoadIntoMapResponse result, CancellationToken token)
        {
            _zoneGenService.OnLoadIntoMap(result, token);
        }
    }
}
