using MessagePack;
using Genrpg.Shared.Constants;

public class PatcherUtils
{
    public static string GetPatchFilename(string prefix, string env, string platform, int version)
    {
        string prefixTxt = prefix.ToLower();
        string envTxt = (env == EnvNames.Prod ? EnvNames.Prod : EnvNames.Test).ToString().ToLower();
        string platformTxt = platform.ToLower();
        return prefixTxt + "_" + envTxt + "_" + platformTxt + "_" + version + ".zip";
    }

    public static string GetPatchVersionFilename()
    {
        return "versions.txt";
    }

    public static string GetPatchVersionPath(string hostName, string gamePrefix, string env, string platform, int version)
    {
        return hostName + GetContainerName(gamePrefix, env) + "/" + GetPatchClientPrefix(gamePrefix, env, platform, version) + GetPatchVersionFilename();
    }

    public static string GetPatchURL(string hostName, string gamePrefix, string env, string platform, int version)
    {
        return hostName + GetContainerName(gamePrefix, env) + "/" + GetRemoteFilePath(gamePrefix, env, platform, version);
    }

    public static string GetRemoteFilePath(string gamePrefix, string env, string platform, int version)
    {
        return GetPatchClientPrefix(gamePrefix, env, platform, version) + GetPatchFilename(gamePrefix, env, platform, version);
    }

    public static string GetPatchClientPrefix(string gamePrefix, string env, string platform, int version)
    {
        return "client/";
    }

    public static string GetContainerName(string gamePrefix, string env)
    {
        string gameSuffix = env == EnvNames.Prod ? "" : "dev";
        return gamePrefix.ToLower() + gameSuffix;
    }
}
