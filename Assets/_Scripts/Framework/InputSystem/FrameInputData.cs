using System;
using UnityEngine;

namespace InputSystemNameSpace {
    [Serializable]
    public struct FrameInputData {
        public int KeyCodeinputs;
        public int LocalFrameCount;
        public int NetworkFrameCount;
        public Vector3 AimDirection;

        public static FrameInputData Null;

        static FrameInputData() {
            Null = new();
            Null.KeyCodeinputs = 0;
            Null.LocalFrameCount = -1;
            Null.NetworkFrameCount = -1;
            Null.AimDirection = Vector3.zero;
        }

        public FrameInputData(InputTypeEnum input,int localFrameCount,int networkFrameCount,Vector3 aimDir) {
            KeyCodeinputs = input.InputTypeToInt();
            LocalFrameCount = localFrameCount;
            NetworkFrameCount = networkFrameCount;
            AimDirection = aimDir;
        }

        #region Utility
        public Vector2 MoveInput { get {
                Vector2 res = Vector2.zero;
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveForward)) { 
                    res += Vector2.up;
                }
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveBackward)) { 
                    res += Vector2.down;
                }
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveLeft)) { 
                    res += Vector2.left;
                }
                if(KeyCodeinputs.HasAllInputType(InputTypeEnum.MoveRight)) { 
                    res += Vector2.right;
                }
                return res;
            }
        }
        #endregion
    }
}
