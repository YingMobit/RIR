using Drive;
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
        public AbilityComponentSnapData AbilityComponentSnapData;

        public SnapData(int localizedLogicFrameCount,
                        TranformSnapData tranformSnapData,
                        AbilityComponentSnapData abilityComponentSnapData) {
            LocalizedLogicFrameCount = localizedLogicFrameCount;
            TranformSnapData = tranformSnapData;
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
    ITransformController transform; 
    AbilityComponent abilityComponent;
    InputComponent inputComponent;
    #endregion

    #region Component Overrides
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.RollBackComponent;
    
    public override void OnAttach(World world, Entity entity) {
        gameObject = world.GetGameObject(entity);
        transform = gameObject.GetComponent<AbilityComponentContextBuilder>().Context.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        world.GetComponentOnEntity(entity, ComponentTypeEnum.AbilityComponent, out var abilityComp);
        abilityComponent = abilityComp as AbilityComponent;
        world.GetComponentOnEntity(entity, ComponentTypeEnum.InputComponent, out var inputComp);
        inputComponent = inputComp as InputComponent;
        SnapShot(0);
    }

    public override void Reset(World world, Entity entity) {
        gameObject = null;
        transform = null;
        abilityComponent = null;
    }

    public override void OnDestroy() {
    }

    public override Component Clone() {
        return new RollBackComponent();
    }
    #endregion

    #region API
    public void SnapShot(int logicFrameCount) { 
        var tranformSnapData = new TranformSnapData(
            transform.LogicPosition.ToLVector3(),
            transform.LogicRotation,
            transform.LogicScale.ToLVector3()
        );
        var abilityComponentSnapData = new AbilityComponentSnapData();
        var snapData = new SnapData(
            logicFrameCount,
            tranformSnapData,
            abilityComponentSnapData
        );

        cachedSnapShots.PushBack(snapData);
    }

    public void RollBackState(int errorStartLocalizedLogicFrameCount, int currentLocalizedLogicFrameCount) { 
        int count = currentLocalizedLogicFrameCount - errorStartLocalizedLogicFrameCount + 1;
        cachedSnapShots.PopBackN(count);
        var lastCorrectSnapData = cachedSnapShots.PeekBack();
        Debug.Log($"[RollBackComponent] RollBack from Frame {errorStartLocalizedLogicFrameCount} to Frame {currentLocalizedLogicFrameCount}\n" +
            $"Frame:{errorStartLocalizedLogicFrameCount}SnapShot:{{position:{lastCorrectSnapData.TranformSnapData.Position}\n" +
            $"rotation:{lastCorrectSnapData.TranformSnapData.Rotation}\n" +
            $"scale:{lastCorrectSnapData.TranformSnapData.Scale}}}\n");
        
        int trackBackFrameCount = (int)(count * (1f / Time.deltaTime) / (float)FixedRateScheduler._cfg.RateHz) / 2;
        transform.MoveToSmoothly(lastCorrectSnapData.TranformSnapData.Position.ToVector3(),trackBackFrameCount);
        transform.RotateToSmoothly(lastCorrectSnapData.TranformSnapData.Rotation,trackBackFrameCount);
        transform.ScaleToSmoothly(lastCorrectSnapData.TranformSnapData.Scale.ToVector3(),trackBackFrameCount);

        abilityComponent.RollBack(lastCorrectSnapData.AbilityComponentSnapData);
        inputComponent.RollBack(errorStartLocalizedLogicFrameCount, currentLocalizedLogicFrameCount);
    }
    #endregion
}
