using GAS;
using InputSystemNameSpace;
using Lockstep.Math;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorTurnAroundAction",menuName = "GAS/Action/Charactor/TurnAround",order = 0)]
public class CharactorTurnAroundAction : AbilityActionUnit {
    [Header("Turn Param")]
    [SerializeField] float TurnSmoothTime;
    
    
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);    
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        var dir = inputQueue.PeekTail().AimDirection;
        dir.y = LFloat.zero;
        dir.Normalize();
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        transformController.RotateTo(dir.ToVector3(),TurnSmoothTime);
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