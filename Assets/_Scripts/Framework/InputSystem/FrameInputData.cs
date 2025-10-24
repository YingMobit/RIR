using Lockstep.Math;
using ProtoBuf;
using System;
using UnityEngine;

namespace InputSystemNameSpace {
    [Serializable]
    [ProtoContract]
    public struct FrameInputData {
        [ProtoMember(1)] public int KeyCodeinputs;
        [ProtoMember(2)] public int LocalFrameCount;
        [ProtoMember(3)] public int NetworkFrameCount;
        [ProtoMember(4)] public LVector3 AimDirection;
        [ProtoMember(5)] public bool ServerRecivedInputThisFrame;
        [ProtoMember(6)] public int PlayerID;

        public static FrameInputData Null;

        static FrameInputData() {
            Null = new();
            Null.KeyCodeinputs = 0;
            Null.LocalFrameCount = -1;
            Null.NetworkFrameCount = -1;
            Null.AimDirection = LVector3.zero;
            Null.ServerRecivedInputThisFrame = false;
            Null.PlayerID = -1;
        }

        public FrameInputData(InputTypeEnum input,int localFrameCount,int networkFrameCount,LVector3 aimDir,int playerID) {
            KeyCodeinputs = input.InputTypeToInt();
            LocalFrameCount = localFrameCount;
            NetworkFrameCount = networkFrameCount;
            AimDirection = aimDir;
            ServerRecivedInputThisFrame = false;
            PlayerID = playerID;
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
        #endregion
    }
}
