using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Genrpg.Editor.Services.Importing
{
    public interface IImportService : IInjectable
    {
        T ImportLine<T>(EditorGameState gs, int row, string[] data, string[] headers, T curr = null) where T : class, new();
        void ConvertImportWordsToContainer(object import, List<IIdName> children, SmallIdLongCollection cont);
    }

    public class ImportService : IImportService
    {

        private IEditorReflectionService _reflectionService;

        public T ImportLine<T>(EditorGameState gs, int row, string[] data, string[] headers, T curr = null) where T : class, new()
        {
            if (curr == null)
            {
                curr = new T();
            }

            PropertyInfo[] allProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 1; i < headers.Length && i < data.Length; i++)
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
    }
}
