using System;
using Unity.VisualScripting;
using UnityEngine;

namespace InputSystemNameSpace { 
    [Flags]
    [Serializable]
    public enum InputTypeEnum{ 
        None            = 0,       
        MoveForward     = 1,
        MoveBackward    = 1 << 1,
        MoveLeft        = 1 << 2,
        MoveRight       = 1 << 3,
        Jump            = 1 << 4,
        MainAbility     = 1 << 5,
        SubAbility      = 1 << 6,
        MovementAbility = 1 << 7,
        UltimateAbility = 1 << 8,
        UseProp         = 1 << 9,
        Interact        = 1 << 10
    }

    public static class InputTypeEnumExtension { 
        public static InputTypeEnum IntToInputType(this int value) {
            return (InputTypeEnum)value;
        }

        public static int InputTypeToInt(this InputTypeEnum value) {
            return (int)value;
        }

        public static bool HasAllInputType(this int type,InputTypeEnum expectedInput) {
            return (type & (int)expectedInput) == (int)expectedInput;
        }

        public static bool HasAnyInputType(this int type,InputTypeEnum expectedInput) {
            return (type & (int)expectedInput) != 0;
        }
    }
}