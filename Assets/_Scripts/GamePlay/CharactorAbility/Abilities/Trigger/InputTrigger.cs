using GAS;
using InputSystemNameSpace;
using UnityEngine;
using Utility;

[CreateAssetMenu(fileName = "InputTrigger",menuName = "GAS/Triggers/Charactor/InputTrigger",order =0)]
public class InputTrigger : AbilityTriggerUnit {
    [SerializeField] InputTypeEnum inputType;

    public override AbilityTriggerUnit Clone() {
        var res = new InputTrigger();
        res.inputType = inputType;
        return res;
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        var input = abilityComponentContext.GlobalBlacboard.Get<DeQueue<FrameInputData>>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        input.TryPeekBack(out var inputData);
        if(inputData.KeyCodeinputs.HasAnyInputType(inputType)) {
            return TaskStatus.Suceeded;
        }
        return TaskStatus.Failed;
    }
}