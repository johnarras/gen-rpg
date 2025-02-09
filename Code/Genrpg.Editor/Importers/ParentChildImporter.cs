using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public abstract class ParentChildImporter<TParent,TChild> : BaseDataImporter where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        protected abstract void ImportChildSubObject(EditorGameState gs, TChild current, string[] headers, string[] rowWords);


        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            TParent settings = gs.data.Get<TParent>(null);

            List<TChild> newList = new List<TChild>();

            string childTypeName = typeof(TChild).Name.ToLower();
            Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

            TChild currentChild = null;
            for (int row = 0; row < lines.Count; row++)
            {
                string[] rowWords = lines[row];

                if (rowWords.Length < 2 || string.IsNullOrEmpty(rowWords[0]))
                {
                    continue;
                }

                rowWords[0] = rowWords[0].ToLower();

                if (rowWords[0].IndexOf("header") >= 0)
                {
                    string headerWord = rowWords[0].Replace("header", "").Trim();

                    headers[headerWord] = rowWords;
                    continue;
                }


                if (rowWords[0] == childTypeName)
                {
                    currentChild = _importService.ImportLine<TChild>(gs, row, rowWords, headers[childTypeName]);
                    newList.Add(currentChild);
                }
                else
                {
                    if (headers.TryGetValue(rowWords[0].ToLower(), out string[] headerRow))
                    {
                        ImportChildSubObject(gs, currentChild, headerRow, rowWords);
                    }
                }
            }


            settings.SetData(newList);
            gs.LookedAtObjects.AddRange(newList);

            await Task.CompletedTask;
            return true;
        }
    }
}
