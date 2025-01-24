using Genrpg.Shared.Crawler.Info.Constants;
using Genrpg.Shared.Crawler.Info.EffectHelpers;
using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Genrpg.Shared.Crawler.Info.Services
{

    public interface IInfoService : IInjectable
    {
        List<string> GetInfoLines(long entityTypeId, long entityId);
        string CreateInfoLink(IIdName idname);
        List<string> GetInfoLines(string entityLink); 
        string GetEffectText(CrawlerSpell spell, CrawlerSpellEffect effect);
    }
    
    public class InfoService : IInfoService
    {
        private ITextService _textService;

        private SetupDictionaryContainer<long,IInfoHelper> _entityInfoDict = new SetupDictionaryContainer<long, IInfoHelper> ();
        private SetupDictionaryContainer<long,ISpellEffectHelper> _spellEffectDict = new SetupDictionaryContainer<long, ISpellEffectHelper> ();

        public List<string> GetInfoLines(long entityTypeId, long entityId)
        {
            if (_entityInfoDict.TryGetValue (entityTypeId, out IInfoHelper info))
            {
                return info.GetInfoLines(entityId);
            }

            return new List<string> ();
        }

        public string GetEffectText(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            if (_spellEffectDict.TryGetValue(effect.EntityTypeId, out ISpellEffectHelper helper))
            {
                return helper.ShowEffectInfo(spell, effect);
            }
            return "";
        }


        public string CreateInfoLink(IIdName idname)
        {
            if (idname == null)
            {
                return "";
            }


            string linkId = idname.GetType().Name + " " + idname.IdKey;
            return InfoConstants.LinkPrefix + linkId + InfoConstants.LinkMiddle + _textService.HighlightText(idname.Name, TextColors.ColorYellow) + InfoConstants.LinkSuffix;

        }

        public List<string> GetInfoLines(string entityLink)
        {
            if (string.IsNullOrEmpty(entityLink))
            {
                return new List<string>();
            }

            string[] words = entityLink.Split (' ');

            if (words.Length < 1 || string.IsNullOrEmpty(words[0]) || string.IsNullOrEmpty(words[1]))
            {
                return new List<string>();
            }
          

            if (Int64.TryParse(words[1], out long entityId))
            {
                foreach (IInfoHelper helper in _entityInfoDict.GetDict().Values)
                {
                    if (helper.GetTypeName() == words[0])
                    {
                        return GetInfoLines(helper.GetKey(), entityId);
                    }
                }
            }
            return new List<string>();
        }
    }
}
