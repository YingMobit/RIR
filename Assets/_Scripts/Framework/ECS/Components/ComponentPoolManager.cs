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
    }
}