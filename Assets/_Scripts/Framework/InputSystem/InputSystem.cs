using ECS;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using Component = ECS.Component;

namespace InputSystemNameSpace {
    public class InputSystem : ISystem {
        List<Component> inputComponents;
        FrameInputData cache;
        int networkFrameCount;

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
            inputComponents = ListPool<Component>.Get();
        }

        public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
            int currentInput = 0;
            foreach(var pair in Config.Mapping) {
                if(Input.GetKey(pair.KeyCode)) {
                    currentInput |= pair.InputTypeEnum.InputTypeToInt();
                }
            }
            cache.LocalFrameCount = localFrameCount;
            cache.NetworkFrameCount = networkFrameCount;
            cache.KeyCodeinputs = currentInput;
            cache.AimDirection = CursorAimer.Instance.AimDirection;

            world.GetComponents(ComponentTypeEnum.InputComponent,inputComponents);
            foreach(var input in inputComponents) {
                (input as InputComponent).InputQueue.EnQueue(cache);
            }
        }

        public void OnFrameLateUpdate(World world,int localFrameCount) {
            // 帧末更新
            inputComponents.Clear();
        }

        public void OnNetworkUpdate(World world,int networkFrameCount) {
            this.networkFrameCount = networkFrameCount;
        }

        public void OnDestroy(World world) {
            ListPool<Component>.Release(inputComponents);
        }
    }
}
