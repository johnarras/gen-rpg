using Genrpg.MapServer.MapMessaging;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Requests;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.WhoList.Entities;
using Genrpg.Shared.WhoList.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.WhoList.MessageHandlers
{
    public class GetWhoListHandler : BaseServerMapMessageHandler<GetWhoList>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, GetWhoList message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            _cloudCommsService.SendRequest(CloudServerNames.Player, new WhoListRequest() { Args = message.Args }, (envelope) =>
            {
                OnReceiveWhoList(obj.Id, envelope);
            });
        }

        private void OnReceiveWhoList(string charId, ResponseEnvelope envelope)
        {
            if (!_objectManager.GetChar(charId, out Character ch))
            {
                return;
            }

            WhoListResponse response = envelope.Response as WhoListResponse;

            if (response == null)
            {
                ch.AddMessage(new ErrorMessage("Bad Who List Response"));
            }
            else
            {
                OnGetWhoList onGetList = new OnGetWhoList()
                {

                };

                foreach (WhoListChar wlc in response.Chars)
                {
                    onGetList.Items.Add(new WhoListItem()
                    {
                        Id= wlc.Id,
                        Name= wlc.Name,
                        Level= wlc.Level,
                        ZoneName = wlc.ZoneName,
                    });
                }
                Console.WriteLine("GOT WHO LIST WITH " + onGetList.Items.Count + " ITEMS");
                ch.AddMessage(onGetList);
            }
        }
    }
}
