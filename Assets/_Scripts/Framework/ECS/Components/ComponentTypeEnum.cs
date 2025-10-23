using GAS;
using InputSystemNameSpace;
using System;
using System.Collections.Generic;

namespace ECS {
    [Flags]
    public enum ComponentTypeEnum {
        AbilityComponent = 1 << 0,
        InputComponent = 1 << 1,
        TagComponent = 1 << 2,
}


    public static class ComponentTypeEnumExtension {
        public const int COMPONENT_TYPE_COUNT = 3;
        public static readonly Type[] COMPONENT_TYPE_MAPPING = new Type[COMPONENT_TYPE_COUNT]{
            typeof(AbilityComponent), // index 0
            typeof(InputComponent), // index 1
            typeof(TagComponent), // index 2
        };

        public static uint GetIndex(this ComponentTypeEnum componentType) {
            int value = (int)componentType;

            // �����ǵ����أ���ö��ֵ�� 2 ���ݣ�
            if((value & (value - 1)) != 0) {
                throw new System.ArgumentException(
                    $"ö��ֵ {componentType} = {value} ���ǵ�һ���أ�δ�� 1<<n ���壩");
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

        public static uint EnumsToMask(this ComponentTypeEnum[] componentTypes) {
            uint mask = 0;
            foreach(var type in componentTypes) {
                mask |= (uint)type;
            }
            return mask;
        }

        public static ComponentTypeEnum[] MaskToEnums(this uint componentType) {
            if(componentType == 0)
                return System.Array.Empty<ComponentTypeEnum>();

            // ��ͳ�� set bit ��
            uint temp = componentType;
            int count = 0;
            while(temp != 0) {
                temp &= (temp - 1); // ������λ 1
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
