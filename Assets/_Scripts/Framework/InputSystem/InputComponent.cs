using ECS;

namespace InputSystemNameSpace {
    public class InputComponent : Component {
        public int PlayerID { get; private set; }
        public InputQueue InputQueue { get; private set; } = new InputQueue();
        public FrameInputData LatestFrameInputData => InputQueue.PeekTail();

        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.InputComponent;

        public void BindPlayerID(int playerID) { 
            PlayerID = playerID;
        }

        public override Component Clone() {
            return new InputComponent() { InputQueue = this.InputQueue.Clone() };
        }

        public override void OnAttach(Entity entity) {
            
        }

        public override void OnDestroy() {
            InputQueue.OnDestroy();
        }

        public override void Reset(Entity entity) {
            InputQueue.Reset();
        }
    }
}
