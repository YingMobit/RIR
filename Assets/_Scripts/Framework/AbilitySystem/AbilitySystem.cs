using ECS;

public class AbilitySystem : ISystem {
    public int Order => 1;

    public void OnInit(World world) {
        // 初始化
    }

    public void OnFrameUpdate(World world,float deltaTime) {
        // 每帧更新
    }

    public void OnFrameLateUpdate(World world) {
        // 帧末更新
    }
    
    public void OnNetworkUpdate(World world, int networkFrameCount){
    
    }

    public void OnDestroy(World world){
    }
}
