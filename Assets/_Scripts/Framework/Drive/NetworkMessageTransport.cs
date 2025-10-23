using Drive.Serialization;
using kcp2k;
using System;

namespace Drive {
    public class NetworkMessageTransport {
        private NetworkManager networkManager;

        public NetworkMessageType GetNetworkMessgaeType(ArraySegment<byte> data,KcpChannel kcpChannel) {
            var messgaeType = new byte[4];
            Buffer.BlockCopy(data.Array,data.Offset,messgaeType,0,4);
            uint messageTypeUint = BitConverter.ToUInt32(messgaeType,0);
            return new NetworkMessageType { MessageType = messageTypeUint };
        }

        public MessageType GetNetworkdMessgae<MessageType>(ArraySegment<byte> data){
            var messageBytes = new byte[data.Count];
            Buffer.BlockCopy(data.Array,data.Offset + 4,messageBytes,0,data.Count);
            return ProtobufSerializer.Deserialize<MessageType>(messageBytes);
        }

        public ArraySegment<byte> OnSendNetworkMessage(NetworkMessage message) { 
            var bytes = ProtobufSerializer.Serialize(message);
            return new ArraySegment<byte>(bytes);
        }

        public NetworkMessageTransport(NetworkManager networkManager) {
            this.networkManager = networkManager;
        }
    }
}