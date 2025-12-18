using GAS;
using InputSystemNameSpace;
using Lockstep.Math;
using LockStepLMath;
using UnityEngine;
using Utility;

[CreateAssetMenu(fileName = "CharactorRunAction",menuName = "GAS/Action/Charactor/Run",order = 0)]
public class CharactorRunAction : AbilityActionUnit {
    [Header("Animator config")]
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [SerializeField] string AnimationParam_Run;
    [Header("SmoothConfig")]
    [SerializeField] int PositionSmoothFrameCount;
    [Header("Attribute Config")]
    [SerializeField] int RunSpeedAttributeID;
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<DeQueue<FrameInputData>>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        inputQueue.TryPeekBack(out var frameInputData);
        var inputDir = frameInputData.MoveInput;
        var aimDir = frameInputData.AimDirection;
        var moveDir = new LVector2();
        LQuaternion rotation = LQuaternion.FromToRotation(LVector2.up,new LVector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetFloatSmooth(AnimationParam_Dir_x,inputDir.x.ToFloat(),PositionSmoothFrameCount);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,inputDir.y.ToFloat(),PositionSmoothFrameCount);

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var runSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[RunSpeedAttributeID];
        var velocity = new Vector2(moveDir.x.ToFloat() * runSpeedAttribute.Float(),moveDir.y.ToFloat() * runSpeedAttribute.Float());
        var deltaTime = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<float>(AbilitySystem.DELTATIMEID_IN_GLOBALBLACKBORAD);
        var newPos = transformController.LogicPosition + new Vector3(velocity.x,0,velocity.y) * deltaTime;

        transformController.MoveToSmoothly(newPos,PositionSmoothFrameCount);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;

        //设置完平滑参数就可以退出了
        animationController.SetFloatSmooth(AnimationParam_Dir_x,0,PositionSmoothFrameCount);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,0,PositionSmoothFrameCount);
        animationController.SetBool(AnimationParam_Run,false);
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_Run,true);
    }
}