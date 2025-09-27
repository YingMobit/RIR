using ECS;
using UnityEngine;

public class NewComponent : ECS.Component
{
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.NewComponent;
    public override void OnAttach(Entity entity) {
        // 初始化组件
    }
    public override void Reset(Entity entity) {
        // 重置组件状态
    }
    // 其他字段和方法
}
