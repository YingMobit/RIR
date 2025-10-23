namespace Drive.Serialization {
    using ProtoBuf;
    using System.IO;
    public static class ProtobufSerializer {
        public static byte[] Serialize<T>(T obj) {
            using (var memoryStream = new MemoryStream()) {
                Serializer.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
        public static T Deserialize<T>(byte[] data) {
            using (var memoryStream = new MemoryStream(data)) {
                return Serializer.Deserialize<T>(memoryStream);
            }
        }
    }
}