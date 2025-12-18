using Drive;
using Drive.Serialization;
using ECS;
using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Pool;
using Utility;
using Component = ECS.Component;

namespace InputSystemNameSpace {
    public class InputSystem : ISystem {
        List<Component> inputComponents;
        FrameInputData cache;//本地当前帧输入缓存
        DeQueue<RecivedNetworkPlayerInputsEventData> recivedNetworkPlayerInputsEventDatas;

        private InputMappingConfig configCache;
        private InputMappingConfig Config {
            get {
                if(configCache == null)
                    configCache = Resources.Load<InputMappingConfig>(InputMappingConfig.AssetPath);
                return configCache;
            }
        }

        #region System Override
        public int Order => 0;
        public void OnInit(World world) {
            // 初始化
            inputComponents = ListPool<Component>.Get();
            GlobalEventCenter.Instance.Listen<IRecivedNetworkPlayerInputsEventData>(OnRecivedNetworkPlayerInputsEventData);
            recivedNetworkPlayerInputsEventDatas = new();
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
            cache.LocalizedLocalLogicFrameCount = localFrameCount;
            cache.KeyCodeinputs = currentInput;
            cache.AimDirection = CursorAimer.Instance.AimDirection;
            NetworkManager.Instance.SendNetworkMessage(new NetworkMessage() {
                NetworkMessageType = NetworkMessageType.PlayerInputsMessage,
                DataStream = ProtobufSerializer.Serialize(new NetworkPlayerInputsUpLinkMessage() {
                    PlayerID = NetworkManager.Instance.LocalPlayerID,
                    Input = cache
                })
            });

            //预测
            world.GetComponents(ComponentTypeEnum.InputComponent,inputComponents);
            foreach(var comp in inputComponents) {
                if(comp is InputComponent input) {
                    input.LogicUpdate(cache,localFrameCount);
                }
            }

            inputComponents.Clear();
        }

        public void OnFrameLateUpdate(World world,int localFrameCount) {
            // 帧末更新
        }

        public void OnNetworkUpdate(World world,int networkFrameCount) {

        }

        public void OnDestroy(World world) {
            GlobalEventCenter.Instance.CancelListen<IRecivedNetworkPlayerInputsEventData>(OnRecivedNetworkPlayerInputsEventData);
            ListPool<Component>.Release(inputComponents);
        }
        #endregion

        public bool IsPredictCorrect(World world,out int errorStartFrameCount) {
            lock(recivedNetworkPlayerInputsEventDatas) {
                if(recivedNetworkPlayerInputsEventDatas.Count == 0) {
                    errorStartFrameCount = -1;
                    return true;
                }

                errorStartFrameCount = -1;
                bool error = false;
                var list = ListPool<Component>.Get();
                world.GetComponents(ComponentTypeEnum.InputComponent,list);
                int playerCount = recivedNetworkPlayerInputsEventDatas.PeekBack().NetworkPlayerInputsDownLinkMessage.Inputs.Length;
                int inputCount = recivedNetworkPlayerInputsEventDatas.Count;
                FrameInputData[/*PlayerCount*/][/*InputDataCount*/] authoritivePlayerInputDatas = new FrameInputData[playerCount][];
                for(int i = 0; i < playerCount; i++) {
                    authoritivePlayerInputDatas[i] = new FrameInputData[inputCount];
                    for(int j = 0; j < inputCount; j++) {
                        authoritivePlayerInputDatas[i][j] = recivedNetworkPlayerInputsEventDatas[j].NetworkPlayerInputsDownLinkMessage.Inputs[i];
                    }
                }

                foreach(var comp in list) {
                    if(comp is InputComponent inputComponent) {
                        if(!inputComponent.IsPredictCorrect(authoritivePlayerInputDatas[inputComponent.PlayerID],out var errorFrame)) {
                            if(!error) {
                                errorStartFrameCount = errorFrame;
                                error = true;
                            } else {
                                errorStartFrameCount = Mathf.Min(errorStartFrameCount,errorFrame);
                            }
                        }
                    }
                }

                ListPool<Component>.Release(list);
                recivedNetworkPlayerInputsEventDatas.Clear();
                return !error;
            }
        }

        public void OnRollingBackState(World world) { 
            var list = ListPool<Component>.Get();
            world.GetComponents(ComponentTypeEnum.InputComponent,list);
            foreach(var comp in list) {
                if(comp is InputComponent inputComponent) {
                    inputComponent.SimulateInputWhenRollingBackState();
                }
            }
        }

        private void OnRecivedNetworkPlayerInputsEventData(IRecivedNetworkPlayerInputsEventData eventData) {
            lock(recivedNetworkPlayerInputsEventDatas) {
                recivedNetworkPlayerInputsEventDatas.PushBack((RecivedNetworkPlayerInputsEventData)eventData);
            }
        }
    }
}
