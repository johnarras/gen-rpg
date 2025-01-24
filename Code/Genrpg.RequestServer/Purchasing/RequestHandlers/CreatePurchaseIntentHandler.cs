using Genrpg.RequestServer.ClientUser.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Purchasing.WebApi.InitializePurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Purchasing.RequestHandlers
{
    internal class CreatePurchaseIntentHandler : BaseClientUserRequestHandler<InitializePurchaseRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, InitializePurchaseRequest request, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}
