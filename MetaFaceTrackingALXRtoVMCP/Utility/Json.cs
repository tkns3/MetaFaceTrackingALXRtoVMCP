using Newtonsoft.Json;
using System.IO;

namespace MetaFaceTrackingALXRtoVMCP.Utility
{
    internal static class Json
    {
        public static T? DeserializeFromLocalFile<T>(string path, JsonSerializerSettings? setting = null)
        {
            if (!File.Exists(path))
            {
                return default;
            }
            using var sr = File.OpenText(path);
            using var reader = new JsonTextReader(sr);
            var serializer = JsonSerializer.Create(setting);
            var deserialized = serializer.Deserialize<T>(reader);
            return deserialized;
        }

        public static void SerializeToLocalFile<T>(T data, string path, JsonSerializerSettings? setting)
        {
            var jsonString = JsonConvert.SerializeObject(data, setting);
            File.WriteAllText(path, jsonString, System.Text.Encoding.UTF8);
        }

        public static void SerializeToLocalFile<T>(T data, string path, Formatting format)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            if (dir != null) Directory.CreateDirectory(dir);
            var jsonString = JsonConvert.SerializeObject(data, format);
            File.WriteAllText(path, jsonString, System.Text.Encoding.UTF8);
        }
    }
}
