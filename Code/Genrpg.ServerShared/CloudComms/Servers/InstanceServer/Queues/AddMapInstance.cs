
namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class AddMapInstance : IInstanceQueueMessage
    {
        public string ServerId { get; set; }
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int Size { get; set; }
    }
}
