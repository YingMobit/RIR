namespace Drive.Serialization {
    using ProtoBuf;
    using System;
    using System.IO;
    public static class ProtobufSerializer {
        public static NetworkMessage GetNetworkMessage(ArraySegment<byte> data) {
            var bytes = new byte[data.Count];
            Buffer.BlockCopy(data.Array!,data.Offset,bytes,0,data.Count);
            var networkMessage = Deserialize<NetworkMessage>(bytes);
            return networkMessage;
        }

        public static byte[] Serialize<T>(T obj) {
            using var memoryStream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(memoryStream,obj);
            return memoryStream.ToArray();
        }
        public static T Deserialize<T>(byte[] data) {
            using var memoryStream = new MemoryStream(data);
            return ProtoBuf.Serializer.Deserialize<T>(memoryStream);
        }

        public static ArraySegment<byte> PackNetworkMessage(NetworkMessage networkMessage) {
            var bytes = Serialize(networkMessage);
            return new ArraySegment<byte>(bytes);
        }
    }
}