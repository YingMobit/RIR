using System;

namespace ECS {
    /// <summary>
    /// 非泛型组件池管理器：使用数组按 ComponentTypeEnum.GetIndex() 存放各类型池。
    /// 外部仍通过原 API 名称 GetComponentPool&lt;TComponent&gt; 获取，但返回类型变为非泛型 ComponentPool。
    /// </summary>
    public sealed class ComponentPoolManager {
        private ComponentPool[] _pools;      // 按类型索引

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