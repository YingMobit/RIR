using ECS;
using UnityEngine;

public class HAHAHA : ECS.Component {
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.HAHAHA;

    public override ECS.Component Clone() {
        return new HAHAHA();
    }

    public override void OnAttach(Entity entity) {
        // 初始化组�?
    }
    public override void Reset(Entity entity) {
        // 重置组件状�?
    }
    // 其他字段和方�?
}
