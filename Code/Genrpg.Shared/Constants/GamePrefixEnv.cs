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
        public static readonly string Local = "local";
        public static readonly string Dev = "dev";
        public static readonly string Test = "test";
        public static readonly string Prod = "prod";
    }
}
