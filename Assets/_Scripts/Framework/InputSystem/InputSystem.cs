using Drive;
using ECS;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Pool;
using Component = ECS.Component;

namespace InputSystemNameSpace {
    public class InputSystem : ISystem {
        List<Component> inputComponents;
        FrameInputData cache;
        Queue<RecivedNetworkPlayerInputsEventData> recivedNetworkPlayerInputsEventDatas;

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
            GlobalEventCenter.Instance.Listen<IRecivedNetworkPlayerInputsEventData>(OnRecivedNetworkPLayerInputsEventData);
        }

        public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
            int currentInput = 0;
            foreach(var pair in Config.Mapping) {
                if(Input.GetKey(pair.KeyCode)) {
                    currentInput |= pair.InputTypeEnum.InputTypeToInt();
                }
            }
            cache.LocalFrameCount = localFrameCount;
            cache.KeyCodeinputs = currentInput;
            cache.AimDirection = CursorAimer.Instance.AimDirection;
            // NetworkManager.Instance
            if(recivedNetworkPlayerInputsEventDatas.Count > 0) {
                InputComponent inputComponent;
                List<Component> components = ListPool<Component>.Get();
                world.GetComponents( ComponentTypeEnum.InputComponent,components);
                while(recivedNetworkPlayerInputsEventDatas.TryDequeue(out var recivedNetworkPlayerInputsEventData)) {
                    foreach(var component in components) {
                        inputComponent = component as InputComponent;
                        inputComponent.InputQueue.EnQueue(recivedNetworkPlayerInputsEventData.NetworkPlayerInputsDownLinkMessage.Inputs[inputComponent.PlayerID]);
                    }
                }
            }
        }

        public void OnFrameLateUpdate(World world,int localFrameCount) {
            // 帧末更新
            inputComponents.Clear();
        }

        public void OnNetworkUpdate(World world,int networkFrameCount) {
            
        }

        public void OnDestroy(World world) {
            GlobalEventCenter.Instance.CancelListen<IRecivedNetworkPlayerInputsEventData>(OnRecivedNetworkPLayerInputsEventData);
            ListPool<Component>.Release(inputComponents);
        }

        private void OnRecivedNetworkPLayerInputsEventData(IRecivedNetworkPlayerInputsEventData eventData) {
            recivedNetworkPlayerInputsEventDatas.Enqueue((RecivedNetworkPlayerInputsEventData)eventData);
        }
    }
}
