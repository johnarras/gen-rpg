using Genrpg.Shared.Crawler.Info.Helpers;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.Services
{

    public interface IInfoService : IInjectable
    {
        List<string> GetInfoLines(long entityTypeId, long entityId);
    }
    
    public class InfoService : IInfoService
    {

       private SetupDictionaryContainer<long,IInfoHelper> _infoDict = new SetupDictionaryContainer<long, IInfoHelper> ();

        public List<string> GetInfoLines(long entityTypeId, long entityId)
        {
            if (_infoDict.TryGetValue (entityTypeId, out IInfoHelper info))
            {
                return info.GetInfoLines(entityId);
            }

            return new List<string> ();
        }
    }
}
