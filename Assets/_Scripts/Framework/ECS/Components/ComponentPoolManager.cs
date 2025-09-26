using System;

namespace ECS {
    /// <summary>
    /// �Ƿ�������ع�������ʹ�����鰴 ComponentTypeEnum.GetIndex() ��Ÿ����ͳء�
    /// �ⲿ��ͨ��ԭ API ���� GetComponentPool&lt;TComponent&gt; ��ȡ�����������ͱ�Ϊ�Ƿ��� ComponentPool��
    /// </summary>
    public sealed class ComponentPoolManager {
        private const int DEFAULT_INITIAL_POOL_CAPACITY = 8;
        private ComponentPool[] _pools;      // ����������

        public ComponentPoolManager() {
            _pools = new ComponentPool[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];
        }

        public ComponentPool GetComponentPool(ComponentTypeEnum componentType) {
            uint idx = componentType.GetIndex();

            var pool = _pools[idx];
            if(pool == null) {
                pool = new ComponentPool(componentType,ComponentTypeEnumExtension.COMPONENT_TYPE_MAPPING[componentType.GetIndex()],DEFAULT_INITIAL_POOL_CAPACITY);
                _pools[idx] = pool;
            }
            return pool;
        }
    }
}