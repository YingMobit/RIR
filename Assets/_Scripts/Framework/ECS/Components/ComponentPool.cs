using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    /// <summary>
    /// ComponentPool 使用稠密/稀疏集合模式管理组件槽位：
    /// - components 列表存所有槽（slot 0 保留为无效占位）
    /// - freeComponentIndexStack 管理空闲槽（pop 分配）
    /// - activeComponentIndexStack 管理活跃槽的稠密列表（遍历仅访问 activeCount 个槽）
    /// - indexOfActiveComponentInStack 保存槽位在 activeComponentIndexStack 中的位置（-1 表示不在活跃中）
    /// 这样可以在不遍历全部 components 的情况下高效返回活跃组件列表。
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
        private int[] indexOfActiveComponentInStack; // -1 表示非活跃
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
        /// 申请一个组件实例并绑定实体。返回该实例，输出槽位索引。
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
        /// 归还组件实例。若组件不属于该池将记录错误并返回。
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
        /// 根据槽位索引取得活跃组件实例；如果非活跃返回 null。
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
        /// 返回当前所有活跃组件（复制列表），高效：只遍历 activeCount 个槽位。
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