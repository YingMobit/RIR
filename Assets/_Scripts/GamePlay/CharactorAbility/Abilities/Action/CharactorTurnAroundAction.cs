using GAS;
using InputSystemNameSpace;
using Lockstep.Math;
using UnityEngine;
using Utility;

[CreateAssetMenu(fileName = "CharactorTurnAroundAction",menuName = "GAS/Action/Charactor/TurnAround",order = 0)]
public class CharactorTurnAroundAction : AbilityActionUnit {
    [Header("Turn Param")]
    [SerializeField] int TurnSmoothFrameCount;
    
    
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);    
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<DeQueue<FrameInputData>>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        inputQueue.TryPeekBack(out var frameInputData);
        var dir = frameInputData.AimDirection;
        dir.y = LFloat.zero;
        dir.Normalize();
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        transformController.RotateToSmoothly(dir.ToVector3(),TurnSmoothFrameCount);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        return TaskStatus.Suceeded;    
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        
    }
}