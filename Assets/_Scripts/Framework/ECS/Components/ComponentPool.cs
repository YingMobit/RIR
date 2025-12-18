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

        private ComponentTypeEnum _componentTypeEnum;
        private List<Component> _components;

        // free stack (indices)
        private uint[] _freeComponentIndexStack;
        private uint _freeCount;

        // active dense stack (indices) and mapping index->pos
        private uint[] _activeComponentIndexStack;
        private int[] _indexOfActiveComponentInStack; // -1 表示非活跃
        private uint _activeCount;

        private Component _componentTemplate;

        public int ActiveComponentCount => (int)_activeCount;
        public int FreeComponentCount => (int)_freeCount;
        public int TotalComponentCount => _components.Count;

        public void Init(ComponentTypeEnum componentTypeEnum) {
            this._componentTypeEnum = componentTypeEnum;

            _components = new List<Component>(DEFAULT_BUCKET_CAPACITY);

            // template instance
            _componentTemplate = (Component)Activator.CreateInstance(ComponentTypeEnumExtension.COMPONENT_TYPE_MAPPING[componentTypeEnum.GetIndex()]);

            // ensure arrays
            _freeComponentIndexStack = new uint[DEFAULT_BUCKET_CAPACITY];
            _activeComponentIndexStack = new uint[DEFAULT_BUCKET_CAPACITY];
            _indexOfActiveComponentInStack = new int[DEFAULT_BUCKET_CAPACITY];

            // reserve index 0 (invalid component)
            _components.Add(_componentTemplate.SetComponentID(0));

            // initialize mapping
            for(int i = 0; i < _indexOfActiveComponentInStack.Length; i++)
                _indexOfActiveComponentInStack[i] = -1;

            // fill free stack with 1..capacity-1
            _freeCount = 0;
            for(uint i = 1; i < DEFAULT_BUCKET_CAPACITY; i++) {
                var inst = _componentTemplate.GetNewInstance().SetComponentID(i);
                _components.Add(inst);
                _freeComponentIndexStack[_freeCount++] = i;
            }

            _activeCount = 0;
        }

        private void EnsureCapacityForIndex(uint desiredIndex) {
            if(desiredIndex < _components.Count)
                return;
            // grow by DEFAULT_BUCKET_CAPACITY
            uint oldCount = (uint)_components.Count;
            int newSize = _components.Count + DEFAULT_BUCKET_CAPACITY;

            _components.Capacity = newSize;
            // resize arrays
            Array.Resize(ref _freeComponentIndexStack,newSize);
            Array.Resize(ref _activeComponentIndexStack,newSize);
            Array.Resize(ref _indexOfActiveComponentInStack,newSize);

            // init new mapping entries and push new free indices
            for(uint i = oldCount; i < (uint)newSize; i++) {
                var inst = _componentTemplate.GetNewInstance().SetComponentID(i);
                _components.Add(inst);
                _indexOfActiveComponentInStack[i] = -1;
                // push into free stack
                _freeComponentIndexStack[_freeCount++] = i;
            }
        }

        private void ExpandPool() {
            EnsureCapacityForIndex((uint)_components.Count + DEFAULT_BUCKET_CAPACITY);
        }

        /// <summary>
        /// 申请一个组件实例并绑定实体。返回该实例，输出槽位索引。
        /// </summary>
        public Component GetInstance(World world, Entity entity,out uint index) {
            if(_freeCount == 0) {
                ExpandPool();
            }
            // pop free index
            index = _freeComponentIndexStack[--_freeCount];

            // mark active: push to active stack
            _activeComponentIndexStack[_activeCount] = index;
            _indexOfActiveComponentInStack[index] = (int)_activeCount;
            _activeCount++;

            var comp = _components[(int)index];
            comp.OnAttach(world,entity);
            return comp;
        }

        /// <summary>
        /// 归还组件实例。若组件不属于该池将记录错误并返回。
        /// </summary>
        public void ReleaseInstance(World world,Component component,Entity entity) {
            uint index = component.ComponentID;
            if(index == 0 || index >= (uint)_components.Count) {
                Debug.LogError($"index out of range:{index}");
                return;
            }
            if(component.ComponentType != _componentTypeEnum) {
                Debug.LogError($"Component Type mismatch,wanted:{_componentTypeEnum}, actual: {component.ComponentType}");
                return;
            }

            int pos = _indexOfActiveComponentInStack[index];
            if(pos < 0) {
                Debug.LogError($"Component {index} is not active");
                return;
            }

            // swap-back remove from active stack
            int lastPos = (int)_activeCount - 1;
            uint lastIndex = _activeComponentIndexStack[lastPos];
            _activeComponentIndexStack[pos] = lastIndex;
            _indexOfActiveComponentInStack[lastIndex] = pos;

            // clear removed
            _indexOfActiveComponentInStack[index] = -1;
            _activeCount--;

            // reset and push to free stack
            component.Reset(world,entity);
            _freeComponentIndexStack[_freeCount++] = index;
        }

        /// <summary>
        /// 根据槽位索引取得活跃组件实例；如果非活跃返回 null。
        /// </summary>
        public Component GetActiveInstance(uint index) {
            if(index == 0 || index >= (uint)_components.Count) {
                Debug.LogError("Zero or out-of-range index");
                return null;
            }
            if(_indexOfActiveComponentInStack[index] < 0) {
                Debug.LogError($"this component is not active:{index}");
                return null;
            }
            return _components[(int)index];
        }

        /// <summary>
        /// 返回当前所有活跃组件（复制列表），高效：只遍历 activeCount 个槽位。
        /// </summary>
        public void GetAllActiveComponents(in List<Component> components) {
            components.Clear();
            components.Capacity = Math.Max(components.Capacity,(int)_activeCount);
            for(int i = 0; i < (int)_activeCount; i++) {
                uint idx = _activeComponentIndexStack[i];
                components.Add(this._components[(int)idx]);
            }
        }

        public void OnDestroy() {
            foreach(var compo in _components) {
                compo.OnDestroy();
            }

            _components.Clear();
            _components = null;
            Array.Clear(_freeComponentIndexStack,0,_freeComponentIndexStack.Length);
            _freeComponentIndexStack = null;
            Array.Clear(_activeComponentIndexStack,0,_activeComponentIndexStack.Length);
            _activeComponentIndexStack = null;
            Array.Clear(_indexOfActiveComponentInStack,0,_indexOfActiveComponentInStack.Length);
            _indexOfActiveComponentInStack = null;
            _componentTemplate.OnDestroy();
            _componentTemplate = null;
        }
    }
}