namespace ECS {
    public abstract class Component {
        public uint ComponentID { get; private set; }
        public abstract ComponentTypeEnum ComponentType { get; }
        public abstract void OnAttach(World world,Entity entity);
        public abstract void Reset(World world,Entity entity);
        public abstract Component GetNewInstance();
        public abstract void OnDestroy();

        internal Component SetComponentID(uint id) {
            ComponentID = id;
            return this;
        }
    }
}
