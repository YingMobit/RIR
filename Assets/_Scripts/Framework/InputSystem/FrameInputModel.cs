using System;
using UnityEngine;

namespace InputSystem {
    [Serializable]
    public struct FrameInputModel {
        public int KeyCodeinputs;
        public int FrameCount;
        public Vector3 AimDirections;

        public static FrameInputModel Null;

        static FrameInputModel() {
            Null = new();
            Null.KeyCodeinputs = 0;
            Null.FrameCount = -1;
            Null.AimDirections = Vector3.zero;
        }

        public FrameInputModel(InputTypeEnum input,int frameCount,Vector3 aimDir) {
            KeyCodeinputs = input.InputTypeToInt();
            FrameCount = frameCount;
            AimDirections = aimDir;
        }
    }
}
