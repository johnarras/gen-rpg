
namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class LoginUser : IPlayerQueueMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
