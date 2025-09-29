namespace ECS {
    public abstract class Component {
        public uint ComponentID { get; private set; }
        public abstract ComponentTypeEnum ComponentType { get; }
        public abstract void OnAttach(Entity entity);
        public abstract void Reset(Entity entity);
        public abstract Component Clone();

        internal Component SetComponentID(uint id) {
            ComponentID = id;
            return this;
        }
    }
}
