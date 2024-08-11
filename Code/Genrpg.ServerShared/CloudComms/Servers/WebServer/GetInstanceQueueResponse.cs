using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;

namespace Genrpg.ServerShared.CloudComms.Servers.WebServer
{
    public class GetInstanceQueueResponse : ILoginQueueMessage, IResponseQueueMessage
    {
        public string RequestId { get; set; }
        public string ErrorText { get; set; }

        public string InstanceId { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
    }
}
