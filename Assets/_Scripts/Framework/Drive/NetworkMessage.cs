using InputSystemNameSpace;
using ProtoBuf;
using System;

namespace Drive {
    [Serializable]
    [ProtoContract]
    public struct NetworkMessage {
        [ProtoMember(1)] public NetworkMessageType NetworkMessageType;
        [ProtoMember(2)] public byte[] DataStream;
    }

    [Serializable]
    [ProtoContract]
    public struct NetworkMessageType {
        [ProtoMember(1)] public uint MessageType;
        public static readonly NetworkMessageType PlayerInputsMessage = new() { MessageType = 1 };
        public static readonly NetworkMessageType PlayerIDAllocationMessage = new() { MessageType = 2 };
        public static readonly NetworkMessageType CommandMessage = new() { MessageType = 3 };
        public static readonly NetworkMessageType CharactorChooseMessage = new() { MessageType = 4 };
        public static readonly NetworkMessageType PlayerReadyMessage = new() { MessageType = 5 };
        public static bool operator ==(NetworkMessageType a,NetworkMessageType b) => a.MessageType == b.MessageType;
        public static bool operator !=(NetworkMessageType a,NetworkMessageType b) => a.MessageType != b.MessageType;
    }


    [Serializable]
    [ProtoContract]
    public struct NetworkPlayerInputsDownLinkMessage {
        [ProtoMember(1)] public int NetworkFrameCount;
        [ProtoMember(2)] public FrameInputData[] Inputs;
    }


    [Serializable]
    [ProtoContract]
    public struct NetworkPlayerInputsUpLinkMessage {
        [ProtoMember(1)] public int PlayerID;
        [ProtoMember(2)] public FrameInputData Input;
    }

    [Serializable]
    [ProtoContract]
    public struct NetworkPlayerIDAllocationMessage {
        [ProtoMember(1)] public int PlayerID;
    }


    [Serializable]
    [ProtoContract]
    public struct NetworkPlayerReadyMessage {
        [ProtoMember(1)] public int PlayerID;
    }


    [Serializable]
    [ProtoContract]
    public struct NetworkCommandMessgae {
        [ProtoMember(1)] public int CommandID;
        public const int NETWORKCOMMAND_PLAYERJOINED = 1;//玩家加入房间
        public const int NETWORKCOMMAND_HOSTASSIGNMENT = 2;//房主分配
        public const int NETWORKCOMMAND_HOSTSTARTGAME = 3;//房主开始游戏
        public const int NETWORKCOMMAND_ALLPLAYERREADY = 4;//所有玩家准备完毕，加载游戏场景
    }

    [Serializable]
    [ProtoContract]
    public struct NetworkCharactorChooseMessage {
        [ProtoMember(1)] public int PlayerID;
        [ProtoMember(2)] public int CharactorID;
    }
}