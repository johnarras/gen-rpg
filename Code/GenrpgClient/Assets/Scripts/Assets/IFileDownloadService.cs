
using Genrpg.Shared.Interfaces;
using System.Threading;

public interface IFileDownloadService : IInitializable
{
    void DownloadFile(UnityGameState gs, string url, DownloadFileData data, bool worldData, CancellationToken token);
}
