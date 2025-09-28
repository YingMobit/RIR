using ECS;
public class Test1 : ECS.Component {
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.Test1;

    public override ECS.Component Clone() {
        return new Test1();
    }

    public override void OnAttach(Entity entity) {
        // 初始化组�?
    }
    public override void Reset(Entity entity) {
        // 重置组件状�?
    }
    // 其他字段和方�?
}
