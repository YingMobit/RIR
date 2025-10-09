using System;

namespace ECS {
    /// <summary>
    /// �Ƿ�������ع�������ʹ�����鰴 ComponentTypeEnum.GetIndex() ��Ÿ����ͳء�
    /// �ⲿ��ͨ��ԭ API ���� GetComponentPool&lt;TComponent&gt; ��ȡ�����������ͱ�Ϊ�Ƿ��� ComponentPool��
    /// </summary>
    public sealed class ComponentPoolManager {
        private ComponentPool[] _pools;      // ����������

        public ComponentPoolManager() {
            _pools = new ComponentPool[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];
        }

        public ComponentPool GetComponentPool(ComponentTypeEnum componentType) {
            uint idx = componentType.GetIndex();

            var pool = _pools[idx];
            if(pool == null) {
                pool = new ComponentPool();
                pool.Init(componentType);
                _pools[idx] = pool;
            }
            return pool;
        }

        public void OnDestroy() {
            foreach(var pool in _pools) {
                pool.OnDestroy();
            }
            Array.Clear(_pools,0,_pools.Length);
            _pools = null;
        }
    }
}