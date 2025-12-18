using GAS;
using InputSystemNameSpace;
using UnityEngine;
using Utility;

[CreateAssetMenu(fileName = "InputCheckAction",menuName = "GAS/Action/General/InputCheck",order = 0)]
public class InputCheckAction : AbilityActionUnit {
    [SerializeField] private InputTypeEnum InputToCheck;
    
    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var input = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<DeQueue<FrameInputData>>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        input.TryPeekBack(out var inputData);
        if(inputData.KeyCodeinputs.HasAnyInputType(InputToCheck)) {
            return TaskStatus.Suceeded;
        }
        return TaskStatus.Failed;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override AbilityBehaviorUnit Clone() {
        return new InputCheckAction() {InputToCheck = this.InputToCheck};
    }
}
