using InputSystem;
using System;
using UnityEngine;

namespace InputSystem{
    [CreateAssetMenu(fileName = "InputMappingConfig",menuName = "ScriptableObject/InputMappingConfig",order = 0)]
    public class InputMappingConfig : ScriptableObject {
        [Serializable]
        public struct InputMapping {
            public InputTypeEnum InputTypeEnum;
            public KeyCode KeyCode;
            
            public InputMapping(InputTypeEnum InputTypeEnum,KeyCode KeyCode) {
                this.InputTypeEnum = InputTypeEnum;
                this.KeyCode = KeyCode;
            }
        }

        public InputMapping[] Mapping = new InputMapping[]{
            new(InputTypeEnum.MoveForward     ,KeyCode.W),
            new(InputTypeEnum.MoveBackward    ,KeyCode.S),
            new(InputTypeEnum.MoveLeft        ,KeyCode.A),
            new(InputTypeEnum.MoveRight       ,KeyCode.D),
            new(InputTypeEnum.Jump            ,KeyCode.Space),
            new(InputTypeEnum.MainAbility     ,KeyCode.Mouse0),
            new(InputTypeEnum.SubAbility      ,KeyCode.Mouse1),
            new(InputTypeEnum.MovementAbility ,KeyCode.Q),
            new(InputTypeEnum.UltimateAbility ,KeyCode.E),
            new(InputTypeEnum.UseProp         ,KeyCode.R),
            new(InputTypeEnum.Interact        ,KeyCode.F),
        };

        public const string AssetPath = "ScriptableObject/InputConfig";
    }
}