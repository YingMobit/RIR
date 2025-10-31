using Lockstep.Math;
using ProtoBuf;
using System;
using UnityEngine;

namespace InputSystemNameSpace {
    [Serializable]
    [ProtoContract]
    public struct FrameInputData {
        [ProtoMember(1)] public int KeyCodeinputs;
        [ProtoMember(2)] public int AuthorityLocalLogicFrameCount; //由本地客户端生成的权威逻辑帧号
        [ProtoMember(3)] public int AuthorityNetworkFrameCount; //远端客户端获取到输入之后将本值赋值为同一个网络包的本地客户端权威逻辑帧号
        [ProtoMember(4)] public int LocalizedLocalLogicFrameCount; //服务器给出的权威网络帧号
        [ProtoMember(5)] public LVector3 AimDirection;
        [ProtoMember(6)] public int PlayerID;

        public static FrameInputData Null;

        static FrameInputData() {
            Null = new();
            Null.KeyCodeinputs = 0;
            Null.AuthorityLocalLogicFrameCount = -1;
            Null.AuthorityNetworkFrameCount = -1;
            Null.AimDirection = LVector3.zero;
            Null.PlayerID = -1;
        }

        public FrameInputData(InputTypeEnum input,int localFrameCount,int networkFrameCount,LVector3 aimDir,int playerID) {
            KeyCodeinputs = input.InputTypeToInt();
            AuthorityLocalLogicFrameCount = localFrameCount;
            AuthorityNetworkFrameCount = networkFrameCount;
            AimDirection = aimDir;
            PlayerID = playerID;
            LocalizedLocalLogicFrameCount = -1;
        }

        #region Utility
        public LVector2 MoveInput { get {
                LVector2 res = LVector2.zero;
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveForward)) { 
                    res += LVector2.up;
                }
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveBackward)) { 
                    res += LVector2.down;
                }
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveLeft)) { 
                    res += LVector2.left;
                }
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveRight)) { 
                    res += LVector2.right;
                }
                return res;
            }
        }

        public override string ToString() {
            return $"FrameInputData PlayerID:{PlayerID} LocalFrame:{AuthorityLocalLogicFrameCount} NetworkFrame:{AuthorityNetworkFrameCount} KeyCodeInputs:{KeyCodeinputs} AimDir:{AimDirection}";
        }
        #endregion
    }
}
