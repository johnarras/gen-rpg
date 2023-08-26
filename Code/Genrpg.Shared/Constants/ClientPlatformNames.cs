using MessagePack;
namespace Genrpg.Shared.Constants
{


    // List of client platform names. Make sure the spellings match the enum names in ClientPlatformNamesEnum
    [MessagePackObject]
    public class ClientPlatformNames
    {
        public const string Win = "Win";
        public const string OSX = "OSX";
        public const string Linux = "Linux";


        public static string GetApplicationSuffix(string platformName)
        {
            if (platformName == Win)
            {
                return ".exe";
            }
            else if (platformName == OSX)
            {
                return ".app";
            }
            else if (platformName == Linux)
            {
                return ".app";
            }

            return ".exe";
        }

    }



}
