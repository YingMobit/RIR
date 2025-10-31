using Drive;
using Drive.Serialization;
using ECS;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Pool;
using Component = ECS.Component;

namespace InputSystemNameSpace {
    public class InputSystem : ISystem {
        List<Component> inputComponents;
        FrameInputData cache;//本地当前帧输入缓存
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
            GlobalEventCenter.Instance.Listen<IRecivedNetworkPlayerInputsEventData>(OnRecivedNetworkPlayerInputsEventData);
            recivedNetworkPlayerInputsEventDatas = new ();
        }

        public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
            int currentInput = 0;
            foreach(var pair in Config.Mapping) {
                if(Input.GetKey(pair.KeyCode)) {
                    currentInput |= pair.InputTypeEnum.InputTypeToInt();
                }
            }
            cache.PlayerID = NetworkManager.Instance.LocalPlayerID;
            cache.AuthorityLocalLogicFrameCount = localFrameCount;
            cache.KeyCodeinputs = currentInput;
            cache.AimDirection = CursorAimer.Instance.AimDirection;
            NetworkManager.Instance.SendNetworkMessage(new NetworkMessage() { NetworkMessageType = NetworkMessageType.PlayerInputsMessage,
                                                                                     DataStream = ProtobufSerializer.Serialize(new NetworkPlayerInputsUpLinkMessage() { PlayerID = NetworkManager.Instance.LocalPlayerID,
                                                                                                                                                                            Input = cache }) });
            
            //预测回滚

        }

        public void OnFrameLateUpdate(World world,int localFrameCount) {
            // 帧末更新
            inputComponents.Clear();
        }

        public void OnNetworkUpdate(World world,int networkFrameCount) {
            
        }

        public void OnDestroy(World world) {
            GlobalEventCenter.Instance.CancelListen<IRecivedNetworkPlayerInputsEventData>(OnRecivedNetworkPlayerInputsEventData);
            ListPool<Component>.Release(inputComponents);
        }

        private void OnRecivedNetworkPlayerInputsEventData(IRecivedNetworkPlayerInputsEventData eventData) {
            lock(recivedNetworkPlayerInputsEventDatas) { 
                recivedNetworkPlayerInputsEventDatas.Enqueue((RecivedNetworkPlayerInputsEventData)eventData);
            }
        }
    }
}
