using GAS;
using InputSystemNameSpace;
using Unity.Physics;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorJumpAction",menuName = "GAS/Action/Charactor/Jump",order = 0)]
public class CharactorJumpAction : AbilityActionUnit {
    [Header("Animation Params")]
    [SerializeField] string AnimationParam_JumpUp;
    [Header("Sommth Config")]
    [SerializeField] float JumpHorizontalSpeedSmoothTime = 0.1f;
    [Header("Attribute Config")]
    [SerializeField] string JumpHeightAttributeName;
    [SerializeField] string JumpHorizontalImpulseAttributeName;
    [SerializeField] string InAirSpeedAttributeName;

    float YVelocityLastFrame;

    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        if(YVelocityLastFrame >=0 && transformController.Velocity.y < 0) {
            return TaskStatus.Suceeded;
        } else { 
            var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
            var inputDir = inputQueue.PeekTail().MoveInput;
            var aimDir = inputQueue.PeekTail().AimDirection;
            var moveDir = new Vector2();
            Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
            moveDir = rotation * inputDir;
            var jumpHorizontalSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[InAirSpeedAttributeName];
            Vector3 horizontalVelocity = jumpHorizontalSpeedAttribute.Float() * new Vector3(moveDir.x,0,moveDir.y).normalized;
            transformController.VelocityTo(new Vector3(horizontalVelocity.x,transformController.Velocity.y,horizontalVelocity.z),JumpHorizontalSpeedSmoothTime);

            YVelocityLastFrame = transformController.Velocity.y;
            return TaskStatus.Running;
        }
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_JumpUp,false);
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_JumpUp,true);

        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        var inputDir = inputQueue.PeekTail().MoveInput;
        var aimDir = inputQueue.PeekTail().AimDirection;
        var moveDir = new Vector2();
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        var jumpHeight = abilityRuntimeContext.AbilityComponentContext.AttributeSet[JumpHeightAttributeName];
        var jumpHorizontalImpulse = abilityRuntimeContext.AbilityComponentContext.AttributeSet[JumpHorizontalImpulseAttributeName];
        Vector3 verticalImpulse = jumpHeight.Float() * Vector3.up;
        Vector3 horizontalImpulse = jumpHorizontalImpulse.Float() * new Vector3(moveDir.x,0,moveDir.y).normalized;
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        transformController.AddForce(verticalImpulse + horizontalImpulse,ForceMode.VelocityChange);
        YVelocityLastFrame = transformController.Velocity.y; 
    }
}