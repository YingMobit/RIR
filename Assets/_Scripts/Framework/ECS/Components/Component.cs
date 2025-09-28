namespace ECS {
    public abstract class Component {
        public abstract ComponentTypeEnum ComponentType { get; }
        public abstract void OnAttach(Entity entity);
        public abstract void Reset(Entity entity);
        public abstract Component Clone();
    }
}
