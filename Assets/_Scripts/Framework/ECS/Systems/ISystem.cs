namespace ECS{ 
    public interface ISystem {
        public void OnFrameUpdate(float deltaTime);
        public void OnFrameLateUpdate();
        public void OnNetwordUpdate(int networkFrameCount);
    }
}