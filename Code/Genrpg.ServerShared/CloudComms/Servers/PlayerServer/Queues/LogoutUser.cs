
namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class LogoutUser : IPlayerQueueMessage
    {
        public string Id { get; set; }
    }
}
