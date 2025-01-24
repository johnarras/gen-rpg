using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public abstract class SimpleInfoHelper<TParent, TChild> : BaseInfoHelper<TParent, TChild> where TParent : ParentSettings<TChild> where TChild : ChildSettings, new()
    {

        public override List<string> GetInfoLines(long entityId)
        {
            TChild child = _gameData.Get<TParent>(_gs.ch).Get(entityId);

            List<string> lines = new List<string>();

            if (child != null)
            {
                lines.Add(typeof(TChild).Name + ": " + child.Name);

                if (child is IIndexedGameItem indexedItem && !string.IsNullOrEmpty(indexedItem.Desc))
                {
                    lines.Add("Desc: " + indexedItem.Desc);
                }
            }

            return lines;
        }
    }
}
