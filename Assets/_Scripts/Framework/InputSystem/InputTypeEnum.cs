using System;
using Unity.VisualScripting;
using UnityEngine;

namespace InputSystem { 
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

        public static bool HasInputType(this InputTypeEnum type,InputTypeEnum expectedInput) {
            return (type & expectedInput) == expectedInput;
        }
    }
}