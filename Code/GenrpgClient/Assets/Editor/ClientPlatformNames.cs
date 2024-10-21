using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Editor
{
    public class ClientPlatformNames
    {
        public const string Win = "Win";
        public const string OSX = "OSX";
        public const string Linux = "Linux";
        public const string Android = "Android";

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
            else if (platformName == Android)
            {
                return ".apk";
            }
            return ".exe";
        }

    }
}
