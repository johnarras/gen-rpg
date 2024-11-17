using MessagePack;
namespace Genrpg.Shared.Constants
{

    [MessagePackObject]
    public class Game
    {
        public const string Prefix = "Genrpg";
    }

    [MessagePackObject]
    public class EnvNames
    {
        public const string Local = "local";
        public const string Dev = "dev";
        public const string Test = "test";
        public const string Prod = "prod";
    }   
}
