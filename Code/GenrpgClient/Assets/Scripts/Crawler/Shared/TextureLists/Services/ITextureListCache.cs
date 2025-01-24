using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Genrpg.Shared.Crawler.TextureLists.Services
{

    public delegate void DownloadTextureListHandler(object textureList, object data);

    public interface ITextureListCache : IInitializable, IGameCleanup
    {
        void LoadTextureList(string textureName, DownloadTextureListHandler handler, object data, CancellationToken token);
    }
}
