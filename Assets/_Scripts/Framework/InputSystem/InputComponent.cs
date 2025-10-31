using ECS;
using UnityEngine;
using Component = ECS.Component;

namespace InputSystemNameSpace {
    public class InputComponent : Component {
        public int PlayerID { get; private set; }
        public InputQueue UnconfirmedInputDataBuffer { get; private set; } = new InputQueue();

        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.InputComponent;

        public void BindPlayerID(int playerID) { 
            PlayerID = playerID;
        }

        public override Component Clone() {
            return new InputComponent() { UnconfirmedInputDataBuffer = this.UnconfirmedInputDataBuffer.Clone() };
        }

        public override void OnAttach(Entity entity) {
            
        }

        public override void OnDestroy() {
            UnconfirmedInputDataBuffer.OnDestroy();
        }

        public override void Reset(Entity entity) {
            UnconfirmedInputDataBuffer.Reset();
        }

        public bool ConfirmeInputData(FrameInputData authoritativeInputData) {
            while(UnconfirmedInputDataBuffer.TryPeekHead(out var tobeComfirme)) {
                if(tobeComfirme.LocalizedLocalLogicFrameCount == authoritativeInputData.LocalizedLocalLogicFrameCount) {
                    if(tobeComfirme.MoveInput == authoritativeInputData.MoveInput) {
                        return true;
                    } else {
                        return false;
                    }
                } else if(tobeComfirme.LocalizedLocalLogicFrameCount > authoritativeInputData.LocalizedLocalLogicFrameCount) {
                    Debug.LogError($"this authoritativeInputData has been confirmed,localframe of oldestUnconfirmedInputData:{tobeComfirme.LocalizedLocalLogicFrameCount},locaframe of authoritativeInputData:{authoritativeInputData.LocalizedLocalLogicFrameCount}");
                    return true;
                } else {
                    Debug.LogWarning("jumped authoritativeInputData,localframe of oldestUnconfirmedInputData:{tobeComfirme.LocalizedLocalLogicFrameCount},locaframe of authoritativeInputData:{authoritativeInputData.LocalizedLocalLogicFrameCount}");
                }
            }
            Debug.LogError("No unconfirmed input data available");
            return true;
        }
    }
}
