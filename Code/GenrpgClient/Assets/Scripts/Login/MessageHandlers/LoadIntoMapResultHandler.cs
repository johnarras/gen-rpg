using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using System.Threading;
using Genrpg.Shared.DataStores.Categories;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoadIntoMapResultHandler : BaseClientLoginResultHandler<LoadIntoMapResult>
    {

        public override int Priority() { return 1000; }

        private IZoneGenService _zoneGenService;
        protected override void InnerProcess(LoadIntoMapResult result, CancellationToken token)
        {
            _zoneGenService.OnLoadIntoMap(result, token).Forget();
        }
    }
}
