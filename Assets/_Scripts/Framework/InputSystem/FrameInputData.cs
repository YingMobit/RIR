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
        [ProtoMember(3)] public int AuthorityNetworkFrameCount; //服务器给出的权威网络帧号
        [ProtoMember(4)] public int LocalizedLocalLogicFrameCount; //远端客户端获取到输入之后将本值赋值为同一个网络包的本地客户端权威逻辑帧号
        [ProtoMember(5)] public LVector3 AimDirection;
        [ProtoMember(6)] public int PlayerID;
        [ProtoMember(7)] public bool ServerReceived; //服务器是否收到该输入数据,如果为false则表示这一阵服务端并没有收到数据,需要客户端模拟

        public static FrameInputData Null;
        public bool IsRightPredict(FrameInputData authoritiveData) { 
            return this.KeyCodeinputs == authoritiveData.KeyCodeinputs &&
                   this.AimDirection == authoritiveData.AimDirection;
        }

        public readonly FrameInputData MakePredict(int localizedLocalLogicFrameCount) { 
            var predict = this;
            predict.KeyCodeinputs = KeyCodeinputs.MakePredict();
            predict.AuthorityNetworkFrameCount = -1;
            predict.LocalizedLocalLogicFrameCount = localizedLocalLogicFrameCount;
            return predict; 
        }

        static FrameInputData() {
            Null = new();
            Null.KeyCodeinputs = 0;
            Null.AuthorityLocalLogicFrameCount = -1;
            Null.AuthorityNetworkFrameCount = -1;
            Null.AimDirection = LVector3.forward;
            Null.PlayerID = -1;
        }

        public FrameInputData(InputTypeEnum input,int localFrameCount,int networkFrameCount,LVector3 aimDir,int playerID) {
            KeyCodeinputs = input.InputTypeToInt();
            AuthorityLocalLogicFrameCount = localFrameCount;
            AuthorityNetworkFrameCount = networkFrameCount;
            AimDirection = aimDir;
            PlayerID = playerID;
            LocalizedLocalLogicFrameCount = -1;
            ServerReceived = false;
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
            return $"FrameInputData PlayerID:{PlayerID} " +
                $"AuthorityLocalLogicFrameCount:{AuthorityLocalLogicFrameCount} " +
                $"AuthorityNetworkFrame:{AuthorityNetworkFrameCount} " +
                $"LocalizedLocalLogicFrameCount:{LocalizedLocalLogicFrameCount} " +
                $"ServerReceived:{ServerReceived} " +
                $"KeyCodeInputs:{KeyCodeinputs} " +
                $"AimDir:{AimDirection}";
        }
        #endregion
    }
}
