using Genrpg.ServerShared.CloudComms.Queues.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Queues.Requests.Entities
{
    public interface IResponseQueueMessage : IQueueMessage
    {
        public string RequestId { get; set; }
        public string ErrorText { get; set; }
    }
}
