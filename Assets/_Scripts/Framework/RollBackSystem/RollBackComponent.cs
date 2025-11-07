using ECS;
using GAS;
using InputSystemNameSpace;
using Lockstep.Math;
using LockStepLMath;
using UnityEngine;
using Utility;
using Component = ECS.Component;

public class RollBackComponent : Component {
    #region Snapshot Data
    private struct SnapData {
        public int LocalizedLogicFrameCount;
        public TranformSnapData TranformSnapData;
        public RigidbodySnapData RigidbodySnapData;
        public AbilityComponentSnapData AbilityComponentSnapData;

        public SnapData(int localizedLogicFrameCount,
                        TranformSnapData tranformSnapData,
                        RigidbodySnapData rigidbodySnapData,
                        AbilityComponentSnapData abilityComponentSnapData) {
            LocalizedLogicFrameCount = localizedLogicFrameCount;
            TranformSnapData = tranformSnapData;
            RigidbodySnapData = rigidbodySnapData;
            AbilityComponentSnapData = abilityComponentSnapData;
        }
    }

    private struct TranformSnapData {
        public LVector3 Position;
        public LQuaternion Rotation;
        public LVector3 Scale;
        
        public TranformSnapData(LVector3 position,
                                LQuaternion rotation,
                                LVector3 scale) {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    private struct RigidbodySnapData {
        public LVector3 Velocity;
        public LVector3 AngularVelocity;
        
        public RigidbodySnapData(LVector3 velocity,
                                 LVector3 angularVelocity) {
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }

    public struct AbilityComponentSnapData {
        
    }

    #endregion

    const int SNAPSHOTCACHESIZE = 60;
    DeQueue<SnapData> cachedSnapShots = new(SNAPSHOTCACHESIZE);

    #region Bind
    GameObject gameObject;
    Rigidbody rigidbody;
    Transform transform;
    AbilityComponent abilityComponent;
    InputComponent inputComponent;
    #endregion

    #region Component Overrides
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.RollBackComponent;
    public override void OnAttach(World world,Entity entity) {
        // 初始化组件
        gameObject = world.GetGameObject(entity);
        rigidbody = gameObject.GetComponent<Rigidbody>();
        transform = gameObject.transform;
        world.GetComponentOnEntity(entity,ComponentTypeEnum.AbilityComponent,out var abilityComp);
        abilityComponent = abilityComp as AbilityComponent;
        world.GetComponentOnEntity(entity,ComponentTypeEnum.InputComponent,out var inputComp);
        inputComponent = inputComp as InputComponent;
        SnapShot(0);
    }

    public override void Reset(World world,Entity entity) {
        // 重置组件状态
        gameObject = null;
        rigidbody = null;
        transform = null;
        abilityComponent = null;
    }

    public override void OnDestroy() {
        // 清理组件
    }

    public override Component Clone() {
        return new RollBackComponent();
    }
    #endregion

    #region API
    public void SnapShot(int logicFrameCount) { 
        var tranformSnapData = new TranformSnapData(
            transform.position.ToLVector3(),
            transform.rotation,
            transform.localScale.ToLVector3()
        );
        var rigidbodySnapData = new RigidbodySnapData(
            rigidbody.linearVelocity.ToLVector3(),
            rigidbody.angularVelocity.ToLVector3()
        );
        var abilityComponentSnapData = new AbilityComponentSnapData(
            
        );
        var snapData = new SnapData(
            logicFrameCount,
            tranformSnapData,
            rigidbodySnapData,
            abilityComponentSnapData
        );

        cachedSnapShots.PushBack(snapData);
    }

    /// <summary>
    /// 回滚到距离当前帧最近的正确帧并清除所有错误帧快照
    /// </summary>
    /// <param name="errorStartLocalizedLogicFrameCount"></param>
    /// <param name="currentLocalizedLogicFrameCount"></param>
    public void RollBackState(int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) { 
        Debug.Log($"[RollBackComponent] RollBack from Frame {errorStartLocalizedLogicFrameCount} to Frame {currentLocalizedLogicFrameCount}");
        int count = currentLocalizedLogicFrameCount - errorStartLocalizedLogicFrameCount + 1;
        cachedSnapShots.PopBackN(count);
        var lastCorrectSnapData = cachedSnapShots.PeekBack();
        transform.position = lastCorrectSnapData.TranformSnapData.Position.ToVector3();
        transform.rotation = lastCorrectSnapData.TranformSnapData.Rotation;
        transform.localScale = lastCorrectSnapData.TranformSnapData.Scale.ToVector3();
        rigidbody.linearVelocity = lastCorrectSnapData.RigidbodySnapData.Velocity.ToVector3();
        rigidbody.angularVelocity = lastCorrectSnapData.RigidbodySnapData.AngularVelocity.ToVector3();
        abilityComponent.RollBack(lastCorrectSnapData.AbilityComponentSnapData);
        inputComponent.RollBack(errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
    }
    #endregion
}
