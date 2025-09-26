using System;
using System.Collections.Generic;

namespace ECS {
    /// <summary>
    /// 非泛型组件池：存放同一 ComponentTypeEnum 类型（单一具体派生 Component）的实例。
    /// 使用槽位索引 (index) 访问；内部复用 slot，不保证遍历顺序稳定。
    /// </summary>
    public sealed class ComponentPool {
        private readonly ComponentTypeEnum _componentTypeEnum;
        private readonly Type _concreteType;

        private readonly List<Component> _components;      // 槽位存放组件实例
        private readonly Stack<int> _freeIndices;          // 空闲槽位
        private bool[] _activeFlags;                       // 槽是否活跃
        private int _activeCount;

        public ComponentTypeEnum ComponentTypeEnum => _componentTypeEnum;
        public Type ConcreteType => _concreteType;

        public int ActiveCount => _activeCount;
        public int InActiveCount => _freeIndices.Count;
        public int TotalCount => _components.Count;

        public ComponentPool(ComponentTypeEnum componentTypeEnum,Type concreteType,int initialCapacity) {
            if(!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} is not a Component",nameof(concreteType));

            _componentTypeEnum = componentTypeEnum;
            _concreteType = concreteType;

            if(initialCapacity < 0)
                initialCapacity = 0;
            _components = new List<Component>(initialCapacity);
            _freeIndices = new Stack<int>(initialCapacity);
            _activeFlags = new bool[initialCapacity > 0 ? initialCapacity : 4];

            Prewarm(initialCapacity);
        }

        private void Prewarm(int count) {
            for(int i = 0; i < count; i++) {
                var inst = (Component)Activator.CreateInstance(_concreteType);
                _components.Add(inst);
                _freeIndices.Push(i);
            }
        }

        private void EnsureCapacity(int desired) {
            if(desired <= _components.Count)
                return;
            int oldCap = _components.Count;
            int growTo = Math.Max(desired,oldCap == 0 ? 4 : oldCap * 2);

            if(growTo > _activeFlags.Length)
                Array.Resize(ref _activeFlags,growTo);

            for(int i = oldCap; i < growTo; i++) {
                var inst = (Component)Activator.CreateInstance(_concreteType);
                _components.Add(inst);
                _freeIndices.Push(i);
            }
        }

        /// <summary>
        /// 申请一个组件实例并绑定实体。返回该实例，输出槽位索引。
        /// </summary>
        public Component GetInstance(Entity entity,out int index) {
            if(_freeIndices.Count == 0)
                EnsureCapacity(_components.Count + 1);

            index = _freeIndices.Pop();
            var comp = _components[index];
            _activeFlags[index] = true;
            _activeCount++;
            comp.OnAttach(entity);
            return comp;
        }

        /// <summary>
        /// 归还组件实例。若组件不属于该池将抛异常。
        /// </summary>
        public void ReleaseInstance(Component component,Entity entity,int index) {
            if(component == null)
                return;

            // 线性查找（可选：若需要更高性能，可为 Component 基类添加 PoolSlotIndex 字段以 O(1) 回收）

            if(!_activeFlags[index])
                return;

            component.Reset(entity);
            _activeFlags[index] = false;
            _activeCount--;
            _freeIndices.Push(index);
        }

        /// <summary>
        /// 根据槽位索引取得活跃组件实例；如果非活跃返回 null。
        /// </summary>
        public Component GetActiveInstance(int index) {
            if((uint)index >= (uint)_components.Count)
                return null;
            if(!_activeFlags[index])
                return null;
            return _components[index];
        }

        /// <summary>
        /// 返回当前所有活跃组件（复制列表）。
        /// </summary>
        public List<Component> GetAllActiveComponents() {
            var res = new List<Component>(_activeCount);
            for(int i = 0; i < _components.Count; i++) {
                if(_activeFlags[i])
                    res.Add(_components[i]);
            }
            return res;
        }
    }
}