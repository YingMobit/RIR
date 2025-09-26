using System;
using System.Collections.Generic;

namespace ECS {
    /// <summary>
    /// �Ƿ�������أ����ͬһ ComponentTypeEnum ���ͣ���һ�������� Component����ʵ����
    /// ʹ�ò�λ���� (index) ���ʣ��ڲ����� slot������֤����˳���ȶ���
    /// </summary>
    public sealed class ComponentPool {
        private readonly ComponentTypeEnum _componentTypeEnum;
        private readonly Type _concreteType;

        private readonly List<Component> _components;      // ��λ������ʵ��
        private readonly Stack<int> _freeIndices;          // ���в�λ
        private bool[] _activeFlags;                       // ���Ƿ��Ծ
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
        /// ����һ�����ʵ������ʵ�塣���ظ�ʵ���������λ������
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
        /// �黹���ʵ��������������ڸóؽ����쳣��
        /// </summary>
        public void ReleaseInstance(Component component,Entity entity,int index) {
            if(component == null)
                return;

            // ���Բ��ң���ѡ������Ҫ�������ܣ���Ϊ Component ������� PoolSlotIndex �ֶ��� O(1) ���գ�

            if(!_activeFlags[index])
                return;

            component.Reset(entity);
            _activeFlags[index] = false;
            _activeCount--;
            _freeIndices.Push(index);
        }

        /// <summary>
        /// ���ݲ�λ����ȡ�û�Ծ���ʵ��������ǻ�Ծ���� null��
        /// </summary>
        public Component GetActiveInstance(int index) {
            if((uint)index >= (uint)_components.Count)
                return null;
            if(!_activeFlags[index])
                return null;
            return _components[index];
        }

        /// <summary>
        /// ���ص�ǰ���л�Ծ����������б���
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