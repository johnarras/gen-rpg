
using Genrpg.Shared.Entities.Utils;
using System.ComponentModel;
using System.Data;
using System;
using System.Linq;
using System.Reflection;

namespace Genrpg.Editor.Utils
{
    public static class DataImportTools
    {


        public static T ImportLine<T>(string[] data, string[] headers, T curr = null) where T : class, new()
        {
            if (curr == null)
            {
                curr = new T();
            }

            PropertyInfo[] allProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 1; i < headers.Length && i < data.Length; i++)
            {
                string header = headers[i].Trim().ToLower();

                PropertyInfo prop = allProperties.FirstOrDefault(x => x.Name.ToLower() == header);

                if (prop == null)
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
                        object value = converter.ConvertFromString(data[i]);
                        if (value is string str)
                        {
                            if (str == "")
                            {
                                value = null;
                            }
                        }
                        prop.SetValue(curr,value);
                    }
                }
            }
            return curr;

        }
    }
}
