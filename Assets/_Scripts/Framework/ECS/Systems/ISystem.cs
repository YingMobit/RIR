namespace ECS{ 
    public interface ISystem {
        public int Order { get; }
        public void OnInit(World world);
        public void OnFrameUpdate(World world,float deltaTime);
        public void OnFrameLateUpdate(World world);
        public void OnNetworkUpdate(World world, int networkFrameCount);
        public void OnDestroy(World world);
    }
}