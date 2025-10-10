using System;
using UnityEngine;

namespace InputSystemNameSpace {
    [Serializable]
    public struct FrameInputData {
        public int KeyCodeinputs;
        public int LocalFrameCount;
        public int NetworkFrameCount;
        public Vector3 AimDirections;

        public static FrameInputData Null;

        static FrameInputData() {
            Null = new();
            Null.KeyCodeinputs = 0;
            Null.LocalFrameCount = -1;
            Null.NetworkFrameCount = -1;
            Null.AimDirections = Vector3.zero;
        }

        public FrameInputData(InputTypeEnum input,int localFrameCount,int networkFrameCount,Vector3 aimDir) {
            KeyCodeinputs = input.InputTypeToInt();
            LocalFrameCount = localFrameCount;
            NetworkFrameCount = networkFrameCount;
            AimDirections = aimDir;
        }
    }
}
