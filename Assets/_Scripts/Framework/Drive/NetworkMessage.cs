using InputSystemNameSpace;
using ProtoBuf;
using System;

namespace Drive {
    [Serializable]
    [ProtoContract]
    public struct NetworkMessage {
        [ProtoMember(0)] public NetworkMessageType NetworkMessageType;
        [ProtoMember(1)] public byte[] DataStream;
    }

    [Serializable]
    [ProtoContract]
    public struct NetworkMessageType {
        [ProtoMember(0)] public uint MessageType;

        public static readonly NetworkMessageType PlayerInputsMessage = new() { MessageType = 1 };
        public static readonly NetworkMessageType PlayerIDAllocationMessage = new() { MessageType = 2 };
        public static readonly NetworkMessageType CommandMessage = new() { MessageType = 3 };
        public static readonly NetworkMessageType CharactorChooseMessage = new() { MessageType = 4 };

        public static bool operator ==(NetworkMessageType a, NetworkMessageType b) => a.MessageType == b.MessageType;
        public static bool operator !=(NetworkMessageType a, NetworkMessageType b) => a.MessageType != b.MessageType;
    }

    public struct NetworkPlayerInputsMessage { 
        public int NetworkFrameCount;
        public FrameInputData[] Inputs;
    }

    public struct NetworkPlayerIDAllocationMessage {
        public int PlayerID;
    }

    public struct NetworkCommandMessgae {
        public int CommandID;

        public const int NETWORKCOMMAND_STARTGAME = 1;
    }

    public struct NetworkCharactorChooseMessage {
        public int PlayerID;
        public int CharactorID;
    }

}