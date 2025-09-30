using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    /// <summary>
    /// ComponentPool ʹ�ó���/ϡ�輯��ģʽ���������λ��
    /// - components �б�����вۣ�slot 0 ����Ϊ��Чռλ��
    /// - freeComponentIndexStack ������вۣ�pop ���䣩
    /// - activeComponentIndexStack �����Ծ�۵ĳ����б����������� activeCount ���ۣ�
    /// - indexOfActiveComponentInStack �����λ�� activeComponentIndexStack �е�λ�ã�-1 ��ʾ���ڻ�Ծ�У�
    /// ���������ڲ�����ȫ�� components ������¸�Ч���ػ�Ծ����б�
    /// </summary>
    public sealed class ComponentPool {
        public const int DEFAULT_BUCKET_CAPACITY = 64;

        private ComponentTypeEnum componentTypeEnum;
        private List<Component> components;

        // free stack (indices)
        private uint[] freeComponentIndexStack;
        private uint freeCount;

        // active dense stack (indices) and mapping index->pos
        private uint[] activeComponentIndexStack;
        private int[] indexOfActiveComponentInStack; // -1 ��ʾ�ǻ�Ծ
        private uint activeCount;

        private Component componentTemplate;

        public int ActiveComponentCount => (int)activeCount;
        public int FreeComponentCount => (int)freeCount;
        public int TotalComponentCount => components.Count;

        public void Init(ComponentTypeEnum componentTypeEnum) {
            this.componentTypeEnum = componentTypeEnum;

            components = new List<Component>(DEFAULT_BUCKET_CAPACITY);

            // template instance
            componentTemplate = (Component)Activator.CreateInstance(ComponentTypeEnumExtension.COMPONENT_TYPE_MAPPING[componentTypeEnum.GetIndex()]);

            // ensure arrays
            freeComponentIndexStack = new uint[DEFAULT_BUCKET_CAPACITY];
            activeComponentIndexStack = new uint[DEFAULT_BUCKET_CAPACITY];
            indexOfActiveComponentInStack = new int[DEFAULT_BUCKET_CAPACITY];

            // reserve index 0 (invalid component)
            components.Add(componentTemplate.SetComponentID(0));

            // initialize mapping
            for(int i = 0; i < indexOfActiveComponentInStack.Length; i++)
                indexOfActiveComponentInStack[i] = -1;

            // fill free stack with 1..capacity-1
            freeCount = 0;
            for(uint i = 1; i < DEFAULT_BUCKET_CAPACITY; i++) {
                var inst = componentTemplate.Clone().SetComponentID(i);
                components.Add(inst);
                freeComponentIndexStack[freeCount++] = i;
            }

            activeCount = 0;
        }

        private void EnsureCapacityForIndex(uint desiredIndex) {
            if(desiredIndex < components.Count)
                return;
            // grow by DEFAULT_BUCKET_CAPACITY
            uint oldCount = (uint)components.Count;
            int newSize = components.Count + DEFAULT_BUCKET_CAPACITY;

            components.Capacity = newSize;
            // resize arrays
            Array.Resize(ref freeComponentIndexStack,newSize);
            Array.Resize(ref activeComponentIndexStack,newSize);
            Array.Resize(ref indexOfActiveComponentInStack,newSize);

            // init new mapping entries and push new free indices
            for(uint i = oldCount; i < (uint)newSize; i++) {
                var inst = componentTemplate.Clone().SetComponentID(i);
                components.Add(inst);
                indexOfActiveComponentInStack[i] = -1;
                // push into free stack
                freeComponentIndexStack[freeCount++] = i;
            }
        }

        private void ExpandPool() {
            EnsureCapacityForIndex((uint)components.Count + (uint)DEFAULT_BUCKET_CAPACITY);
        }

        /// <summary>
        /// ����һ�����ʵ������ʵ�塣���ظ�ʵ���������λ������
        /// </summary>
        public Component GetInstance(Entity entity,out uint index) {
            if(freeCount == 0) {
                ExpandPool();
            }
            // pop free index
            index = freeComponentIndexStack[--freeCount];

            // mark active: push to active stack
            activeComponentIndexStack[activeCount] = index;
            indexOfActiveComponentInStack[index] = (int)activeCount;
            activeCount++;

            var comp = components[(int)index];
            comp.OnAttach(entity);
            return comp;
        }

        /// <summary>
        /// �黹���ʵ��������������ڸóؽ���¼���󲢷��ء�
        /// </summary>
        public void ReleaseInstance(Component component,Entity entity) {
            uint index = component.ComponentID;
            if(index == 0 || index >= (uint)components.Count) {
                Debug.LogError($"index out of range:{index}");
                return;
            }
            if(component.ComponentType != componentTypeEnum) {
                Debug.LogError($"Component Type mismatch,wanted:{componentTypeEnum}, actual: {component.ComponentType}");
                return;
            }

            int pos = indexOfActiveComponentInStack[index];
            if(pos < 0) {
                Debug.LogError($"Component {index} is not active");
                return;
            }

            // swap-back remove from active stack
            int lastPos = (int)activeCount - 1;
            uint lastIndex = activeComponentIndexStack[lastPos];
            activeComponentIndexStack[pos] = lastIndex;
            indexOfActiveComponentInStack[lastIndex] = pos;

            // clear removed
            indexOfActiveComponentInStack[index] = -1;
            activeCount--;

            // reset and push to free stack
            component.Reset(entity);
            freeComponentIndexStack[freeCount++] = index;
        }

        /// <summary>
        /// ���ݲ�λ����ȡ�û�Ծ���ʵ��������ǻ�Ծ���� null��
        /// </summary>
        public Component GetActiveInstance(uint index) {
            if(index == 0 || index >= (uint)components.Count) {
                Debug.LogError("Zero or out-of-range index");
                return null;
            }
            if(indexOfActiveComponentInStack[index] < 0) {
                Debug.LogError($"this component is not active:{index}");
                return null;
            }
            return components[(int)index];
        }

        /// <summary>
        /// ���ص�ǰ���л�Ծ����������б�����Ч��ֻ���� activeCount ����λ��
        /// </summary>
        public void GetAllActiveComponents(in List<Component> _components) {
            _components.Clear();
            _components.Capacity = Math.Max(_components.Capacity,(int)activeCount);
            for(int i = 0; i < (int)activeCount; i++) {
                uint idx = activeComponentIndexStack[i];
                _components.Add(components[(int)idx]);
            }
        }
    }
}