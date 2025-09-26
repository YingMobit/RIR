using System;
using System.Collections.Generic;

namespace ECS {
    public enum ComponentTypeEnum {
    }


    public static class ComponentTypeEnumExtension {
        public const int COMPONENT_TYPE_COUNT = 0;
        public static readonly Type[] COMPONENT_TYPE_MAPPING = new Type[COMPONENT_TYPE_COUNT]{ 
        };

        public static uint GetIndex(this ComponentTypeEnum componentType) {
            int value = (int)componentType;

            // 必须是单比特（即枚举值是 2 的幂）
            if((value & (value - 1)) != 0) {
                throw new System.ArgumentException(
                    $"枚举值 {componentType} = {value} 不是单一比特（未按 1<<n 定义）");
            }

            uint index = 0;
            while((value >>= 1) != 0) {
                index++;
            }
            return index;
        }

        public static uint ToMask(this ComponentTypeEnum componentType) {
            return (uint)componentType;
        }

        public static ComponentTypeEnum[] MaskToEnums(this uint componentType) {
            if(componentType == 0)
                return System.Array.Empty<ComponentTypeEnum>();

            // 先统计 set bit 数
            uint temp = componentType;
            int count = 0;
            while(temp != 0) {
                temp &= (temp - 1); // 清除最低位 1
                count++;
            }

            var result = new ComponentTypeEnum[count];
            int write = 0;
            int bitValue = 1;
            uint working = componentType;

            while(working != 0) {
                if((working & 1) != 0) {
                    result[write++] = (ComponentTypeEnum)bitValue;
                }
                working >>= 1;
                bitValue <<= 1;
            }

            return result;
        }
    }
}
