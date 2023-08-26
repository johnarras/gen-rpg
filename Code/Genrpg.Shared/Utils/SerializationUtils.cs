using MessagePack;
using Newtonsoft.Json;
using System;
using MessagePack;
using System.Runtime.Serialization.Formatters;
using MessagePack.Resolvers;

namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// This class is used to serialize and deserialize all kinds of data.
    /// IT is used for 
    /// 1. Server data storage I/O
    /// 2. Cache I/O
    /// 3. Asynchronous Client/Server communications
    /// 4. Editor I/O
    /// 5. Sending commands to all instances in a role.
    /// 6. Realtime Client/Server communication
    /// 7. Client device I/O
    /// </summary>
    [MessagePackObject]
    public class SerializationUtils
    {

        private static JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Formatting = Formatting.Indented,
        };

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        public static object Deserialize(string txt, Type t)
    {
            int newIndex = 0;
            while (newIndex < txt.Length && txt[newIndex] != '{' && txt[newIndex] != '[')
            {
                newIndex++;
            }
            if (newIndex > 0)
            {
                txt = txt.Substring(newIndex);
            }
            return JsonConvert.DeserializeObject(txt, t, _settings);
        }

        public static T Deserialize<T>(string txt)
        {
            return (T)Deserialize(txt, typeof(T));
        }

        public static T SafeMakeCopy<T>(T t) where T : class
        {
            return (T)Deserialize(Serialize(t), t.GetType());
        }

        public static TOutput ConvertType<TInput, TOutput>(TInput input)
        {
            string txt = Serialize(input);
            return Deserialize<TOutput>(txt);
        }

        public static T BinaryDeserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public static byte[] BinarySerialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public static T FastMakeCopy<T>(T t) where T : class
        {
            return BinaryDeserialize<T>(BinarySerialize(t));
        }

    }
}
