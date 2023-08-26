using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Users.Entities
{
    /// <summary>
    /// This is used on the client to store things that must be loaded instantly when the game starts.
    /// </summary>
    [MessagePackObject]
    public class InitialClientConfig : IStringId
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public int UserFlags { get; set; }


        public InitialClientConfig()
        {
            // Don't set flags here since if they are all set to 0, protobuf will ignore and 
            // Set them in the constructor again.
        }

    }

}
