using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Genrpg.Editor.Services.Importing
{
    public interface IImportService : IInjectable
    {
        T ImportLine<T>(EditorGameState gs, int row, string[] data, string[] headers, T curr = null, bool firstColumnHasData = false) where T : class, new();
        void ConvertImportWordsToContainer(object import, List<IIdName> children, SmallIdLongCollection cont);
        void AddEffectList<TImport, TParent, TChild, TEffect>(EditorGameState gs, int row, string headerWord, long entityTypeId, List<TEffect> effects, string data) where TEffect : IEffect, new()
            where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new();

        Task CleanOldObjects<T>(List<T> newObjects) where T : ChildSettings, IIndexedGameItem;

    }

    public class ImportService : IImportService
    {

        private IEditorReflectionService _reflectionService;
        private IRepositoryService _repoService;

        public T ImportLine<T>(EditorGameState gs, int row, string[] data, string[] headers, T curr = null, bool firstColumnHasData = false) where T : class, new()
        {
            if (curr == null)
            {
                curr = new T();
            }

            PropertyInfo[] allProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int i = (firstColumnHasData ? 0 : 1); i < headers.Length && i < data.Length; i++)
            {
                string header = StrUtils.NormalizeWord(headers[i]);

                PropertyInfo prop = allProperties.FirstOrDefault(x => x.Name.ToLower() == header);

                if (prop == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(data[i]))
                {
                    continue;
                }

                if (prop.PropertyType == typeof(DateTime))
                {
                    DateTimeConverter timeConverter = new DateTimeConverter();
                    prop.SetValue(curr, timeConverter.ConvertFromString(data[i]));
                }
                else
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(prop.PropertyType);

                    if (converter != null)
                    {
                        try
                        {
                            object value = converter.ConvertFromString(data[i]);
                            if (value is string str)
                            {
                                if (str == "")
                                {
                                    value = null;
                                }
                            }
                            prop.SetValue(curr, value);
                        }
                        catch (Exception ex)
                        {
                            bool didFindName = false;
                            List<IIdName> dropdownList = _reflectionService.GetDropdownList(gs, prop, curr);

                            if (prop.PropertyType.IsPrimitive)
                            {
                                if (dropdownList.Count > 0)
                                {
                                    string lowerName = StrUtils.NormalizeWord(data[i]);

                                    foreach (IIdName iidname in dropdownList)
                                    {
                                        string lowerIdName = StrUtils.NormalizeWord(iidname.Name);


                                        if (lowerIdName.Length >= lowerName.Length &&
                                            lowerName == lowerIdName.Substring(0, lowerName.Length))
                                        {
                                            prop.SetValue(curr, iidname.IdKey);
                                            didFindName = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!didFindName)
                            {
                                throw new Exception($"Bad Import for {typeof(T).Name} Row: {row} Header: {header} Data: {data[i]}");
                            }
                        }
                    }
                }
            }
            return curr;

        }



        public void ConvertImportWordsToContainer(object import, List<IIdName> children, SmallIdLongCollection cont)
        {

            PropertyInfo[] props = import.GetType().GetProperties();

            for (int p = 0; p < props.Length; p++)
            {
                IIdName matchingStat = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == props[p].Name.ToLower());

                if (matchingStat != null)
                {
                    int value = EntityUtils.GetObjectInt(import, props[p].Name);
                    if (value != 0)
                    {
                        cont.Add(matchingStat.IdKey, value);
                    }
                }
            }
        }

        public void AddEffectList<TImport, TParent, TChild, TEffect>(EditorGameState gs, int mainRow, string headerWord, long entityTypeId, List<TEffect> effects, string data)
            where TParent : ParentSettings<TChild>
            where TChild : ChildSettings, IIdName, new()
            where TEffect : IEffect, new()
        {

            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            string[] rows = data.Split(',');

            if (rows.Length < 1)
            {
                return;
            }


            IReadOnlyList<TChild> children = gs.data.Get<TParent>(null).GetData();

            foreach (string row in rows)
            {
                string trimmedRow = row.Trim();
                string mergedLowerRow = StrUtils.NormalizeWord(trimmedRow);
                if (string.IsNullOrEmpty(trimmedRow))
                {
                    continue;
                }
                
                string[] words = trimmedRow.Split(' ');

                if (words.Length < 1)
                {
                    continue;
                }

                for (int  w = 0; w < words.Length; w++)
                {
                    words[w] = StrUtils.NormalizeWord(words[w]);
                }

                if (words.Length < 1)
                {
                    continue;
                }

                TChild child = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == words[0]);

                if (child == null)
                {
                    child = children.FirstOrDefault(x=>StrUtils.NormalizeWord(x.Name) == mergedLowerRow);
                }

                if (child == null)
                {
                    throw new Exception($"Bad Import for {typeof(TImport).Name} Row: {mainRow} Header: {headerWord} Data: {data} Subitem: {row} Word: {words[0]} No {typeof(TChild).Name}  matches");
                }

                long quantity = 1;

                if (words.Length > 1)
                {
                    if (Int64.TryParse(words[1], out long qty))
                    {
                        quantity = qty;
                    }
                }

                quantity = Math.Max(1, quantity);

                effects.Add(new TEffect()
                {
                    EntityTypeId = entityTypeId,
                    EntityId = child.IdKey,
                    Quantity = quantity,
                });

            }
        }

        public async Task CleanOldObjects<T>(List<T> newObjects) where T : ChildSettings, IIndexedGameItem
        {

            if (newObjects.Count < 1)
            {
                return;
            }

            List<T> oldObjects = await _repoService.Search<T>(x => x.ParentId == GameDataConstants.DefaultFilename);


            foreach (T oldObject in oldObjects)
            {
                if (!newObjects.Any(x => x.IdKey == oldObject.IdKey))
                {
                    await _repoService.Delete(oldObject);
                }
            }
        }
    }
}
