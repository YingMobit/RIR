using Drive;

namespace ECS{ 
    public interface ISystem {
        public int Order { get; }
        public void OnInit(World world);
        public void OnFrameUpdate(World world,int localFrameCount,float deltaTime);
        public void OnFrameLateUpdate(World world,int localFrameCount);
        public void OnNetworkUpdate(World world, int networkFrameCount);
        public void OnDestroy(World world);
    }
}