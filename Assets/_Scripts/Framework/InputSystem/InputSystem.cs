using ECS;
using InputSystemNameSpace;
using UnityEngine;
namespace InputSystemNameSpace {
    public class InputSystem : ISystem {

        private InputMappingConfig configCache;
        private InputMappingConfig Config {
            get {
                if(configCache == null)
                    configCache = Resources.Load<InputMappingConfig>(InputMappingConfig.AssetPath);
                return configCache;
            }
        }
        public int Order => 0;

        public void OnInit(World world) {
            // 初始化
        }

        public void OnFrameUpdate(World world,float deltaTime) {
            // 每帧更新
        }

        public void OnFrameLateUpdate(World world) {
            // 帧末更新
        }

        public void OnNetworkUpdate(World world,int networkFrameCount) {

        }

        public void OnDestroy(World world) {
        }
    }
}
