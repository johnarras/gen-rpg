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
        public static readonly string Local = EnvEnum.Local.ToString();
        public static readonly string Dev = EnvEnum.Dev.ToString();
        public static readonly string Test = EnvEnum.Test.ToString();
        public static readonly string Prod = EnvEnum.Prod.ToString();
    }

    public enum EnvEnum
    {
        Local = 0,
        Dev = 1,
        Test = 2,
        Prod = 3,
    }
}
