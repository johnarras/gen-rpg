using Genrpg.Shared.Interfaces;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public interface IInfoHelper : ISetupDictionaryItem<long>
    {
        List<string> GetInfoLines(long entityId);
        string GetTypeName();
    }
}
