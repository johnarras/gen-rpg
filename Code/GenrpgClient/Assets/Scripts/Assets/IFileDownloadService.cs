
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading;

public interface IFileDownloadService : IInitializable
{
    void DownloadFile(string url, DownloadFileData data, CancellationToken token);
    void DownloadTypedFile<T>(string url, Action<T> handler, EDataCategories category, CancellationToken token);
}
