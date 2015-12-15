using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AIT.Taskboard.Model;

namespace AIT.Taskboard.Interface.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string SerializeAsString(this IConfiguration config)
        {
            if (config == null) return string.Empty;

            var formatter = new BinaryFormatter();
            using (var serializationStream = new MemoryStream())
            {
                formatter.Serialize(serializationStream, config);
                serializationStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new BinaryReader(serializationStream))
                {
                    return Convert.ToBase64String(reader.ReadBytes((int) serializationStream.Length));
                }
            }
        }
        public static IConfiguration DeserializeFromString(this string serializedValue)
        {
            if (string.IsNullOrEmpty(serializedValue)) return new ConfigurationV5();
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(Convert.FromBase64String(serializedValue));
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var formatter = new BinaryFormatter();
                    try
                    {
                        return formatter.Deserialize(memoryStream) as IConfiguration;
                    }
                    catch 
                    {
                        return new ConfigurationV5();
                    }
                }
            }
        }
    }
}